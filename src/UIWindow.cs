using System;
using System.Collections.Generic;
using odl;

namespace amethyst;

public class UIWindow : Window
{
    public static List<UIWindow> Windows = new List<UIWindow>();

    /// <summary>
    /// The main UI manager object.
    /// When used as widget parent, will force widget to be the size of the UI Manager.
    /// Use UI.Container instead if not desired.
    /// </summary>
    public UIManager UI { get; protected set; }
    /// <summary>
    /// The active Widget in the window. Used for higher priority popup windows that overlay the old active widget.
    /// </summary>
    public IContainer ActiveWidget;
    /// <summary>
    /// The list of former active widgets. Used to go back to an older active widget when the currently active widget closes.
    /// </summary>
    public List<IContainer> Widgets = new List<IContainer>();

    public UIWindow(bool Borderless = false, bool ShouldInitialize = true) : base()
    {
        if (!Amethyst.Initialized) throw new Exception("Amethyst was not initialized. Could not create window.");
        if (ShouldInitialize)
        {
            Initialize(true, false, Borderless);
            InitializeUI();
            Windows.Add(this);
            OnClosed += _ => Windows.Remove(this);
        }
    }

    public void InitializeUI()
    {
        InitializeUI(Color.BLACK);
    }
    public void InitializeUI(Color Color)
    {
        InitializeUI(Color.Red, Color.Green, Color.Blue, Color.Alpha);
    }
    public void InitializeUI(byte R, byte G, byte B, byte A = 255)
    {
        UI = new UIManager(this);
        UI.SizeChanged(new BaseEventArgs());
        UI.SetBackgroundColor(R, G, B, A);
        UI.Container = new Container(UI);
        UI.Container.SetSize(Width, Height);
    }

    /// <summary>
    /// Sets the main active widget.
    /// </summary>
    /// <param name="Widget">The widget to set as the main widget.</param>
    public void SetActiveWidget(IContainer Widget)
    {
        ActiveWidget = Widget;
        if (!Widgets.Contains(Widget)) Widgets.Add(Widget);
        if (Graphics.LastMouseEvent is MouseEventArgs) Graphics.LastMouseEvent.Handled = true;
    }

    /// <summary>
    /// Sets the opacity of the main window overlay.
    /// </summary>
    public void SetOverlayOpacity(byte Opacity)
    {
        TopSprite.Opacity = Opacity;
    }

    /// <summary>
    /// Sets the Z index of the main window overlay's viewport.
    /// </summary>
    public void SetOverlayZIndex(int Z)
    {
        TopViewport.Z = Z;
    }
}
