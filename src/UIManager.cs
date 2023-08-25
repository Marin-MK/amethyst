using System;
using System.Collections.Generic;
using System.Diagnostics;
using odl;

namespace amethyst.src;

public class UIManager : IContainer
{
    public UIWindow Window { get; protected set; }
    public Point AdjustedPosition { get { return new Point(0, 0); } set { throw new MethodNotSupportedException(this); } }
    public Point Position { get { return new Point(0, 0); } }
    public int ScrolledX { get { return 0; } set { throw new MethodNotSupportedException(this); } }
    public int ScrolledY { get { return 0; } set { throw new MethodNotSupportedException(this); } }
    public int ZIndex { get { return 0; } }
    public Point ScrolledPosition { get { return new Point(0, 0); } }
    public Size Size { get { return new Size(Window.Width, Window.Height); } }
    public Viewport Viewport { get { return Window.Viewport; } }
    public List<Widget> Widgets { get; protected set; } = new List<Widget>();
    public Color BackgroundColor { get { return Window.BackgroundColor; } }
    public Widget SelectedWidget { get; protected set; }
    public ScrollBar HScrollBar { get { return null; } set { throw new MethodNotSupportedException(this); } }
    public ScrollBar VScrollBar { get { return null; } set { throw new MethodNotSupportedException(this); } }
    public List<Shortcut> Shortcuts { get; protected set; } = new List<Shortcut>();
    public List<string> ValidShortcutInputs = new List<string>();
    public int WindowLayer { get { return 0; } set { throw new MethodNotSupportedException(this); } }
    public int LeftCutOff { get { return 0; } }
    public int TopCutOff { get { return 0; } }
    public List<Timer> Timers = new List<Timer>();
    public Container Container;
    public bool EvaluatedLastMouseEvent { get; set; }

    List<Shortcut> ShortcutsPendingAddition = new List<Shortcut>();

    /// <summary>
    /// This object aids in fetching mouse input.
    /// </summary>
    public MouseManager Mouse { get; protected set; }

    /// <summary>
    /// Called whenever the mouse moves across the window.
    /// </summary>
    public MouseEvent OnMouseMoving { get; set; }

    /// <summary>
    /// Called whenever the mouse moves in or out of the widget.
    /// </summary>
    public MouseEvent OnHoverChanged { get; set; }

    /// <summary>
    /// Called whenever a mouse button is pressed down.
    /// </summary>
    public MouseEvent OnMouseDown { get; set; }

    /// <summary>
    /// Called whenever a mouse button is released.
    /// </summary>
    public MouseEvent OnMouseUp { get; set; }

    /// <summary>
    /// Called while a mouse button is being held down.
    /// </summary>
    public MouseEvent OnMousePress { get; set; }

    /// <summary>
    /// Called whenever the left mouse button is pressed down.
    /// </summary>
    public MouseEvent OnLeftMouseDown { get; set; }

    /// <summary>
    /// Called whenever the left mouse button is pressed down inside the widget.
    /// </summary>
    public MouseEvent OnLeftMouseDownInside { get; set; }

    /// <summary>
    /// Called whenever the left mouse button is released.
    /// </summary>
    public MouseEvent OnLeftMouseUp { get; set; }

    /// <summary>
    /// Called while the left mouse button is being held down.
    /// </summary>
    public MouseEvent OnLeftMousePress { get; set; }

    /// <summary>
    /// Called whenever the right mouse button is pressed down.
    /// </summary>
    public MouseEvent OnRightMouseDown { get; set; }

    /// <summary>
    /// Called whenever the right mouse button is pressed down inside the widget.
    /// </summary>
    public MouseEvent OnRightMouseDownInside { get; set; }

    /// <summary>
    /// Called whenever the right mouse button is released.
    /// </summary>
    public MouseEvent OnRightMouseUp { get; set; }

    /// <summary>
    /// Called while the right mouse button is being held down.
    /// </summary>
    public MouseEvent OnRightMousePress { get; set; }

    /// <summary>
    /// Called whenever the middle mouse button is pressed down.
    /// </summary>
    public MouseEvent OnMiddleMouseDown { get; set; }

    /// <summary>
    /// Called whenever the middle mouse button is pressed down inside the widget.
    /// </summary>
    public MouseEvent OnMiddleMouseDownInside { get; set; }

    /// <summary>
    /// Called whenever the middle mouse button is released.
    /// </summary>
    public MouseEvent OnMiddleMouseUp { get; set; }

    /// <summary>
    /// Called while the middle mouse button is being held down.
    /// </summary>
    public MouseEvent OnMiddleMousePress { get; set; }

