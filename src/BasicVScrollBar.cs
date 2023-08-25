using System;
using odl;

namespace amethyst.src;

public abstract class BasicVScrollBar : ScrollBar
{
    protected int RealSliderHeight { get { return (int)Math.Round((Size.Height - Arrow1Size - Arrow2Size) * SliderSize); } }
    protected int RealSliderPosition { get { return (int)Math.Round((Size.Height - Arrow1Size - Arrow2Size - RealSliderHeight) * Value); } }

    protected int SliderX = 0;
    protected int SliderWidth = 8;

    public BasicVScrollBar(IContainer Parent) : base(Parent)
    {
        Size = new Size(17, 60);
        OnWidgetSelected += WidgetSelected;
    }

    public void SetSliderWidth(int SliderWidth)
    {
        MinimumSize.Width = SliderWidth;
        MaximumSize.Width = SliderWidth;
        this.SliderWidth = SliderWidth;
        SetWidth(SliderWidth);
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (!Pressable) return;
        bool OldSliderHovering = SliderHovering;
        bool OldSliderDragging = SliderDragging;
        bool OldArrow1Hovering = Arrow1Hovering;
        bool OldArrow1Pressing = Arrow1Pressing;
        bool OldArrow2Hovering = Arrow2Hovering;
        bool OldArrow2Pressing = Arrow2Pressing;
        SliderHovering = false;
        SliderDragging = false;
        Arrow1Hovering = false;
        Arrow1Pressing = false;
        Arrow2Hovering = false;
        Arrow2Pressing = false;
        if (DragOffset == -1)
        {
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx >= SliderX && rx < SliderX + SliderWidth)
            {
                if (ry >= Arrow1Size + RealSliderPosition && ry < Arrow1Size + RealSliderPosition + RealSliderHeight)
                {
                    SliderHovering = true;
                }
            }
            if (rx >= 0 && rx < Size.Width)
            {
                if (ry >= 0 && ry < Arrow1Size)
                {
                    Arrow1Hovering = true;
                    if (Arrow1StartedPressing) Arrow1Pressing = true;
                }
                else if (ry >= Size.Height - Arrow2Size && ry < Size.Height)
                {
                    Arrow2Hovering = true;
                    if (Arrow2StartedPressing) Arrow2Pressing = true;
                }
            }
            if (!Arrow1Pressing && Arrow1StartedPressing) Arrow1Hovering = true;
            if (!Arrow2Pressing && Arrow2StartedPressing) Arrow2Hovering = true;
            if (SliderHovering != OldSliderHovering ||
                SliderDragging != OldSliderDragging ||
                Arrow1Hovering != OldArrow1Hovering ||
                Arrow1Pressing != OldArrow1Pressing ||
                Arrow2Hovering != OldArrow2Hovering ||
                Arrow2Pressing != OldArrow2Pressing)
            {
                Redraw();
            }
        }
        else
        {
            // Dragging slider
            SliderDragging = true;
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y - Arrow1Size - DragOffset;
            int available = Size.Height - Arrow1Size - Arrow2Size - RealSliderHeight;
            SetValue((double)ry / available);
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Pressable) return;
        if (e.LeftButton == e.OldLeftButton) return;
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        if (rx < SliderX || rx >= SliderWidth) return;
        if (ry < Arrow1Size || ry >= Size.Height - Arrow2Size)
        {
            if (ry >= 0 && ry < Arrow1Size)
            {
                Arrow1StartedPressing = true;
                Arrow1Pressing = true;
                Redraw();
                ScrollUp();
                SetTimer("up", 300);
                return;
            }
            else if (ry >= Size.Height - Arrow2Size && ry < Size.Height)
            {
                Arrow2StartedPressing = true;
                Arrow2Pressing = true;
                ScrollDown();
                Redraw();
                SetTimer("down", 300);
                return;
            }
        }
        ry -= Arrow1Size;
        if (ry >= RealSliderPosition && ry < RealSliderPosition + RealSliderHeight)
        {
            DragOffset = ry - RealSliderPosition;
            Input.CaptureMouse();
        }
    }

    public override void Update()
    {
        base.Update();
        if (TimerExists("up"))
        {
            if (Arrow1StartedPressing)
            {
                if (TimerPassed("up") && Arrow1Pressing)
                {
                    ScrollUp();
                    DestroyTimer("up");
                    SetTimer("up", 50);
                }
            }
            else
            {
                DestroyTimer("up");
            }
        }
        if (TimerExists("down"))
        {
            if (Arrow2StartedPressing)
            {
                if (TimerPassed("down") && Arrow2Pressing)
                {
                    ScrollDown();
                    DestroyTimer("down");
                    SetTimer("down", 50);
                }
            }
            else
            {
                DestroyTimer("down");
            }
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (!Pressable) return;
        if (e.LeftButton != e.OldLeftButton)
        {
            DragOffset = -1;
            Input.ReleaseMouse();
            if (SliderDragging)
            {
                SliderDragging = false;
                Redraw();
            }
            if (Arrow1StartedPressing)
            {
                Arrow1StartedPressing = false;
                Arrow1Pressing = false;
                MouseMoving(e);
                Redraw();
            }
            if (Arrow2StartedPressing)
            {
                Arrow2StartedPressing = false;
                Arrow2Pressing = false;
                MouseMoving(e);
                Redraw();
            }
        }
    }

    public override void ScrollUp(int Count = 1)
    {
        if (!IsVisible() && !KeepInvisible) return;
        double px = ScrollStep * Count;
        if (MinScrollStep != null && px < (int)MinScrollStep) px = (int)MinScrollStep;
        if (LinkedWidget != null) SetValue((LinkedWidget.ScrolledY - px) / (LinkedWidget.MaxChildHeight - LinkedWidget.Viewport.Height));
        else SetValue(Value - px / Size.Height);
    }

    public override void ScrollDown(int Count = 1)
    {
        if (!IsVisible() && !KeepInvisible) return;
        double px = ScrollStep * Count;
        if (MinScrollStep != null && px < (int)MinScrollStep) px = (int)MinScrollStep;
        if (LinkedWidget != null) SetValue((LinkedWidget.ScrolledY + px) / (LinkedWidget.MaxChildHeight - LinkedWidget.Viewport.Height));
        else SetValue(Value + px / Size.Height);
    }

    public override void SetValue(double value, bool CallEvent = true)
    {
        if (value < 0) value = 0;
        if (value > 1) value = 1;
        if (Value != value)
        {
            Value = value;
            if (LinkedWidget != null)
            {
                if (LinkedWidget.MaxChildHeight > LinkedWidget.Viewport.Height)
                {
                    LinkedWidget.ScrolledY = (int)Math.Round((LinkedWidget.MaxChildHeight - LinkedWidget.Viewport.Height) * Value);
                    LinkedWidget.UpdateBounds();
                }
            }
            if (CallEvent) OnValueChanged?.Invoke(new BaseEventArgs());
            Redraw();
        }
    }

    public override void MouseWheel(MouseEventArgs e)
    {
        // If a HScrollBar exists
        if (LinkedWidget != null && LinkedWidget.HScrollBar != null)
        {
            // Return if pressing shift (i.e. HScrollBar will scroll instead)
            if (Input.Press(Keycode.SHIFT)) return;
        }
        if (LinkedWidget != null && !LinkedWidget.VAutoScroll) return;
        base.MouseWheel(e);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["arrow2"].Y = Size.Height - Arrow2Size;
    }
}
