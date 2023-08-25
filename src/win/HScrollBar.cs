using odl;

namespace amethyst.Windows;

public class HScrollBar : BasicHScrollBar
{
    public HScrollBar(IContainer Parent) : base(Parent)
    {
        SetBackgroundColor(SystemColors.WindowBackground);
        SetSliderHeight(16);
    }

    protected override void Draw()
    {
        base.Draw();

        Color sc = SliderDragging ? SystemColors.SliderColorPressing : SliderHovering ? SystemColors.SliderColorHovering : SystemColors.SliderColorInactive;
        Sprites["slider"].Bitmap?.Dispose();
        Sprites["slider"].Bitmap = new Bitmap(RealSliderWidth, SliderHeight - 1);
        Sprites["slider"].Bitmap.Unlock();
        Sprites["slider"].Bitmap.FillRect(RealSliderWidth - 1, SliderHeight - 1, sc);
        Sprites["slider"].Bitmap.Lock();
        Sprites["slider"].X = Arrow1Size + RealSliderPosition;

        Color tac = Arrow1Pressing ? SystemColors.ControlBackground : Arrow1Hovering ? Color.BLACK : SystemColors.SliderArrowColorArrow;
        Sprites["arrow1"].Bitmap?.Dispose();
        Sprites["arrow1"].Bitmap = new Bitmap(Arrow1Size, Size.Height);
        Sprites["arrow1"].Bitmap.Unlock();
        Sprites["arrow1"].Bitmap.FillRect(Arrow1Size, Size.Height, Arrow1Pressing ? SystemColors.SliderArrowColorPressing : Arrow1Hovering ? SystemColors.SliderArrowColorHovering : SystemColors.SliderArrowColorInactive);
        Sprites["arrow1"].Bitmap.DrawLine(9, 4, 6, 7, tac);
        Sprites["arrow1"].Bitmap.DrawLine(10, 4, 7, 7, tac);
        Sprites["arrow1"].Bitmap.DrawLine(11, 4, 8, 7, tac);
        Sprites["arrow1"].Bitmap.DrawLine(7, 8, 9, 10, tac);
        Sprites["arrow1"].Bitmap.DrawLine(8, 8, 10, 10, tac);
        Sprites["arrow1"].Bitmap.DrawLine(9, 8, 11, 10, tac);
        Sprites["arrow1"].Bitmap.Lock();

        Color dac = Arrow2Pressing ? SystemColors.ControlBackground : Arrow2Hovering ? Color.BLACK : SystemColors.SliderArrowColorArrow;
        Sprites["arrow2"].Bitmap?.Dispose();
        Sprites["arrow2"].Bitmap = new Bitmap(Arrow2Size, Size.Height);
        Sprites["arrow2"].Bitmap.Unlock();
        Sprites["arrow2"].Bitmap.FillRect(Arrow2Size, Size.Height, Arrow2Pressing ? SystemColors.SliderArrowColorPressing : Arrow2Hovering ? SystemColors.SliderArrowColorHovering : SystemColors.SliderArrowColorInactive);
        Sprites["arrow2"].Bitmap.DrawLine(6, 4, 9, 7, dac);
        Sprites["arrow2"].Bitmap.DrawLine(7, 4, 10, 7, dac);
        Sprites["arrow2"].Bitmap.DrawLine(8, 4, 11, 7, dac);
        Sprites["arrow2"].Bitmap.DrawLine(8, 8, 6, 10, dac);
        Sprites["arrow2"].Bitmap.DrawLine(9, 8, 7, 10, dac);
        Sprites["arrow2"].Bitmap.DrawLine(10, 8, 8, 10, dac);
        Sprites["arrow2"].Bitmap.Lock();
        base.Draw();
    }
}
