using System;
using odl;

namespace amethyst;

public abstract class ScrollBar : Widget
{
    public double SliderSize { get; protected set; }
    public double Value { get; protected set; }
    public float ScrollStep { get; set; } = 32 / 3f;
    public float? MinScrollStep { get; set; }
    public Rect MouseInputRect { get; set; }
    public bool SliderHovering { get; protected set; } = false;
    public bool SliderDragging { get; protected set; } = false;
    public bool Arrow1Hovering { get; protected set; } = false;
    public bool Arrow1Pressing { get; protected set; } = false;
    public bool Arrow2Hovering { get; protected set; } = false;
    public bool Arrow2Pressing { get; protected set; } = false;
    public bool Arrow1StartedPressing { get; protected set; } = false;
    public bool Arrow2StartedPressing { get; protected set; } = false;
    public int MinSliderSize = 8;

    protected double OriginalSize = 0.25;
    protected int Arrow1Size = 17;
    protected int Arrow2Size = 17;
    protected int DragOffset = -1;

    public Widget LinkedWidget;

    public BaseEvent OnValueChanged;
    public DirectionEvent OnControlScrolling;

    public ScrollBar(IContainer Parent) : base(Parent)
    {
        this.ConsiderInAutoScrollPositioning = this.ConsiderInAutoScrollCalculation = false;
        this.Sprites["slider"] = new Sprite(this.Viewport);
        this.Sprites["arrow1"] = new Sprite(this.Viewport);
        this.Sprites["arrow2"] = new Sprite(this.Viewport);
        this.SliderSize = 0.25;
        this.Value = 0;
    }

    public void SetSliderSize(double size)
    {
        OriginalSize = size;
        double minsize = (double)MinSliderSize / this.Size.Height;
        size = Math.Max(Math.Min(size, 1), 0);
        size = Math.Max(size, minsize);
        if (this.SliderSize != size)
        {
            this.SliderSize = size;
            this.Redraw();
        }
    }

    public override Widget SetSize(Size size)
    {
        base.SetSize(size);
        SetSliderSize(OriginalSize);
        return this;
    }

    public virtual void ScrollLeft(int Count = 1)
    {

    }

    public virtual void ScrollRight(int Count = 1)
    {

    }

    public virtual void ScrollDown(int Count = 1)
    {

    }

    public virtual void ScrollUp(int Count = 1)
    {

    }

    public virtual void SetValue(double value, bool CallEvent = true)
    {
        if (value < 0) value = 0;
        if (value > 1) value = 1;
        if (this.Value != value)
        {
            this.Value = value;
            if (CallEvent) this.OnValueChanged?.Invoke(new BaseEventArgs());
            Redraw();
        }
    }

    public override void MouseWheel(MouseEventArgs e)
    {
        if (!IsVisible()) return;
        bool inside = Viewport.Contains(e.X, e.Y);
        if (this.MouseInputRect != null && !inside) inside = this.MouseInputRect.Contains(e.X, e.Y);
        if (inside)
        {
            if (Input.Press(Keycode.CTRL))
            {
                this.OnControlScrolling?.Invoke(new DirectionEventArgs(e.WheelY > 0, e.WheelY < 0));
            }
            else
            {
                int rightcount = 0;
                int leftcount = 0;
                if (e.WheelX < 0) rightcount = Math.Abs(e.WheelX);
                else leftcount = e.WheelX;
                if (rightcount != 0) this.ScrollRight(rightcount * 3);
                if (leftcount != 0) this.ScrollLeft(leftcount * 3);
                int downcount = 0;
                int upcount = 0;
                if (e.WheelY < 0) downcount = Math.Abs(e.WheelY);
                else upcount = e.WheelY;
                if (downcount != 0) this.ScrollDown(downcount * 3);
                if (upcount != 0) this.ScrollUp(upcount * 3);
            }
        }
    }
}