    /// <summary>
    /// Called whenever the mouse wheel is scrolled.
    /// </summary>
    public MouseEvent OnMouseWheel { get; set; }

    /// <summary>
    /// Called whenever the left mouse button is pressed down twice inside the widget in short succession.
    /// </summary>
    public MouseEvent OnDoubleLeftMouseDownInside { get; set; }

    public IContainer Parent { get { throw new MethodNotSupportedException(this); } }

    public UIManager(UIWindow Window)
    {
        this.Window = Window;
        this.Window.SetActiveWidget(this);
        Mouse = new MouseManager(this);
        this.Window.OnMouseMoving += e => Mouse.ProcessMouseMoving(e, true);
        this.Window.OnMouseDown += e =>
        {
            Mouse.ProcessMouseDown(e, true);
            Mouse.ProcessMouseMoving(e, true);
        };
        this.Window.OnMouseUp += e =>
        {
            Mouse.ProcessMouseUp(e, true);
            Mouse.ProcessMouseMoving(e, true);
        };
        this.Window.OnMousePress += e => Mouse.ProcessMousePress(e, true);
        this.Window.OnMouseWheel += e =>
        {
            Mouse.ProcessMouseWheel(e, true);
            Mouse.ProcessMouseMoving(e, true);
        };
        this.Window.OnTick += _ => Update();
        this.Window.OnSizeChanged += e => SizeChanged(e);
        this.Window.OnTextInput += e => TextInput(e);
    }

    public void Add(Widget w, int Index = -1)
    {
        if (Index == -1) Widgets.Add(w);
        else Widgets.Insert(Index, w);
    }

    public Widget Remove(Widget w)
    {
        for (int i = 0; i < Widgets.Count; i++)
        {
            if (Widgets[i] == w)
            {
                Widgets.RemoveAt(i);
                return w;
            }
        }
        return null;
    }

    public void SizeChanged(BaseEventArgs e)
    {
        Size OldSize = Size;
        Viewport.Width = Size.Width;
        Viewport.Height = Size.Height;
        Widgets.ForEach(w =>
        {
            w.SetSize(Size);
            w.OnParentSizeChanged(new ObjectEventArgs(Size, OldSize));
            w.Redraw();
        });
    }

    public void Update()
    {
        if (Window.Disposed) return;
        // Update children before self, that way widget-local shortcuts will trigger
        // before any global shortcuts will.
        for (int i = 0; i < Widgets.Count; i++)
        {
            if (Widgets[i].Disposed)
            {
                Widgets.RemoveAt(i);
                i--;
                continue;
            }
            Widgets[i].Update();
        }

        Shortcuts.RemoveAll(s => s.PendingRemoval);
        Shortcuts.AddRange(ShortcutsPendingAddition);
        ShortcutsPendingAddition.Clear();
        foreach (Shortcut s in Shortcuts)
        {
            // UIManager only has global shortcuts, and all global shortcuts are in UIManager.
            if (s.PendingRemoval || s.Widget != null && (s.Widget.WindowLayer < Window.ActiveWidget.WindowLayer || !s.Widget.IsVisible() || s.Widget.Disposed)) continue;

            Key k = s.Key;
            bool Valid = false;
            if (Input.Press(k.MainKey))
            {
                if (TimerPassed($"key_{k.ID}") && ValidShortcutInputs.Contains(k.ID))
                {
                    ResetTimer($"key_{k.ID}");
                    Valid = true;
                }
                else if (TimerPassed($"key_{k.ID}_initial") && ValidShortcutInputs.Contains(k.ID))
                {
                    SetTimer($"key_{k.ID}", 50);
                    DestroyTimer($"key_{k.ID}_initial");
                    Valid = true;
                }
                else if (!TimerExists($"key_{k.ID}") && !TimerExists($"key_{k.ID}_initial"))
                {
                    if (Input.Trigger(k.MainKey))
                    {
                        SetTimer($"key_{k.ID}_initial", 300);
                        Valid = true;
                    }
                }
                else
                {
                    if (!ValidShortcutInputs.Contains(k.ID)) ValidShortcutInputs.Add(k.ID);
                }
            }
            else
            {
                if (TimerExists($"key_{k.ID}")) DestroyTimer($"key_{k.ID}");
                if (TimerExists($"key_{k.ID}_initial")) DestroyTimer($"key_{k.ID}_initial");
                if (ValidShortcutInputs.Contains(k.ID)) ValidShortcutInputs.Remove(k.ID);
            }
            if (!Valid) continue;

            // Modifiers
            if (Input.Press(Keycode.SHIFT) && !k.Modifiers.Contains(Keycode.SHIFT)) Valid = false;
            else if (Input.Press(Keycode.CTRL) && !k.Modifiers.Contains(Keycode.CTRL)) Valid = false;
            else if (Input.Press(Keycode.ALT) && !k.Modifiers.Contains(Keycode.ALT)) Valid = false;
            else if (!k.Modifiers.TrueForAll(m => Input.Press(m))) Valid = false;

            if (!Valid) continue;

            if (s.Condition != null)
            {
                BoolEventArgs e = new BoolEventArgs(true);
                s.Condition(e);
                if (!e.Value) Valid = false;
            }

            if (Valid)
            {
                // Remove any other key triggers for this iteration
                ResetShortcutTimers(this);
                s.Event(new BaseEventArgs());
            }
        }
    }

