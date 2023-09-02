using odl;

namespace amethyst;

public class BorderlessWindow : UIWindow
{
    public int TitleBarHeight = 33;
    public int ResizeMargin = 8;

    public bool MovingWindow = false;
    public bool ResizingWindow = false;

    protected bool ResizingHorizontal = false;
    protected bool ResizingVertical = false;

    protected Point AnchorPoint;

    public Rect NormalRect { get; protected set; }
    public new bool Maximized { get; protected set; } = false;

    protected Widget BorderWidget;
    protected Widget TitleBarWidget;
    protected Widget BackgroundWidget;
    protected WindowButton CloseButton;
    protected WindowButton MaximizeRestoreButton;
    protected WindowButton MinimizeButton;

    protected bool MovedWindow = false;

    public BorderlessWindow() : base(true)
    {
        BorderWidget = new Widget(UI.Container);
        BorderWidget.SetBackgroundColor(92, 90, 88);
        TitleBarWidget = new Widget(UI.Container);
        TitleBarWidget.SetBackgroundColor(76, 74, 72);
        TitleBarWidget.SetPosition(1, 1);
        BackgroundWidget = new Widget(UI.Container);
        BackgroundWidget.SetBackgroundColor(Color.BLACK);
        BackgroundWidget.SetPosition(1, TitleBarHeight);
        CloseButton = new WindowButton(UI.Container);
        CloseButton.WindowButtonType = WindowButtonType.Close;
        CloseButton.SetSize(45, 31);
        CloseButton.OnPressed += delegate (BaseEventArgs e)
        {
            Close();
            Dispose();
        };
        MaximizeRestoreButton = new WindowButton(UI.Container);
        MaximizeRestoreButton.WindowButtonType = WindowButtonType.MaximizeRestore;
        MaximizeRestoreButton.SetSize(45, 31);
        MaximizeRestoreButton.OnPressed += delegate (BaseEventArgs e)
        {
            if (Maximized) Restore(false);
            else Maximize();
        };
        MinimizeButton = new WindowButton(UI.Container);
        MinimizeButton.WindowButtonType = WindowButtonType.Minimize;
        MinimizeButton.SetSize(45, 31);
        MinimizeButton.OnPressed += delegate (BaseEventArgs e)
        {
            Minimize();
        };
        NormalRect = new Rect(X, Y, Width, Height);
        SizeChanged(new BaseEventArgs());
        SetResizable(false);
        OnTick += UpdateInput;
    }

    Sprite s;

