using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NativeLibraryLoader;

namespace amethyst;

public class OpenFileDialog
{
    NativeLibrary tfd;

    internal delegate IntPtr OFDDelegate(string Title, string DefaultPath, int NumFilePatterns, string[] FilterPatterns, string SingleFilterDescription, int AllowMultiple);
    internal delegate IntPtr SFDDelegate(string Title, string DefaultPath);
    internal OFDDelegate FUNC_OpenFileDialog;
    internal SFDDelegate FUNC_SelectFolderDialog;

    public string Title;
    public string DefaultFolder;
    public FileFilter FileFilter;

    public OpenFileDialog()
    {
        if (tfd == null)
        {
            if (odl.Graphics.Platform == odl.Platform.Windows)
            {
                tfd = NativeLibrary.Load("./lib/windows/tinyfiledialogs64.dll");
            }
            else if (odl.Graphics.Platform == odl.Platform.Linux)
            {
                tfd = NativeLibrary.Load("./lib/linux/tinyfiledialogs64.so");
            }
            else if (odl.Graphics.Platform == odl.Platform.MacOS)
            {
                throw new Exception("MacOS support has not yet been implemented.");
            }
            else
            {
                throw new Exception("Failed to detect platform.");
            }
            FUNC_OpenFileDialog = tfd.GetFunction<OFDDelegate>("tinyfd_openFileDialog");
            FUNC_SelectFolderDialog = tfd.GetFunction<SFDDelegate>("tinyfd_selectFolderDialog");
        }
        this.Title = "Open File";
        this.DefaultFolder = Directory.GetCurrentDirectory();
    }

    public void SetTitle(string Title)
    {
        this.Title = Title;
    }

    public void SetInitialDirectory(string Dir)
    {
        while (Dir.Contains('\\')) Dir = Dir.Replace('\\', '/');
        this.DefaultFolder = Dir;
        if (!Dir.EndsWith('/')) this.DefaultFolder += '/';
    }

    public void SetFilter(FileFilter FileFilter)
    {
        this.FileFilter = FileFilter;
    }

    public string ChooseFile()
    {
        IntPtr ptr = FUNC_OpenFileDialog(
            this.Title,
            this.DefaultFolder,
            this.FileFilter == null ? 0 : this.FileFilter.Extensions.Count,
            this.FileFilter.Extensions.Select(e => "*." + e).ToArray(),
            this.FileFilter.ToString(),
            0
        );
        if (ptr != IntPtr.Zero)
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
        IntPtr ptr = FUNC_OpenFileDialog(
            this.Title,
            this.DefaultFolder,
            this.FileFilter == null ? 0 : this.FileFilter.Extensions.Count,
            this.FileFilter.Extensions.Select(e => "*." + e).ToArray(),
            this.FileFilter.ToString(),
            1
        );
        if (ptr != IntPtr.Zero)
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
        IntPtr ptr = FUNC_SelectFolderDialog(this.Title, this.DefaultFolder);
        if (ptr != IntPtr.Zero)
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
}

public class FileFilter
{
    public string Name;
    public List<string> Extensions;

    public FileFilter(string Name, params string[] ext)
    {
        this.Name = Name;
        this.Extensions = new List<string>(ext);
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
