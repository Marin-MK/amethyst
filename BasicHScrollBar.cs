using System;
using System.Collections.Generic;
using System.Text;
using odl;

namespace amethyst
{
    public abstract class BasicHScrollBar : ScrollBar
    {
        protected int RealSliderWidth { get { return (int) Math.Round((Size.Width - Arrow1Size - Arrow2Size) * this.SliderSize); } }
        protected int RealSliderPosition { get { return (int) Math.Round((Size.Width - Arrow1Size - Arrow2Size - RealSliderWidth) * this.Value); } }

        protected int SliderY = 0;
        protected int SliderHeight = 8;

        public BasicHScrollBar(IContainer Parent) : base(Parent)
        {
            this.Size = new Size(60, 17);
            this.OnWidgetSelected += WidgetSelected;
        }

        public void SetSliderHeight(int SliderHeight)
        {
            MinimumSize.Height = SliderHeight;
            MaximumSize.Height = SliderHeight;
            this.SliderHeight = SliderHeight;
            this.SetHeight(SliderHeight);
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
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
            }
            else
            {
                // Dragging slider
                SliderDragging = true;
                int rx = e.X - Viewport.X - Arrow1Size - DragOffset;
                int ry = e.Y - Viewport.Y;
                int available = Size.Width - Arrow1Size - Arrow2Size - RealSliderWidth;
                this.SetValue((double) rx / available);
            }
            this.Redraw();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
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
                    this.Redraw();
                    ScrollUp();
                    SetTimer("left", 300);
                    return;
                }
                else if (rx >= Size.Width - Arrow2Size && rx < Size.Width)
                {
                    Arrow2StartedPressing = true;
                    Arrow2Pressing = true;
                    ScrollDown();
                    this.Redraw();
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
            if (e.LeftButton != e.OldLeftButton)
            {
                DragOffset = -1;
                Input.ReleaseMouse();
                if (SliderDragging)
                {
                    SliderDragging = false;
                    this.Redraw();
                }
                if (Arrow1StartedPressing)
                {
                    Arrow1StartedPressing = false;
                    Arrow1Pressing = false;
                    MouseMoving(e);
                    this.Redraw();
                }
                if (Arrow2StartedPressing)
                {
                    Arrow2StartedPressing = false;
                    Arrow2Pressing = false;
                    MouseMoving(e);
                    this.Redraw();
                }
            }
        }

        public override void ScrollUp(int Count = 1)
        {
            if (!IsVisible()) return;
            if (LinkedWidget != null) this.SetValue(((double) LinkedWidget.ScrolledX - ScrollStep * Count) / (LinkedWidget.MaxChildWidth - LinkedWidget.Viewport.Width));
            else this.SetValue(this.Value - (double) ScrollStep * Count / Size.Width);
        }

        public override void ScrollDown(int Count = 1)
        {
            if (!IsVisible()) return;
            if (LinkedWidget != null) this.SetValue(((double) LinkedWidget.ScrolledX + ScrollStep * Count) / (LinkedWidget.MaxChildWidth - LinkedWidget.Viewport.Width));
            else this.SetValue(this.Value + (double) ScrollStep * Count / Size.Width);
        }

        public override void SetValue(double value, bool CallEvent = true)
        {
            if (value < 0) value = 0;
            if (value > 1) value = 1;
            if (this.Value != value)
            {
                this.Value = value;
                if (LinkedWidget != null)
                {
                    if (LinkedWidget.MaxChildWidth > LinkedWidget.Viewport.Width)
                    {
                        LinkedWidget.ScrolledX = (int) Math.Round((LinkedWidget.MaxChildWidth - LinkedWidget.Viewport.Width) * this.Value);
                        LinkedWidget.UpdateBounds();
                    }
                }
                if (CallEvent) this.OnValueChanged?.Invoke(new BaseEventArgs());
                Redraw();
            }
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            // If a VScrollBar exists
            if (LinkedWidget != null && LinkedWidget.VScrollBar != null)
            {
                // Return if not pressing shift (i.e. VScrollBar will scroll instead)
                if (!Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) && !Input.Press(SDL2.SDL.SDL_Keycode.SDLK_RSHIFT)) return;
            }
            base.MouseWheel(e);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            Sprites["arrow2"].X = Size.Width - Arrow2Size;
        }
    }
}
