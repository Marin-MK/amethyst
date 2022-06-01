﻿using System;
using System.Collections.Generic;
using odl;

namespace amethyst;

public class UIManager : IContainer
{
    public UIWindow Window { get; protected set; }
    public Point AdjustedPosition { get { return new Point(0, 0); } set { throw new MethodNotSupportedException(this); } }
    public Point Position { get { return new Point(0, 0); } }
    public int ScrolledX { get { return 0; } set { throw new MethodNotSupportedException(this); } }
    public int ScrolledY { get { return 0; } set { throw new MethodNotSupportedException(this); } }
    public int ZIndex { get { return 0; } }
    public Point ScrolledPosition { get { return new Point(0, 0); } }
    public Size Size { get { return new Size(this.Window.Width, this.Window.Height); } }
    public Viewport Viewport { get { return this.Window.Viewport; } }
    public List<Widget> Widgets { get; protected set; } = new List<Widget>();
    public Color BackgroundColor { get { return this.Window.BackgroundColor; } }
    public Widget SelectedWidget { get; protected set; }
    public ScrollBar HScrollBar { get { return null; } set { throw new MethodNotSupportedException(this); } }
    public ScrollBar VScrollBar { get { return null; } set { throw new MethodNotSupportedException(this); } }
    public List<Shortcut> Shortcuts { get; protected set; } = new List<Shortcut>();
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
        this.Mouse = new MouseManager(this);
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
        this.Window.OnMouseWheel += e => Mouse.ProcessMouseWheel(e, true);
        this.Window.OnTick += _ => Update();
        this.Window.OnSizeChanged += e => SizeChanged(e);
        this.Window.OnTextInput += e => TextInput(e);
    }

    public void Add(Widget w)
    {
        this.Widgets.Add(w);
    }

    public Widget Remove(Widget w)
    {
        for (int i = 0; i < this.Widgets.Count; i++)
        {
            if (this.Widgets[i] == w)
            {
                this.Widgets.RemoveAt(i);
                return w;
            }
        }
        return null;
    }

    public void SizeChanged(BaseEventArgs e)
    {
        this.Viewport.Width = Size.Width;
        this.Viewport.Height = Size.Height;
        this.Widgets.ForEach(w =>
        {
            w.SetSize(this.Size);
            w.OnParentSizeChanged(new BaseEventArgs());
            w.Redraw();
        });
    }

    public void Update()
    {
        this.Shortcuts.RemoveAll(s => s.PendingRemoval);
        Shortcuts.AddRange(ShortcutsPendingAddition);
        ShortcutsPendingAddition.Clear();
        foreach (Shortcut s in this.Shortcuts)
        {
            // UIManager only has global shortcuts, and all global shortcuts are in UIManager.
            if (s.Widget != null && (s.Widget.WindowLayer < Window.ActiveWidget.WindowLayer || !s.Widget.IsVisible() || s.Widget.Disposed)) continue;

            Key k = s.Key;
            bool Valid = false;
            if (Input.Press(k.MainKey))
            {
                if (TimerPassed($"key_{s.Key.ID}"))
                {
                    ResetTimer($"key_{s.Key.ID}");
                    Valid = true;
                }
                else if (TimerPassed($"key_{s.Key.ID}_initial"))
                {
                    SetTimer($"key_{s.Key.ID}", 50);
                    DestroyTimer($"key_{s.Key.ID}_initial");
                    Valid = true;
                }
                else if (!TimerExists($"key_{s.Key.ID}") && !TimerExists($"key_{s.Key.ID}_initial"))
                {
                    if (Input.Trigger(k.MainKey))
                    {
                        SetTimer($"key_{s.Key.ID}_initial", 300);
                        Valid = true;
                    }
                }
            }
            else
            {
                if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
                if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
            }
            if (!Valid) continue;

            // Modifiers
            Valid = k.Modifiers.TrueForAll(m => Input.Press(m));

            if (!Valid) continue;

            if (s.Condition != null)
            {
                BoolEventArgs e = new BoolEventArgs(true);
                s.Condition(e);
                if (!e.Value) Valid = false;
            }

            if (Valid)
            {
                s.Event(new BaseEventArgs());
                // Remove any other key triggers for this iteration
                ResetShortcutTimers(this);
            }
        }

        for (int i = 0; i < this.Widgets.Count; i++)
        {
            if (this.Widgets[i].Disposed)
            {
                this.Widgets.RemoveAt(i);
                i--;
                continue;
            }
            this.Widgets[i].Update();
        }
    }

    public void ResetShortcutTimers(IContainer Exception)
    {
        if (this != Exception)
        {
            foreach (Shortcut s in this.Shortcuts)
            {
                if (!s.GlobalShortcut) continue;
                if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
                if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
            }
        }
        for (int i = 0; i < this.Widgets.Count; i++)
        {
            this.Widgets[i].ResetShortcutTimers(Exception);
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
        Timers.Add(new Timer(identifier, DateTime.Now.Ticks, 10000 * milliseconds));
    }

    /// <summary>
    /// Returns whether or not the specified timer's time has elapsed.
    /// </summary>
    public bool TimerPassed(string identifier)
    {
        Timer t = Timers.Find(timer => timer.Identifier == identifier);
        if (t == null) return false;
        return DateTime.Now.Ticks >= t.StartTime + t.Timespan;
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
        t.StartTime = DateTime.Now.Ticks;
    }

    public void SetSelectedWidget(Widget w)
    {
        if (this.SelectedWidget == w) return;
        if (this.SelectedWidget != null && !this.SelectedWidget.Disposed)
        {
            this.SelectedWidget.SelectedWidget = false;
            Widget selbefore = this.SelectedWidget;
            this.SelectedWidget.OnWidgetDeselected(new BaseEventArgs());
            if (!selbefore.Disposed) selbefore.Redraw();
            // Possible if OnWidgetDeselected itself called SetSelectedWidget on a different widget.
            // In that case we should skip the setting-bit below, as it would
            // set the selected widget to null AFTER the previous SetSelectedWidget call.
            if (selbefore != this.SelectedWidget)
                return;
        }
        this.SelectedWidget = w;
        if (w != null)
        {
            this.SelectedWidget.SelectedWidget = true;
            this.SelectedWidget.Redraw();
        }
    }

    public void TextInput(TextEventArgs e)
    {
        this.SelectedWidget?.OnTextInput(e);
    }

    public void RegisterShortcut(Shortcut s)
    {
        ShortcutsPendingAddition.Add(s);
    }

    public void DeregisterShortcut(Shortcut s)
    {
        if (this.ShortcutsPendingAddition.Contains(s)) this.ShortcutsPendingAddition.Remove(s);
        this.Shortcuts.Remove(s);
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