    public void ResetShortcutTimers(IContainer Exception)
    {
        if (this != Exception)
        {
            foreach (Shortcut s in Shortcuts)
            {
                if (!s.GlobalShortcut) continue;
                if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
                if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
                if (ValidShortcutInputs.Contains(s.Key.ID)) ValidShortcutInputs.Remove(s.Key.ID);
            }
        }
        for (int i = 0; i < Widgets.Count; i++)
        {
            Widgets[i].ResetShortcutTimers(Exception);
        }
        Input.IterationEnd();
    }

    /// <summary>
    /// Sets a timer.
    /// </summary>
    /// <param name="identifier">Unique string identifier.</param>
    /// <param name="milliseconds">Number of milliseconds to run the timer for.</param>
    public void SetTimer(string identifier, long milliseconds)
    {
        Timers.Add(new Timer(identifier, Stopwatch.GetTimestamp(), 10000 * milliseconds));
    }

    /// <summary>
    /// Returns whether or not the specified timer's time has elapsed.
    /// </summary>
    public bool TimerPassed(string identifier)
    {
        Timer t = Timers.Find(timer => timer.Identifier == identifier);
        if (t == null) return false;
        return Stopwatch.GetTimestamp() >= t.StartTime + t.Timespan;
    }

    /// <summary>
    /// Returns whether or not the specified timer exists.
    /// </summary>
    public bool TimerExists(string identifier)
    {
        return Timers.Exists(t => t.Identifier == identifier);
    }

    /// <summary>
    /// Destroys the specified timer object.
    /// </summary>
    public void DestroyTimer(string identifier)
    {
        Timer t = Timers.Find(timer => timer.Identifier == identifier);
        if (t == null) throw new Exception("No timer by the identifier of '" + identifier + "' was found.");
        Timers.Remove(t);
    }

    /// <summary>
    /// Resets the specified timer with the former timespan.
    /// </summary>
    public void ResetTimer(string identifier)
    {
        Timer t = Timers.Find(timer => timer.Identifier == identifier);
        if (t == null) throw new Exception("No timer by the identifier of '" + identifier + "' was found.");
        t.StartTime = Stopwatch.GetTimestamp();
    }

    public void SetSelectedWidget(Widget w)
    {
        if (SelectedWidget == w) return;
        if (SelectedWidget != null && !SelectedWidget.Disposed)
        {
            SelectedWidget.SelectedWidget = false;
            Widget selbefore = SelectedWidget;
            SelectedWidget.OnWidgetDeselected(new BaseEventArgs());
            // Possible if OnWidgetDeselected itself called SetSelectedWidget on a different widget.
            // In that case we should skip the setting-bit below, as it would
            // set the selected widget to null AFTER the previous SetSelectedWidget call.
            if (selbefore != SelectedWidget)
                return;
        }
        SelectedWidget = w;
        if (w != null)
        {
            SelectedWidget.SelectedWidget = true;
        }
    }

    public void TextInput(TextEventArgs e)
    {
        SelectedWidget?.OnTextInput(e);
    }

    public void RegisterShortcut(Shortcut s)
    {
        ShortcutsPendingAddition.Add(s);
    }

    public void DeregisterShortcut(Shortcut s)
    {
        if (ShortcutsPendingAddition.Contains(s)) ShortcutsPendingAddition.Remove(s);
        s.PendingRemoval = true;
        if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
        if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
    }

    public void SetBackgroundColor(Color c)
    {
        Window.SetBackgroundColor(c);
    }
    public void SetBackgroundColor(byte r, byte g, byte b, byte a = 255)
    {
        Window.SetBackgroundColor(new Color(r, g, b, a));
    }
}
