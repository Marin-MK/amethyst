using odl;
using NativeLibraryLoader;
using System.IO;
using amethyst.Windows;

namespace amethyst;

public class Program
{
    public static void Main(params string[] Args)
    {
        InitializeGraphics();
        InitializeAudio();
        //BorderlessWindow Window = new BorderlessWindow();
        //Window.Show();

        UIWindow Window = new UIWindow();

        Window.SetBackgroundColor(SystemColors.WindowBackground);

        /*GroupBox GroupBox1 = new GroupBox(Window.UI.Container);
        GroupBox1.SetPosition(64, 64);
        GroupBox1.SetSize(400, 400);

        Label Label1 = new Label(GroupBox1);
        Label1.SetPosition(16, 21);
        Label1.SetText("Hello world! This is a standard Windows UI label.");

        Button Button1 = new Button(GroupBox1);
        Button1.SetPosition(16, 36);
        Button1.SetText("Start task");
        Button1.OnPressed += delegate (BaseEventArgs e)
        {
            Console.WriteLine($"Clicked button 1!");
            Audio.BGMPlay("D:/Dropbox/Pokemon Jam/Audio/BGM/Battle trainer.ogg", 50, 4);
        };

        Button Button2 = new Button(GroupBox1);
        Button2.SetPosition(16, Button1.Position.Y + Button1.Size.Height + 4);
        Button2.SetText("End task");
        Button2.OnPressed += delegate (BaseEventArgs e) { Console.WriteLine("Clicked button 2!"); };

        CheckBox CheckBox1 = new CheckBox(GroupBox1);
        CheckBox1.SetPosition(16, Button2.Position.Y + Button2.Size.Height + 4);
        CheckBox1.OnCheckChanged += delegate (BaseEventArgs e) { Console.WriteLine("Clicked checkbox 1!"); };

        TabView TabView1 = new TabView(GroupBox1);
        TabView1.SetPosition(130, 46);
        TabView1.SetSize(200, 200);
        TabView1.CreateTab("Autotiles");
        TabView1.CreateTab("Tilesets");
        TabView1.OnTabChanged += delegate (BaseEventArgs e) { Console.WriteLine("Changed tab!"); };

        ListBox ListBox1 = new ListBox(GroupBox1);
        ListBox1.SetPosition(15, CheckBox1.Position.Y + CheckBox1.Size.Height + 10);
        ListBox1.SetSize(100, 200);
        ListBox1.SetItems(new List<ListItem>()
            {
                new ListItem("Alpha"),
                new ListItem("Beta"),
                new ListItem("Gamma"),
                new ListItem("Delta"),
                new ListItem("Epsilon"),
                new ListItem("Zeta"),
                new ListItem("Etha"),
                new ListItem("Iota"),
                new ListItem("Kappa"),
                new ListItem("Labda"),
                new ListItem("Mu"),
                new ListItem("Nu"),
                new ListItem("Xi"),
                new ListItem("Omicron"),
                new ListItem("Pi"),
                new ListItem("Rho"),
                new ListItem("Sigma"),
                new ListItem("Tau"),
                new ListItem("Upsilon"),
                new ListItem("Phi"),
                new ListItem("Chi"),
                new ListItem("Psi"),
                new ListItem("Omega")
            });
        ListBox1.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            Console.WriteLine($"Selection: {ListBox1.SelectedIndex}");
        };

        RadioBox RadioBox1 = new RadioBox(GroupBox1);
        RadioBox1.SetPosition(ListBox1.Position.X, ListBox1.Position.Y + ListBox1.Size.Height + 10);
        RadioBox1.SetText("Is shiny?");
        RadioBox1.OnCheckChanged += delegate (BaseEventArgs e)
        {
            if (RadioBox1.Checked) System.Console.WriteLine("Checked RadioBox 1");
        };
        RadioBox RadioBox2 = new RadioBox(GroupBox1);
        RadioBox2.SetPosition(RadioBox1.Position.X, RadioBox1.Position.Y + RadioBox1.Size.Height + 4);
        RadioBox2.SetText("Is modifiable?");
        RadioBox2.OnCheckChanged += delegate (BaseEventArgs e)
        {
            if (RadioBox2.Checked) System.Console.WriteLine("Checked RadioBox 2");
        };*/

        Window.Show();

        while (Graphics.CanUpdate())
        {
            Graphics.Update();
        }
        Graphics.Stop();
    }

    private static void InitializeGraphics()
    {
        PathPlatformInfo windows = new PathPlatformInfo(NativeLibraryLoader.Platform.Windows);
        windows.AddPath("libsdl2", "./lib/windows/SDL2.dll");
        windows.AddPath("libz", "./lib/windows/zlib1.dll");
        windows.AddPath("libsdl2_image", "./lib/windows/SDL2_image.dll");
        windows.AddPath("libpng", "./lib/windows/libpng16-16.dll");
        if (File.Exists("lib/windows/libjpeg-9.dll")) windows.AddPath("libjpeg", "./lib/windows/libjpeg-9.dll");
        windows.AddPath("libsdl2_ttf", "./lib/windows/SDL2_ttf.dll");
        windows.AddPath("libfreetype", "./lib/windows/libfreetype-6.dll");

        PathPlatformInfo linux = new PathPlatformInfo(NativeLibraryLoader.Platform.Linux);
        linux.AddPath("libsdl2", "./lib/linux/SDL2.so");
        linux.AddPath("libz", "./lib/linux/libz.so");
        linux.AddPath("libsdl2_image", "./lib/linux/SDL2_image.so");
        linux.AddPath("libpng", "./lib/linux/libpng16-16.so");
        if (File.Exists("lib/linux/libjpeg-9.so")) linux.AddPath("libjpeg", "./lib/linux/libjpeg-9.so");
        linux.AddPath("libsdl2_ttf", "./lib/linux/SDL2_ttf.so");
        linux.AddPath("libfreetype", "./lib/linux/libfreetype-6.so");

        Graphics.Start(PathInfo.Create(windows, linux));
    }

    private static void InitializeAudio()
    {
        PathPlatformInfo windows = new PathPlatformInfo(NativeLibraryLoader.Platform.Windows);
        windows.AddPath("bass", "./lib/windows/bass.dll");
        windows.AddPath("bass_fx", "./lib/windows/bass_fx.dll");
        windows.AddPath("bass_midi", "./lib/windows/bassmidi.dll");

        PathPlatformInfo linux = new PathPlatformInfo(NativeLibraryLoader.Platform.Linux);
        linux.AddPath("bass", "./lib/linux/libbass.so");
        linux.AddPath("bass_fx", "./lib/linux/libbass_fx.so");
        linux.AddPath("bass_midi", "./lib/linux/libbassmidi.so");

        Audio.Start(PathInfo.Create(windows, linux));
    }
}
