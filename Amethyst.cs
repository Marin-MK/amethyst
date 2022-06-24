using System;
using NativeLibraryLoader;
using odl;

namespace amethyst;

public static class Amethyst
{
    public static bool Initialized => Info != null;

    internal static PathInfo Info;
    internal static NativeLibrary tfd;

    public static void Start(PathInfo Info, bool InitializeAudio = true, bool InitializeFilePickerLibrary = false)
    {
        if (Info == null) throw new Exception("PathInfo cannot be null.");
        Amethyst.Info = Info;
        if (InitializeFilePickerLibrary)
        {
            PathPlatformInfo platform = Info.GetPlatform(NativeLibrary.Platform);
            tfd = NativeLibrary.Load(platform.Get("tinyfiledialogs"));
            OpenFileDialog.FUNC_OpenFileDialog = tfd.GetFunction<OpenFileDialog.OFDDelegate>("tinyfd_openFileDialog");
            OpenFileDialog.FUNC_SelectFolderDialog = tfd.GetFunction<OpenFileDialog.SFDDelegate>("tinyfd_selectFolderDialog");
        }
        Graphics.Start(Info);
        if (InitializeAudio) Audio.Start(Info);
        Font.AddFontPath("assets/fonts");
        if (Graphics.Platform == odl.Platform.Windows) Font.AddFontPath("C:/Windows/Fonts");
        else if (Graphics.Platform == odl.Platform.Linux)
        {
            Font.AddFontPath("/usr/share/fonts");
            Font.AddFontPath("/usr/local/share/fonts");
            Font.AddFontPath("~/.fonts");
        }
        else if (Graphics.Platform == odl.Platform.MacOS)
        {
            throw new PlatformNotSupportedException();
        }
    }

    public static void Run(Action TickDelegate = null)
    {
        if (TickDelegate == null)
        {
            while (Graphics.CanUpdate())
            {
                Graphics.Update();
            }
        }
        else
        {
            while (Graphics.CanUpdate())
            {
                TickDelegate();
            }
        }
    }

    public static void Stop()
    {
        Graphics.Stop();
        if (Audio.Initialized) Audio.Stop();
    }
}