using System;
using odl;

namespace amethyst.src;

public abstract class ScrollBar : Widget
{
    public double SliderSize { get; protected set; }
    public double Value { get; protected set; }
    public float ScrollStep { get; protected set; } = 32 / 3f;
    public float? MinScrollStep { get; protected set; }
    public Rect MouseInputRect { get; set; }
    public bool SliderHovering { get; protected set; } = false;
    public bool SliderDragging { get; protected set; } = false;
    public bool Arrow1Hovering { get; protected set; } = false;
    public bool Arrow1Pressing { get; protected set; } = false;
    public bool Arrow2Hovering { get; protected set; } = false;
    public bool Arrow2Pressing { get; protected set; } = false;
    public bool Arrow1StartedPressing { get; protected set; } = false;
    public bool Arrow2StartedPressing { get; protected set; } = false;
    public bool Pressable { get; protected set; } = true;
    public bool KeepInvisible = false;
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
        ConsiderInAutoScrollPositioningX = ConsiderInAutoScrollPositioningY = ConsiderInAutoScrollCalculation = false;
        Sprites["slider"] = new Sprite(Viewport);
        Sprites["arrow1"] = new Sprite(Viewport);
        Sprites["arrow2"] = new Sprite(Viewport);
        SliderSize = 0.25;
        Value = 0;
    }

    public void SetSliderSize(double size)
    {
        OriginalSize = size;
        double minsize = (double)MinSliderSize / Size.Height;
        size = Math.Max(Math.Min(size, 1), 0);
        size = Math.Max(size, minsize);
        if (SliderSize != size)
        {
            SliderSize = size;
            Redraw();
        }
    }

    public override Widget SetSize(Size size)
    {
        base.SetSize(size);
        SetSliderSize(OriginalSize);
        return this;
    }

    public void SetScrollStep(float ScrollStep)
    {
        if (this.ScrollStep != ScrollStep)
        {
            this.ScrollStep = ScrollStep;
        }
    }

    public void SetMinScrollStep(float? MinScrollStep)
    {
        if (this.MinScrollStep != MinScrollStep)
        {
            this.MinScrollStep = MinScrollStep;
        }
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
        if (Value != value)
        {
            Value = value;
            if (CallEvent) OnValueChanged?.Invoke(new BaseEventArgs());
            Redraw();
        }
    }

    public void SetPressable(bool Pressable)
    {
        this.Pressable = Pressable;
    }

    public override void MouseWheel(MouseEventArgs e)
    {
        if (!IsVisible() && !KeepInvisible) return;
        bool inside = Viewport.Contains(e.X, e.Y);
        if (MouseInputRect != null && !inside) inside = MouseInputRect.Contains(e.X, e.Y);
        if (inside)
        {
            if (Input.Press(Keycode.CTRL))
            {
                OnControlScrolling?.Invoke(new DirectionEventArgs(e.WheelY > 0, e.WheelY < 0));
            }
            else
            {
                int rightcount = 0;
                int leftcount = 0;
                if (e.WheelX < 0) rightcount = Math.Abs(e.WheelX);
                else leftcount = e.WheelX;
                if (rightcount != 0) ScrollRight(rightcount * 3);
                if (leftcount != 0) ScrollLeft(leftcount * 3);
                int downcount = 0;
                int upcount = 0;
                if (e.WheelY < 0) downcount = Math.Abs(e.WheelY);
                else upcount = e.WheelY;
                if (downcount != 0) ScrollDown(downcount * 3);
                if (upcount != 0) ScrollUp(upcount * 3);
            }
        }
    }
}
