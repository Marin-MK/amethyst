using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NativeLibraryLoader;

namespace amethyst;

public class OpenFileDialog
{
    NativeLibrary tfd;

    internal delegate nint OpenFileDelegate(string Title, string DefaultPath, int NumFilePatterns, string[] FilterPatterns, string SingleFilterDescription, int AllowMultiple);
    internal delegate nint SelectFolderDelegate(string Title, string DefaultPath);
    internal delegate nint SaveFileDelegate(string Title, string DefaultPath, int NumFilePatterns, string[] FilterPatterns, string SingleFilterDescription);
    internal static OpenFileDelegate FUNC_OpenFileDialog;
    internal static SelectFolderDelegate FUNC_SelectFolderDialog;
    internal static SaveFileDelegate FUNC_SaveFileDialog;

    public string Title;
    public string DefaultFolder;
    public FileFilter FileFilter;

    public OpenFileDialog()
    {
        if (Amethyst.tfd == null) throw new Exception("Amethyst was not initialized with tinyfiledialogs enabled.");
        Title = "Open File";
        DefaultFolder = Directory.GetCurrentDirectory();
    }

    public void SetTitle(string Title)
    {
        this.Title = Title;
    }

    public void SetInitialDirectory(string Dir)
    {
        while (Dir.Contains('\\')) Dir = Dir.Replace('\\', '/');
        DefaultFolder = Dir;
        if (!Dir.EndsWith('/')) DefaultFolder += '/';
    }

    public void SetFilter(FileFilter FileFilter)
    {
        this.FileFilter = FileFilter;
    }

    public string ChooseFile()
    {
        nint ptr = FUNC_OpenFileDialog(
            Title,
            DefaultFolder,
            FileFilter == null ? 0 : FileFilter.Extensions.Count,
            FileFilter.Extensions.Select(e => "*." + e).ToArray(),
            FileFilter.ToString(),
            0
        );
        if (ptr != nint.Zero)
        {
            string File = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
            if (!string.IsNullOrEmpty(File))
            {
                while (File.Contains('\\')) File = File.Replace('\\', '/');
                return File;
            }
        }
        return null;
    }

    public List<string> ChooseFiles()
    {
        nint ptr = FUNC_OpenFileDialog(
            Title,
            DefaultFolder,
            FileFilter == null ? 0 : FileFilter.Extensions.Count,
            FileFilter.Extensions.Select(e => "*." + e).ToArray(),
            FileFilter.ToString(),
            1
        );
        if (ptr != nint.Zero)
        {
            string Files = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
            if (!string.IsNullOrEmpty(Files))
            {
                while (Files.Contains("\\")) Files = Files.Replace('\\', '/');
                if (Files.Contains('|'))
                    return Files.Split('|').ToList();
                else return new List<string>() { Files };
            }
        }
        return null;
    }

    public string ChooseFolder()
    {
        nint ptr = FUNC_SelectFolderDialog(Title, DefaultFolder);
        if (ptr != nint.Zero)
        {
            string Folder = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
            if (!string.IsNullOrEmpty(Folder))
            {
                while (Folder.Contains('\\')) Folder = Folder.Replace('\\', '/');
                return Folder;
            }
        }
        return null;
    }

    public string SaveFile()
    {
        nint ptr = FUNC_SaveFileDialog(
            Title,
            DefaultFolder,
            FileFilter == null ? 0 : FileFilter.Extensions.Count,
            FileFilter.Extensions.Select(e => "*." + e).ToArray(),
            FileFilter.ToString()
        );
        if (ptr != nint.Zero)
        {
            string File = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
            if (!string.IsNullOrEmpty(File))
            {
                File = File.Replace('\\', '/');
                return File;
            }
        }
        return null;
    }
}

public class FileFilter
{
    public string Name;
    public List<string> Extensions;

    public FileFilter(string Name, params string[] ext)
    {
        this.Name = Name;
        Extensions = new List<string>(ext);
    }

    public override string ToString()
    {
        string ext = "";
        for (int i = 0; i < Extensions.Count; i++)
        {
            ext += "*." + Extensions[i];
            if (i != Extensions.Count - 1) ext += ";";
        }
        return $"{Name} ({ext})\0{ext}\0";
    }
}