    public void UpdateInput(BaseEventArgs e)
    {
        if (Input.Press(Keycode.DOWN))
        {
            s.Y -= 1;
        }
        else if (Input.Press(Keycode.UP))
        {
            s.Y += 1;
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        BorderWidget.SetSize(Width, Height);
        TitleBarWidget.SetSize(Width - 2, TitleBarHeight - 1);
        BackgroundWidget.SetSize(Width - 2, Height - TitleBarHeight - 1);
        CloseButton.SetPosition(Width - CloseButton.Size.Width - 1, 1);
        MaximizeRestoreButton.SetPosition(CloseButton.Position.X - MaximizeRestoreButton.Size.Width, CloseButton.Position.Y);
        MinimizeButton.SetPosition(MaximizeRestoreButton.Position.X - MinimizeButton.Size.Width, MinimizeButton.Position.Y);
    }

    public override void Maximize()
    {
        if (!Resizable) return;
        Maximized = true;
        NormalRect = new Rect(X, Y, Width, Height);
        Rect UsableBounds = Graphics.GetUsableBounds(Screen);
        SetPosition(UsableBounds.X - 1, UsableBounds.Y - 1);
        SetSize(UsableBounds.Width + 2, UsableBounds.Height + 2);
        AnchorPoint = null;
        MovingWindow = false;
        MovedWindow = false;
        MaximizeRestoreButton.Redraw();
    }

    public void Restore(bool Dragging)
    {
        if (!Resizable) return;
        Maximized = false;
        SetSize(NormalRect.Width, NormalRect.Height);
        int x = NormalRect.X;
        int y = NormalRect.Y;
        if (Dragging)
        {
            AnchorPoint = new Point(NormalRect.Width / 2, TitleBarHeight / 2);
            x += AnchorPoint.X;
            y += AnchorPoint.Y;
        }
        if (y < 0) y = 0;
        SetPosition(x, y);
        MaximizeRestoreButton.Redraw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (e.LeftButton != e.OldLeftButton && e.LeftButton)
        {
            if (Resizable)
            {
                ResizingVertical = e.Y >= Height - ResizeMargin;
                ResizingHorizontal = e.X >= Width - ResizeMargin;
                ResizingWindow = false;
                if (ResizingHorizontal || ResizingVertical)
                {
                    ResizingWindow = true;
                    AnchorPoint = new Point(e.X, e.Y);
                    Input.CaptureMouse();
                }
            }
            if (e.Y < TitleBarHeight && !ResizingWindow)
            {
                if (!CloseButton.Hovering && !MaximizeRestoreButton.Hovering && !MinimizeButton.Hovering)
                {
                    if (UI.TimerExists("double") && !UI.TimerPassed("double"))
                    {
                        if (Maximized) Restore(false);
                        else Maximize();
                        UI.DestroyTimer("double");
                        return;
                    }
                    else if (UI.TimerExists("double") && UI.TimerPassed("double"))
                    {
                        UI.ResetTimer("double");
                    }
                    else
                    {
                        UI.SetTimer("double", 300);
                    }
                    MovingWindow = true;
                    AnchorPoint = new Point(e.X, e.Y);
                    Input.CaptureMouse();
                }
            }
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (MovingWindow)
        {
            if (Maximized && MovedWindow)
            {
                Restore(true);
            }
            else
            {
                int x = X + e.X - AnchorPoint.X;
                int y = Y + e.Y - AnchorPoint.Y;
                SetPosition(x, y);
                if (e.X != AnchorPoint.X || e.Y != AnchorPoint.Y) MovedWindow = true;
            }
        }
        else
        {
            if (Resizable)
            {
                if (ResizingWindow)
                {
                    int w = Width;
                    int h = Height;
                    if (ResizingHorizontal)
                    {
                        w += e.X - AnchorPoint.X;
                        AnchorPoint.X = e.X;
                    }
                    if (ResizingVertical)
                    {
                        h += e.Y - AnchorPoint.Y;
                        AnchorPoint.Y = e.Y;
                    }
                    if (w != Width || h != Height) SetSize(w, h);
                }
                else
                {
                    if (!CloseButton.Hovering && !MaximizeRestoreButton.Hovering && !MinimizeButton.Hovering)
                    {
                        if (e.X >= 0 && e.Y >= 0 && e.X < Width && e.Y < Height)
                        {
                            bool bottom = e.Y >= Height - ResizeMargin;
                            bool right = e.X >= Width - ResizeMargin;
                            if (right && !bottom) Input.SetCursor(CursorType.SizeWE);
                            else if (bottom && !right) Input.SetCursor(CursorType.SizeNS);
                            else if (right && bottom) Input.SetCursor(CursorType.SizeNWSE);
                            else Input.SetCursor(CursorType.Arrow);
                        }
                    }
                    else
                    {
                        Input.SetCursor(CursorType.Arrow);
                    }
                }
            }
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (e.LeftButton != e.OldLeftButton && !e.LeftButton)
        {
            if (MovingWindow)
            {
                MovingWindow = false;
                AnchorPoint = null;
                Input.ReleaseMouse();
                if (Y + e.Y < 4 && !Maximized && MovedWindow && Resizable)
                {
                    Maximize();
                }
                else if (Y < 0) SetPosition(X, 0);
                MovedWindow = false;
            }
            else if (ResizingWindow)
            {
                ResizingWindow = false;
                ResizingHorizontal = false;
                ResizingVertical = false;
                AnchorPoint = null;
                Input.ReleaseMouse();
            }
        }
    }

    public override void FocusGained(BaseEventArgs e)
    {
        base.FocusGained(e);
        // Must force re-render when gaining focus in order
        // to properly display all sprites/bitmaps/etc.
        // Otherwise nothing except some generic window header
        // will pop up.
        Graphics.Update(false, true);
    }

    public override void FocusLost(BaseEventArgs e)
    {
        base.FocusLost(e);
        // Out-of-reach mouse moving event to make sure the minimize button
        // doesn't stay in the hovered state.
        OnMouseMoving?.Invoke(new MouseEventArgs(-1, -1, false, false, false, false, false, false, 0, 0));
        Graphics.Update(false, true);
    }
}

public class WindowButton : ActivatableWidget
{
    public WindowButtonType WindowButtonType;

    public WindowButton(IContainer Parent) : base(Parent)
    {
        Sprites["icon"] = new Sprite(Viewport);
        Sprites["icon"].X = 17;
        Sprites["icon"].Y = 10;
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["icon"].Bitmap?.Dispose();
        Sprites["icon"].Bitmap = new Bitmap(10, 10);
        Sprites["icon"].Bitmap.Unlock();
        if (WindowButtonType == WindowButtonType.Close)
        {
            Color outer = Pressing ? new Color(201, 121, 127) : Hovering ? new Color(241, 107, 118) : new Color(141, 139, 138);
            Color inner = Pressing ? new Color(255, 255, 255) : Hovering ? new Color(255, 255, 255) : new Color(250, 248, 247);
            Sprites["icon"].Bitmap.DrawLine(0, 1, 8, 9, outer);
            Sprites["icon"].Bitmap.DrawLine(1, 0, 9, 8, outer);
            Sprites["icon"].Bitmap.DrawLine(8, 0, 0, 8, outer);
            Sprites["icon"].Bitmap.DrawLine(9, 1, 1, 9, outer);
            Sprites["icon"].Bitmap.DrawLine(0, 0, 9, 9, inner);
            Sprites["icon"].Bitmap.DrawLine(9, 0, 0, 9, inner);
            SetBackgroundColor(Pressing ? new Color(169, 40, 49) : Hovering ? new Color(232, 17, 35) : Color.ALPHA);
        }
        else if (WindowButtonType == WindowButtonType.MaximizeRestore)
        {
            if (Window is BorderlessWindow && ((BorderlessWindow)Window).Maximized)
            {
                Sprites["icon"].Bitmap.DrawRect(0, 2, 8, 8, Color.WHITE);
                Sprites["icon"].Bitmap.DrawRect(2, 0, 8, 8, Color.WHITE);
                Sprites["icon"].Bitmap.FillRect(1, 3, 6, 6, Color.ALPHA);
            }
            else
            {
                Sprites["icon"].Bitmap.DrawRect(10, 10, Color.WHITE);
            }
            SetBackgroundColor(Pressing ? new Color(112, 110, 109) : Hovering ? new Color(94, 92, 91) : Color.ALPHA);
        }
        else if (WindowButtonType == WindowButtonType.Minimize)
        {
            Sprites["icon"].Bitmap.DrawLine(0, 5, 9, 5, Color.WHITE);
            SetBackgroundColor(Pressing ? new Color(112, 110, 109) : Hovering ? new Color(94, 92, 91) : Color.ALPHA);
        }
        Sprites["icon"].Bitmap.Lock();
    }
}

public enum WindowButtonType
{
    Close = 0,
    MaximizeRestore = 1,
    Minimize = 2
}
