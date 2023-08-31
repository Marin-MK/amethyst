using System;
using NativeLibraryLoader;
using odl;

namespace amethyst;

public static class Amethyst
{
    public static bool Initialized => Info != null;

    internal static PathInfo Info;
    internal static NativeLibrary tfd;
    internal static bool Stopped = false;

    public static void Start(PathInfo Info, bool InitializeAudio = true, bool InitializeFilePickerLibrary = false)
    {
        if (Info == null) throw new Exception("PathInfo cannot be null.");
        Amethyst.Info = Info;
        if (InitializeFilePickerLibrary)
        {
            PathPlatformInfo platform = Info.GetPlatform(NativeLibrary.Platform);
            tfd = NativeLibrary.Load(platform.Get("tinyfiledialogs"));
            OpenFileDialog.FUNC_OpenFileDialog = tfd.GetFunction<OpenFileDialog.OpenFileDelegate>("tinyfd_openFileDialog");
            OpenFileDialog.FUNC_SelectFolderDialog = tfd.GetFunction<OpenFileDialog.SelectFolderDelegate>("tinyfd_selectFolderDialog");
            OpenFileDialog.FUNC_SaveFileDialog = tfd.GetFunction<OpenFileDialog.SaveFileDelegate>("tinyfd_saveFileDialog");
        }
        Graphics.Start(Info);
        if (InitializeAudio) Audio.Start(Info);
        ODL.FontResolver.AddPath("assets/fonts");
        
        if (ODL.OnMacOS) throw new PlatformNotSupportedException();
    }

    public static void Run(Action TickDelegate = null)
    {
        if (TickDelegate == null)
        {
            while (Graphics.CanUpdate())
            {
                if (Stopped) break;
                Graphics.Update();
            }
        }
        else
        {
            while (Graphics.CanUpdate())
            {
                if (Stopped) break;
                TickDelegate();
            }
        }
    }

    public static void Stop()
    {
        Stopped = true;
        Graphics.Stop();
        if (Audio.Initialized) Audio.Stop();
    }
}