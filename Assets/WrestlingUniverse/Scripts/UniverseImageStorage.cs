using System;
using System.IO;
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
}
