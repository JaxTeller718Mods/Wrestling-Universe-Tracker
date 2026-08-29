using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace WrestlingUniverse.Persistence
{
    public static class UniverseImageStorage
    {
        private const long MaximumImageBytes = 20 * 1024 * 1024;

        public static string Import(string universeId, string sourcePath, string imageRole)
        {
            if (string.IsNullOrWhiteSpace(sourcePath)) return string.Empty;
            if (!File.Exists(sourcePath)) throw new FileNotFoundException("The selected image no longer exists.", sourcePath);

            var source = new FileInfo(sourcePath);
            if (source.Length > MaximumImageBytes)
                throw new InvalidOperationException("Images must be 20 MB or smaller.");

            var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".bmp")
                throw new InvalidOperationException("Choose a PNG, JPG, JPEG, or BMP image.");

            var directory = Path.Combine(Application.persistentDataPath, "UniverseImages", universeId);
            Directory.CreateDirectory(directory);
            foreach (var oldPath in Directory.GetFiles(directory, imageRole + ".*"))
                if (!string.Equals(oldPath, Path.Combine(directory, imageRole + extension), StringComparison.OrdinalIgnoreCase))
                    File.Delete(oldPath);
            var destination = Path.Combine(directory, imageRole + extension);
            File.Copy(sourcePath, destination, true);
            return destination;
        }

        public static string ImportBytes(string universeId, byte[] data, string extension, string imageRole)
        {
            if (data == null || data.Length == 0) return string.Empty;
            if (data.LongLength > MaximumImageBytes) throw new InvalidOperationException("Images must be 20 MB or smaller.");
            extension = (extension ?? string.Empty).ToLowerInvariant();
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".bmp")
                throw new InvalidOperationException("Roster packages may only contain PNG, JPG, JPEG, or BMP images.");
            var directory = Path.Combine(Application.persistentDataPath, "UniverseImages", universeId);
            Directory.CreateDirectory(directory);
            var destination = Path.Combine(directory, imageRole + extension);
            File.WriteAllBytes(destination, data);
            return destination;
        }

        public static Texture2D LoadTexture(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            var data = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.name = Path.GetFileNameWithoutExtension(path);
            if (texture.LoadImage(data)) return texture;
            UnityEngine.Object.Destroy(texture);
            return null;
        }
    }

    [Serializable]
    public sealed class RosterTransferPackage
    {
        public int version = 1;
        public string exportedUtc;
        public List<RosterTransferWrestler> wrestlers = new List<RosterTransferWrestler>();
        public List<RosterTransferTeam> teams = new List<RosterTransferTeam>();
    }

    [Serializable]
    public sealed class RosterTransferWrestler
    {
        public string sourceId;
        public string name;
        public string disposition;
        public string gender;
        public string tier;
        public int overall;
        public string imageExtension;
        public string imageBase64;
    }

    [Serializable]
    public sealed class RosterTransferTeam
    {
        public string sourceId;
        public string name;
        public string disposition;
        public List<string> memberSourceIds = new List<string>();
        public string imageExtension;
        public string imageBase64;
    }

    public static class RosterTransferService
    {
        private const long MaximumPackageBytes = 1024L * 1024L * 1024L;

        public static void Export(string path, List<UI.WrestlerRecord> wrestlers, List<UI.TeamRecord> teams)
        {
            var package = new RosterTransferPackage { exportedUtc = DateTime.UtcNow.ToString("O") };
            foreach (var wrestler in wrestlers)
            {
                var item = new RosterTransferWrestler { sourceId = wrestler.id, name = wrestler.name, disposition = wrestler.disposition,
                    gender = wrestler.gender, tier = wrestler.tier, overall = wrestler.overall };
                ReadImage(wrestler.photoPath, out item.imageExtension, out item.imageBase64); package.wrestlers.Add(item);
            }
            foreach (var team in teams)
            {
                var item = new RosterTransferTeam { sourceId = team.id, name = team.name, disposition = team.disposition,
                    memberSourceIds = new List<string>(team.memberIds) };
                ReadImage(team.photoPath, out item.imageExtension, out item.imageBase64); package.teams.Add(item);
            }
            var directory = Path.GetDirectoryName(path); if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var gzip = new GZipStream(file, System.IO.Compression.CompressionLevel.Optimal))
            using (var writer = new StreamWriter(gzip, new UTF8Encoding(false))) writer.Write(JsonUtility.ToJson(package));
        }

        public static RosterTransferPackage Import(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("The roster package no longer exists.", path);
            if (new FileInfo(path).Length > MaximumPackageBytes) throw new InvalidOperationException("The roster package is too large.");
            string json;
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var gzip = new GZipStream(file, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzip, Encoding.UTF8)) json = reader.ReadToEnd();
            var package = JsonUtility.FromJson<RosterTransferPackage>(json);
            if (package == null || package.version != 1) throw new InvalidOperationException("This roster package version is not supported.");
            if (package.wrestlers == null) package.wrestlers = new List<RosterTransferWrestler>();
            if (package.teams == null) package.teams = new List<RosterTransferTeam>();
            if (package.wrestlers.Count > 10000 || package.teams.Count > 10000) throw new InvalidOperationException("The roster package contains too many records.");
            return package;
        }

        public static byte[] DecodeImage(string imageBase64)
        {
            if (string.IsNullOrEmpty(imageBase64)) return null;
            var data = Convert.FromBase64String(imageBase64);
            if (data.LongLength > 20L * 1024L * 1024L) throw new InvalidOperationException("A package image exceeds the 20 MB limit.");
            return data;
        }

        private static void ReadImage(string path, out string extension, out string base64)
        {
            extension = string.Empty; base64 = string.Empty;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            var info = new FileInfo(path); if (info.Length > 20L * 1024L * 1024L) return;
            extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".bmp") { extension = string.Empty; return; }
            base64 = Convert.ToBase64String(File.ReadAllBytes(path));
        }
    }
}
