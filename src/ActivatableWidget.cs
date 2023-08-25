using odl;

namespace amethyst;

public class ActivatableWidget : Widget
{
    public bool Hovering { get; protected set; } = false;
    public bool Pressing { get; protected set; } = false;

    protected bool StartedPressingInside = false;

    public BaseEvent OnPressed;

    public ActivatableWidget(IContainer Parent) : base(Parent)
    {
        OnWidgetSelected += WidgetSelected;
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Hovering = Mouse.Inside;
        if (Hovering)
        {
            if (StartedPressingInside)
            {
                Pressing = true;
            }
        }
        else
        {
            if (Pressing && StartedPressingInside)
            {
                Pressing = false;
                Hovering = true;
            }
        }
        Redraw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (e.LeftButton != e.OldLeftButton && Hovering)
        {
            Pressing = true;
            if (Hovering) StartedPressingInside = true;
            Window.UI.SetSelectedWidget(this);
            Redraw();
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (e.LeftButton != e.OldLeftButton)
        {
            if (Hovering && Pressing && StartedPressingInside)
            {
                OnPressed?.Invoke(new BaseEventArgs());
                Pressing = false;
                StartedPressingInside = false;
                if (!Disposed) Redraw();
            }
            else
            {
                Pressing = false;
                StartedPressingInside = false;
                Hovering = false;
                if (!Disposed) Redraw();
            }
        }
    }

    public override void WidgetDeselected(BaseEventArgs e)
    {
        base.WidgetDeselected(e);
        Redraw();
    }
}
