using System;
using odl;

namespace amethyst;

public class MouseManager
{
    private IContainer Widget;
    private MouseEventArgs CurrentArgs;

    public MouseManager(IContainer Widget)
    {
        this.Widget = Widget;
    }

    public bool LeftMouseTriggered => Accessible && CurrentArgs.LeftButton && !CurrentArgs.OldLeftButton;
    public bool RightMouseTriggered => Accessible && CurrentArgs.RightButton && !CurrentArgs.OldRightButton;
    public bool MiddleMouseTriggered => Accessible && CurrentArgs.MiddleButton && !CurrentArgs.OldMiddleButton;

    public bool LeftMousePressed => Accessible && CurrentArgs.LeftButton;
    public bool RightMousePressed => Accessible && CurrentArgs.RightButton;
    public bool MiddleMousePressed => Accessible && CurrentArgs.MiddleButton;

    public bool LeftMouseReleased => CurrentArgs is not null && !CurrentArgs.LeftButton && CurrentArgs.OldLeftButton;
    public bool RightMouseReleased => CurrentArgs is not null && !CurrentArgs.RightButton && CurrentArgs.OldRightButton;
    public bool MiddleMouseReleased => CurrentArgs is not null && !CurrentArgs.MiddleButton && CurrentArgs.OldMiddleButton;

    public bool LeftStartedInside = false;
    public bool RightStartedInside = false;
    public bool MiddleStartedInside = false;

    private bool _inside = false;
    public bool Inside { get { return _inside && Accessible; } set { _inside = value; } }

    /// <summary>
    /// For this property to be true, there must be a valid mouse event.
    /// Furthermore, the linked container must be a UIManager,
    /// or if it's a widget, then that widget must not be disposed and visible,
    /// and either be an always-active-mouse widget or not have a window overlaying it.
    /// </summary>
    public bool Accessible => CurrentArgs is not null && (
                                Widget is not amethyst.Widget || (
                                    !((Widget) Widget).Disposed && Widget.Viewport.Visible && (
                                        ((Widget) Widget).MouseAlwaysActive ||
                                        Widget.WindowLayer >= ((Widget) Widget).Window.ActiveWidget.WindowLayer
                                    )
                                )
                              );

    public void ProcessMouseMoving(MouseEventArgs Args, bool Root)
    {
        if (Args.Handled)
        {
            Widget.EvaluatedLastMouseEvent = true;
            if (Root) ResetEventStates();
            return;
        }
        bool OldInside = this.Inside;
        this.CurrentArgs = Args;
        if (Accessible) Widget.OnMouseMoving?.Invoke(Args);
        RecursivelyPerform(m => m.ProcessMouseMoving(Args, false));
        this.Inside = Accessible && Widget.Viewport.Rect.Contains(Args.X, Args.Y);
        if (Accessible && OldInside != this.Inside) Widget.OnHoverChanged?.Invoke(Args);
        if (Root) ResetEventStates();
    }

    protected delegate void MouseManagerEvent(MouseManager Manager);

    protected void RecursivelyPerform(MouseManagerEvent Action)
    {
        Widget.EvaluatedLastMouseEvent = true;
        while (true)
        {
            bool any = false;
            for (int i = 0; i < Widget.Widgets.Count; i++)
            {
                if (!Widget.Widgets[i].EvaluatedLastMouseEvent)
                {
                    Action.Invoke(Widget.Widgets[i].Mouse);
                    any = true;
                }
            }
            if (!any) break;
        }
    }

    protected void ResetEventStates()
    {
        Widget.EvaluatedLastMouseEvent = false;
        Widget.Widgets.ForEach(widget => widget.Mouse.ResetEventStates());
    }

    public void ProcessMouseDown(MouseEventArgs Args, bool Root)
    {
        if (Args.Handled)
        {
            Widget.EvaluatedLastMouseEvent = true;
            if (Root) ResetEventStates();
            return;
        }
        this.CurrentArgs = Args;
        if (LeftMouseTriggered) this.LeftStartedInside = Inside;
        if (RightMouseTriggered) this.RightStartedInside = Inside;
        if (MiddleMouseTriggered) this.MiddleStartedInside = Inside;
        if (Accessible)
        {
            Widget.OnMouseDown?.Invoke(Args);
            if (LeftMouseTriggered)
            {
                Widget.OnLeftMouseDown?.Invoke(Args);
                if (Inside)
                {
                    if (Widget is Widget) ((Widget) Widget).OnWidgetSelected?.Invoke(Args);
                    Widget.OnLeftMouseDownInside?.Invoke(Args);
                }
            }
            if (RightMouseTriggered)
            {
                Widget.OnRightMouseDown?.Invoke(Args);
                if (Inside)
                {
                    if (Widget is Widget) ((Widget) Widget).OnWidgetSelected?.Invoke(Args);
                    Widget.OnRightMouseDownInside?.Invoke(Args);
                }
            }
            if (MiddleMouseTriggered)
            {
                Widget.OnMiddleMouseDown?.Invoke(Args);
                if (Inside)
                {
                    if (Widget is Widget) ((Widget) Widget).OnWidgetSelected?.Invoke(Args);
                    Widget.OnMiddleMouseDownInside?.Invoke(Args);
                }
            }
        }
        RecursivelyPerform(m => m.ProcessMouseDown(Args, false));
        if (Root) ResetEventStates();
    }

    public void ProcessMouseUp(MouseEventArgs Args, bool Root)
    {
        if (Args.Handled)
        {
            Widget.EvaluatedLastMouseEvent = true;
            if (Root) ResetEventStates();
            return;
        }
        this.CurrentArgs = Args;
        Widget.OnMouseUp?.Invoke(Args);
        if (LeftMouseReleased)
        {
            Widget.OnLeftMouseUp?.Invoke(Args);
            this.LeftStartedInside = false;
        }
        if (RightMouseReleased)
        {
            Widget.OnRightMouseUp?.Invoke(Args);
            this.RightStartedInside = false;
        }
        if (MiddleMouseReleased)
        {
            Widget.OnMiddleMouseUp?.Invoke(Args);
            this.MiddleStartedInside = false;
        }
        RecursivelyPerform(m => m.ProcessMouseUp(Args, false));
        if (Root) ResetEventStates();
    }

    public void ProcessMousePress(MouseEventArgs Args, bool Root)
    {
        if (Args.Handled)
        {
            Widget.EvaluatedLastMouseEvent = true;
            if (Root) ResetEventStates();
            return;
        }
        this.CurrentArgs = Args;
        if (Accessible)
        {
            Widget.OnMousePress?.Invoke(Args);
            if (LeftMousePressed) Widget.OnLeftMousePress?.Invoke(Args);
            if (RightMousePressed) Widget.OnRightMousePress?.Invoke(Args);
            if (MiddleMousePressed) Widget.OnMiddleMousePress?.Invoke(Args);
        }
        RecursivelyPerform(m => m.ProcessMousePress(Args, false));
        if (Root) ResetEventStates();
    }

    public void ProcessMouseWheel(MouseEventArgs Args, bool Root)
    {
        if (Args.Handled)
        {
            Widget.EvaluatedLastMouseEvent = true;
            if (Root) ResetEventStates();
            return;
        }
        this.CurrentArgs = Args;
        if (Accessible) Widget.OnMouseWheel?.Invoke(Args);
        RecursivelyPerform(m => m.ProcessMouseWheel(Args, false));
        if (Root) ResetEventStates();
    }
}
