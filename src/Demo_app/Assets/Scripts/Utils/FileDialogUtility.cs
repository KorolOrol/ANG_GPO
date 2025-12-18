using System;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils
{
    /// <summary>
    /// Кроссплатформенная утилита для работы с файловыми диалогами.
    /// Работает в редакторе и в runtime билдах на Windows и Linux.
    /// </summary>
    public static class FileDialogUtility
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetSaveFileName(ref OpenFileName ofn);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct OpenFileName
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public string lpstrFile;
            public int nMaxFile;
            public string lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int flagsEx;
        }

        private const int OFN_OVERWRITEPROMPT = 0x00000002;
        private const int OFN_NOCHANGEDIR = 0x00000008;
        private const int OFN_PATHMUSTEXIST = 0x00000800;
        private const int OFN_FILEMUSTEXIST = 0x00001000;
        private const int OFN_EXPLORER = 0x00080000;
#endif

#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
        /// <summary>
        /// Открывает диалог через zenity на Linux.
        /// </summary>
        private static string OpenZenityDialog(string args)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "zenity",
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return process.ExitCode == 0 ? output : string.Empty;
            }
            catch (Exception e)
            {
                Debug.LogError($"Zenity dialog failed: {e.Message}");
                return string.Empty;
            }
        }
#endif

        /// <summary>
        /// Открывает диалог выбора файла для открытия.
        /// </summary>
        /// <param name="title">Заголовок диалога.</param>
        /// <param name="directory">Начальная директория.</param>
        /// <param name="extension">Расширение файла (без точки).</param>
        /// <returns>Путь к выбранному файлу или пустая строка при отмене.</returns>
        public static string OpenFilePanel(string title, string directory, string extension)
        {
#if UNITY_EDITOR
            return EditorUtility.OpenFilePanel(title, directory, extension);
#elif UNITY_STANDALONE_WIN
            var ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            ofn.lpstrFilter = string.IsNullOrEmpty(extension) 
                ? "All Files\0*.*\0" 
                : $"{extension.ToUpper()} Files\0*.{extension}\0All Files\0*.*\0";
            ofn.lpstrFile = new string(new char[256]);
            ofn.nMaxFile = ofn.lpstrFile.Length;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
            ofn.lpstrInitialDir = directory;
            ofn.lpstrTitle = title;
            ofn.lpstrDefExt = extension;
            ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST | OFN_EXPLORER | OFN_NOCHANGEDIR;

            if (GetOpenFileName(ref ofn))
            {
                return ofn.lpstrFile;
            }
            return string.Empty;
#elif UNITY_STANDALONE_LINUX
            string filter = string.IsNullOrEmpty(extension) ? "" : $"--file-filter='*.{extension}' --file-filter='*'";
            string args = $"--file-selection --title=\"{title}\" {filter}";
            return OpenZenityDialog(args);
#else
            Debug.LogWarning("File dialogs are not supported on this platform.");
            return string.Empty;
#endif
        }

        /// <summary>
        /// Открывает диалог сохранения файла.
        /// </summary>
        /// <param name="title">Заголовок диалога.</param>
        /// <param name="directory">Начальная директория.</param>
        /// <param name="defaultName">Имя файла по умолчанию.</param>
        /// <param name="extension">Расширение файла (без точки).</param>
        /// <returns>Путь для сохранения или пустая строка при отмене.</returns>
        public static string SaveFilePanel(string title, string directory, string defaultName, string extension)
        {
#if UNITY_EDITOR
            return EditorUtility.SaveFilePanel(title, directory, defaultName, extension);
#elif UNITY_STANDALONE_WIN
            var ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            ofn.lpstrFilter = string.IsNullOrEmpty(extension) 
                ? "All Files\0*.*\0" 
                : $"{extension.ToUpper()} Files\0*.{extension}\0All Files\0*.*\0";
            string initialFile = string.IsNullOrEmpty(extension) ? defaultName : $"{defaultName}.{extension}";
            ofn.lpstrFile = initialFile + new string(new char[256 - initialFile.Length]);
            ofn.nMaxFile = 256;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
            ofn.lpstrInitialDir = directory;
            ofn.lpstrTitle = title;
            ofn.lpstrDefExt = extension;
            ofn.Flags = OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST | OFN_EXPLORER | OFN_NOCHANGEDIR;

            if (GetSaveFileName(ref ofn))
            {
                return ofn.lpstrFile;
            }
            return string.Empty;
#elif UNITY_STANDALONE_LINUX
            string filter = string.IsNullOrEmpty(extension) ? "" : $"--file-filter='*.{extension}' --file-filter='*'";
            string filename = string.IsNullOrEmpty(extension) ? defaultName : $"{defaultName}.{extension}";
            string args = $"--file-selection --save --confirm-overwrite --title=\"{title}\" --filename=\"{filename}\" {filter}";
            return OpenZenityDialog(args);
#else
            Debug.LogWarning("File dialogs are not supported on this platform.");
            return string.Empty;
#endif
        }

        /// <summary>
        /// Показывает информационный диалог.
        /// </summary>
        /// <param name="title">Заголовок диалога.</param>
        /// <param name="message">Сообщение.</param>
        /// <param name="ok">Текст кнопки OK.</param>
        public static void DisplayDialog(string title, string message, string ok)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog(title, message, ok);
#elif UNITY_STANDALONE_WIN
            MessageBox(IntPtr.Zero, message, title, 0);
#elif UNITY_STANDALONE_LINUX
            OpenZenityDialog($"--info --title=\"{title}\" --text=\"{message}\"");
#else
            Debug.Log($"[{title}] {message}");
#endif
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
#endif
    }
}

