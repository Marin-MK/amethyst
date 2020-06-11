using System;
using System.Collections.Generic;
using System.Text;
using odl;

namespace amethyst.Windows
{
    public class VScrollBar : BasicVScrollBar
    {
        public VScrollBar(IContainer Parent) : base(Parent)
        {
            this.SetBackgroundColor(SystemColors.WindowBackground);
            this.SetSliderWidth(16);
        }

        protected override void Draw()
        {
            base.Draw();

            Color sc = SliderDragging ? SystemColors.SliderColorPressing : SliderHovering ? SystemColors.SliderColorHovering : SystemColors.SliderColorInactive;
            Sprites["slider"].Bitmap?.Dispose();
            Sprites["slider"].Bitmap = new Bitmap(this.SliderWidth - 1, this.RealSliderHeight);
            Sprites["slider"].Bitmap.Unlock();
            Sprites["slider"].Bitmap.FillRect(this.SliderWidth - 1, this.RealSliderHeight - 1, sc);
            Sprites["slider"].Bitmap.Lock();
            Sprites["slider"].Y = Arrow1Size + this.RealSliderPosition;

            Color tac = Arrow1Pressing ? SystemColors.ControlBackground : Arrow1Hovering ? Color.BLACK : SystemColors.SliderArrowColorArrow;
            Sprites["arrow1"].Bitmap?.Dispose();
            Sprites["arrow1"].Bitmap = new Bitmap(Size.Width, Arrow1Size);
            Sprites["arrow1"].Bitmap.Unlock();
            Sprites["arrow1"].Bitmap.FillRect(Size.Width, Arrow1Size, Arrow1Pressing ? SystemColors.SliderArrowColorPressing : Arrow1Hovering ? SystemColors.SliderArrowColorHovering : SystemColors.SliderArrowColorInactive);
            Sprites["arrow1"].Bitmap.DrawLine(4, 9, 7, 6, tac);
            Sprites["arrow1"].Bitmap.DrawLine(4, 10, 7, 7, tac);
            Sprites["arrow1"].Bitmap.DrawLine(4, 11, 7, 8, tac);
            Sprites["arrow1"].Bitmap.DrawLine(8, 7, 10, 9, tac);
            Sprites["arrow1"].Bitmap.DrawLine(8, 8, 10, 10, tac);
            Sprites["arrow1"].Bitmap.DrawLine(8, 9, 10, 11, tac);
            Sprites["arrow1"].Bitmap.Lock();

            Color dac = Arrow2Pressing ? SystemColors.ControlBackground : Arrow2Hovering ? Color.BLACK : SystemColors.SliderArrowColorArrow;
            Sprites["arrow2"].Bitmap?.Dispose();
            Sprites["arrow2"].Bitmap = new Bitmap(Size.Width, Arrow2Size);
            Sprites["arrow2"].Bitmap.Unlock();
            Sprites["arrow2"].Bitmap.FillRect(Size.Width, Arrow2Size, Arrow2Pressing ? SystemColors.SliderArrowColorPressing : Arrow2Hovering ? SystemColors.SliderArrowColorHovering : SystemColors.SliderArrowColorInactive);
            Sprites["arrow2"].Bitmap.DrawLine(4, 6, 7, 9, dac);
            Sprites["arrow2"].Bitmap.DrawLine(4, 7, 7, 10, dac);
            Sprites["arrow2"].Bitmap.DrawLine(4, 8, 7, 11, dac);
            Sprites["arrow2"].Bitmap.DrawLine(8, 8, 10, 6, dac);
            Sprites["arrow2"].Bitmap.DrawLine(8, 9, 10, 7, dac);
            Sprites["arrow2"].Bitmap.DrawLine(8, 10, 10, 8, dac);
            Sprites["arrow2"].Bitmap.Lock();
            base.Draw();
        }
    }
}
