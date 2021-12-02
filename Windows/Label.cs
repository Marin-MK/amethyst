using odl;
using System;
using System.Collections.Generic;
using System.Text;

namespace amethyst.Windows;

public class Label : TextWidget
{
    public Label(IContainer Parent) : base(Parent)
    {
        Sprites["text"] = new Sprite(this.Viewport);
    }

    protected override void DrawText()
    {
        Sprites["text"].Bitmap?.Dispose();
        if (!string.IsNullOrEmpty(this.Text))
        {
            Size s = this.Font.TextSize(this.Text);
            this.SetSize(s);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            Sprites["text"].Bitmap.DrawText(this.Text, this.TextColor);
            Sprites["text"].Bitmap.Lock();
            Sprites["text"].X = Size.Width / 2 - s.Width / 2;
            Sprites["text"].Y = Size.Height / 2 - s.Height / 2 - 1;
        }
        base.DrawText();
    }
}

public class MultilineLabel : TextWidget
{
    public MultilineLabel(IContainer Parent) : base(Parent)
    {
        Sprites["text"] = new Sprite(this.Viewport);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RedrawText();
    }

    protected override void DrawText()
    {
        Sprites["text"].Bitmap?.Dispose();
        if (!string.IsNullOrEmpty(Text))
        {
            List<string> Lines = FormatString(this.Font, Text, Size.Width);
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            SetSize(Size.Width, (Font.Size + 4) * Lines.Count);
            Sprites["text"].Bitmap = new Bitmap(Size);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = this.Font;
            for (int i = 0; i < Lines.Count; i++)
            {
                Sprites["text"].Bitmap.DrawText(Lines[i], 0, (Font.Size + 2) * i, this.TextColor);
            }
            Sprites["text"].Bitmap.Lock();
        }
    }
}
