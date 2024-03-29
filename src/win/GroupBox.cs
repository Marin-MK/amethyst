﻿using odl;

namespace amethyst.Windows;

public class GroupBox : TextWidget
{
    public int LineY = 6;

    public GroupBox(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(Viewport);
        Sprites["text"] = new Sprite(Viewport);
        Sprites["text"].X = 9;
        Sprites["text"].Y = -2;
    }

    protected override void DrawText()
    {
        Sprites["text"].Bitmap?.Dispose();
        if (!string.IsNullOrEmpty(Text))
        {
            Size s = Font.TextSize(Text);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = Font;
            Sprites["text"].Bitmap.DrawText(Text, TextColor);
            Sprites["text"].Bitmap.Lock();
        }
        base.DrawText();
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap(Size);
        Sprites["box"].Bitmap.Unlock();
        Sprites["box"].Bitmap.DrawRect(0, LineY, Size.Width, Size.Height - LineY, SystemColors.LightBorderColor);
        Size s = Font.TextSize(Text);
        Sprites["box"].Bitmap.DrawLine(7, LineY, s.Width + 11, LineY, Color.ALPHA);
        Sprites["box"].Bitmap.Lock();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Redraw();
    }
}
