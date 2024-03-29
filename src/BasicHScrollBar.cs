﻿using System;
using odl;

namespace amethyst;

public abstract class BasicHScrollBar : ScrollBar
{
    protected int RealSliderWidth { get { return (int)Math.Round((Size.Width - Arrow1Size - Arrow2Size) * SliderSize); } }
    protected int RealSliderPosition { get { return (int)Math.Round((Size.Width - Arrow1Size - Arrow2Size - RealSliderWidth) * Value); } }

    protected int SliderY = 0;
    protected int SliderHeight = 8;

    public BasicHScrollBar(IContainer Parent) : base(Parent)
    {
        Size = new Size(60, 17);
        OnWidgetSelected += WidgetSelected;
    }

    public void SetSliderHeight(int SliderHeight)
    {
        MinimumSize.Height = SliderHeight;
        MaximumSize.Height = SliderHeight;
        this.SliderHeight = SliderHeight;
        SetHeight(SliderHeight);
    }

    public override void ScrollLeft(int Count = 1)
    {
        ScrollDown(Count);
    }

    public override void ScrollRight(int Count = 1)
    {
        ScrollUp(Count);
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
            if (ry >= SliderY && ry < SliderY + SliderHeight)
            {
                if (rx >= Arrow1Size + RealSliderPosition && rx < Arrow1Size + RealSliderPosition + RealSliderWidth)
                {
                    SliderHovering = true;
                }
            }
            if (ry >= 0 && ry < Size.Height)
            {
                if (rx >= 0 && rx < Arrow1Size)
                {
                    Arrow1Hovering = true;
                    if (Arrow1StartedPressing) Arrow1Pressing = true;
                }
                else if (rx >= Size.Width - Arrow2Size && rx < Size.Width)
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
            int rx = e.X - Viewport.X - Arrow1Size - DragOffset;
            int ry = e.Y - Viewport.Y;
            int available = Size.Width - Arrow1Size - Arrow2Size - RealSliderWidth;
            SetValue((double)rx / available);
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Pressable) return;
        if (e.LeftButton == e.OldLeftButton) return;
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        if (ry < SliderY || ry >= SliderHeight) return;
        if (rx < Arrow1Size || rx >= Size.Width - Arrow2Size)
        {
            if (rx >= 0 && rx < Arrow1Size)
            {
                Arrow1StartedPressing = true;
                Arrow1Pressing = true;
                Redraw();
                ScrollUp();
                SetTimer("left", 300);
                return;
            }
            else if (rx >= Size.Width - Arrow2Size && rx < Size.Width)
            {
                Arrow2StartedPressing = true;
                Arrow2Pressing = true;
                ScrollDown();
                Redraw();
                SetTimer("right", 300);
                return;
            }
        }
        rx -= Arrow1Size;
        if (rx >= RealSliderPosition && rx < RealSliderPosition + RealSliderWidth)
        {
            DragOffset = rx - RealSliderPosition;
            Input.CaptureMouse();
        }
    }

    public override void Update()
    {
        base.Update();
        if (TimerExists("left"))
        {
            if (Arrow1StartedPressing)
            {
                if (TimerPassed("left") && Arrow1Pressing)
                {
                    ScrollUp();
                    DestroyTimer("left");
                    SetTimer("left", 50);
                }
            }
            else
            {
                DestroyTimer("left");
            }
        }
        if (TimerExists("right"))
        {
            if (Arrow2StartedPressing)
            {
                if (TimerPassed("right") && Arrow2Pressing)
                {
                    ScrollDown();
                    DestroyTimer("right");
                    SetTimer("right", 50);
                }
            }
            else
            {
                DestroyTimer("right");
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
        if (LinkedWidget != null) SetValue((LinkedWidget.ScrolledX - px) / (LinkedWidget.MaxChildWidth - (LinkedWidget.Viewport.Width - LinkedWidget.ChildPadding.Left + LinkedWidget.ChildPadding.Right)));
        else SetValue(Value - px / Size.Width);
    }

    public override void ScrollDown(int Count = 1)
    {
        if (!IsVisible() && !KeepInvisible) return;
        double px = ScrollStep * Count;
        if (MinScrollStep != null && px < (int)MinScrollStep) px = (int)MinScrollStep;
        if (LinkedWidget != null) SetValue((LinkedWidget.ScrolledX + px) / (LinkedWidget.MaxChildWidth - (LinkedWidget.Viewport.Width - LinkedWidget.ChildPadding.Left + LinkedWidget.ChildPadding.Right)));
        else SetValue(Value + px / Size.Width);
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
                int effW = LinkedWidget.Viewport.Width - LinkedWidget.ChildPadding.Left + LinkedWidget.ChildPadding.Right;
                if (LinkedWidget.MaxChildWidth > effW)
                {
                    LinkedWidget.ScrolledX = (int) Math.Round((LinkedWidget.MaxChildWidth - effW) * Value);
                    LinkedWidget.UpdateBounds();
                }
            }
            if (CallEvent) OnValueChanged?.Invoke(new BaseEventArgs());
            Redraw();
        }
    }

    public override void MouseWheel(MouseEventArgs e)
    {
        // If a VScrollBar exists
        if (LinkedWidget != null && LinkedWidget.VScrollBar != null && e.WheelX == 0)
        {
            // Return if not pressing shift (i.e. VScrollBar will scroll instead)
            if (!Input.Press(Keycode.SHIFT)) return;
        }
        if (LinkedWidget != null && !LinkedWidget.HAutoScroll) return;
        base.MouseWheel(e);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["arrow2"].X = Size.Width - Arrow2Size;
    }
}
