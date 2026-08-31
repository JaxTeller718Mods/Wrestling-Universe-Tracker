using System;
using System.Runtime.InteropServices;

namespace WrestlingUniverse.Platform
{
    public static class WindowsImageFilePicker
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [StructLayout(LayoutKind.Sequential)]
        private struct OpenFileNameNative
        {
            public uint structSize;
            public IntPtr owner;
            public IntPtr instance;
            public IntPtr filter;
            public IntPtr customFilter;
            public uint maxCustomFilter;
            public uint filterIndex;
            public IntPtr file;
            public uint maxFile;
            public IntPtr fileTitle;
            public uint maxFileTitle;
            public IntPtr initialDirectory;
            public IntPtr title;
            public uint flags;
            public ushort fileOffset;
            public ushort fileExtension;
            public IntPtr defaultExtension;
            public IntPtr customData;
            public IntPtr hook;
            public IntPtr templateName;
            public IntPtr reservedPtr;
            public uint reservedInt;
            public uint flagsExtended;
        }

        [DllImport("Comdlg32.dll", EntryPoint = "GetOpenFileNameW", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetOpenFileNameNative(ref OpenFileNameNative dialog);

        [DllImport("Comdlg32.dll", EntryPoint = "GetSaveFileNameW", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSaveFileNameNative(ref OpenFileNameNative dialog);
#endif

        public static bool TryPickImage(out string path)
        {
            path = string.Empty;
#if UNITY_EDITOR_WIN
            path = UnityEditor.EditorUtility.OpenFilePanelWithFilters(
                "Select an image", string.Empty,
                new[] { "Image files", "png,jpg,jpeg,bmp", "All files", "*" });
            return !string.IsNullOrWhiteSpace(path);
#elif UNITY_STANDALONE_WIN
            return TryPickImageInWindowsPlayer(out path);
#else
            return false;
#endif
        }

        public static bool TryPickRosterPackage(out string path)
        {
            path = string.Empty;
#if UNITY_EDITOR_WIN
            path = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Import roster package", string.Empty,
                new[] { "Wrestling Universe roster", "wuroster", "All files", "*" });
            return !string.IsNullOrWhiteSpace(path);
#elif UNITY_STANDALONE_WIN
            return TryPickRosterPackageInWindowsPlayer(false, string.Empty, out path);
#else
            return false;
#endif
        }

        public static bool TryChooseRosterExportPath(string suggestedName, string initialDirectory, out string path)
        {
            path = string.Empty;
#if UNITY_EDITOR_WIN
            path = UnityEditor.EditorUtility.SaveFilePanel("Export roster package", initialDirectory, suggestedName, "wuroster");
            return !string.IsNullOrWhiteSpace(path);
#elif UNITY_STANDALONE_WIN
            if (!TryPickRosterPackageInWindowsPlayer(true, initialDirectory, out path)) return false;
            if (!path.EndsWith(".wuroster", StringComparison.OrdinalIgnoreCase)) path += ".wuroster";
            return true;
#else
            return false;
#endif
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private static bool TryPickRosterPackageInWindowsPlayer(bool save, string initialDirectory, out string path)
        {
            const int maxPathCharacters = 4096;
            path = string.Empty; var filter = IntPtr.Zero; var file = IntPtr.Zero; var title = IntPtr.Zero; var extension = IntPtr.Zero; var initial = IntPtr.Zero;
            try
            {
                filter = Marshal.StringToHGlobalUni("Wrestling Universe Roster\0*.wuroster\0All Files\0*.*\0\0");
                file = Marshal.AllocHGlobal(maxPathCharacters * sizeof(char));
                for (var offset = 0; offset < maxPathCharacters * sizeof(char); offset += sizeof(long)) Marshal.WriteInt64(file, offset, 0L);
                title = Marshal.StringToHGlobalUni(save ? "Export roster package" : "Import roster package");
                extension = Marshal.StringToHGlobalUni("wuroster");
                if (!string.IsNullOrEmpty(initialDirectory)) initial = Marshal.StringToHGlobalUni(initialDirectory);
                var dialog = new OpenFileNameNative { structSize = (uint)Marshal.SizeOf(typeof(OpenFileNameNative)), filter = filter,
                    filterIndex = 1, file = file, maxFile = maxPathCharacters, initialDirectory = initial, title = title, defaultExtension = extension,
                    flags = save ? 0x00000002u | 0x00000800u | 0x00000008u : 0x00001000u | 0x00000800u | 0x00000008u };
                var accepted = save ? GetSaveFileNameNative(ref dialog) : GetOpenFileNameNative(ref dialog);
                if (!accepted) return false;
                path = Marshal.PtrToStringUni(file) ?? string.Empty; return !string.IsNullOrWhiteSpace(path);
            }
            finally
            {
                if (filter != IntPtr.Zero) Marshal.FreeHGlobal(filter); if (file != IntPtr.Zero) Marshal.FreeHGlobal(file);
                if (title != IntPtr.Zero) Marshal.FreeHGlobal(title); if (extension != IntPtr.Zero) Marshal.FreeHGlobal(extension);
                if (initial != IntPtr.Zero) Marshal.FreeHGlobal(initial);
            }
        }

        private static bool TryPickImageInWindowsPlayer(out string path)
        {
            const int maxPathCharacters = 4096;
            path = string.Empty;
            var filter = IntPtr.Zero;
            var file = IntPtr.Zero;
            var title = IntPtr.Zero;
            var extension = IntPtr.Zero;

            try
            {
                filter = Marshal.StringToHGlobalUni(
                    "Image Files\0*.png;*.jpg;*.jpeg;*.bmp\0PNG Files\0*.png\0JPEG Files\0*.jpg;*.jpeg\0All Files\0*.*\0\0");
                file = Marshal.AllocHGlobal(maxPathCharacters * sizeof(char));
                for (var offset = 0; offset < maxPathCharacters * sizeof(char); offset += sizeof(long))
                    Marshal.WriteInt64(file, offset, 0L);
                title = Marshal.StringToHGlobalUni("Select an image");
                extension = Marshal.StringToHGlobalUni("png");

                var dialog = new OpenFileNameNative
                {
                    structSize = (uint)Marshal.SizeOf(typeof(OpenFileNameNative)),
                    filter = filter,
                    filterIndex = 1,
                    file = file,
                    maxFile = maxPathCharacters,
                    title = title,
                    defaultExtension = extension,
                    flags = 0x00001000 | 0x00000800 | 0x00000008
                };

                if (!GetOpenFileNameNative(ref dialog)) return false;
                path = Marshal.PtrToStringUni(file) ?? string.Empty;
                return !string.IsNullOrWhiteSpace(path);
            }
            finally
            {
                if (filter != IntPtr.Zero) Marshal.FreeHGlobal(filter);
                if (file != IntPtr.Zero) Marshal.FreeHGlobal(file);
                if (title != IntPtr.Zero) Marshal.FreeHGlobal(title);
                if (extension != IntPtr.Zero) Marshal.FreeHGlobal(extension);
            }
        }
#endif
    }
}
