using odl;
using System.Collections.Generic;

namespace amethyst.src.Windows;

public class Label : TextWidget
{
    public Label(IContainer Parent) : base(Parent)
    {
        Sprites["text"] = new Sprite(Viewport);
    }

    protected override void DrawText()
    {
        Sprites["text"].Bitmap?.Dispose();
        if (!string.IsNullOrEmpty(Text))
        {
            Size s = Font.TextSize(Text);
            SetSize(s);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = Font;
            Sprites["text"].Bitmap.DrawText(Text, TextColor, DrawOptions);
            Sprites["text"].Bitmap.BlendMode = BlendMode ?? Bitmap.DefaultBlendMode;
            Sprites["text"].Bitmap.Lock();
            Sprites["text"].X = Size.Width / 2 - s.Width / 2;
            Sprites["text"].Y = Size.Height / 2 - s.Height / 2 - 1;
        }
        base.DrawText();
    }
}

public class MultilineLabel : TextWidget
{
    public int? LineHeight { get; protected set; }

    public MultilineLabel(IContainer Parent) : base(Parent)
    {
        Sprites["text"] = new Sprite(Viewport);
    }

    public void SetLineHeight(int? LineHeight)
    {
        if (this.LineHeight != LineHeight)
        {
            this.LineHeight = LineHeight;
            RedrawText();
        }
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
            List<string> Lines = FormatString(Font, Text, Size.Width);
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            SetSize(Size.Width, (LineHeight ?? Font.Size + 2) * Lines.Count + 6);
            Sprites["text"].Bitmap = new Bitmap(Size);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = Font;
            for (int i = 0; i < Lines.Count; i++)
            {
                Sprites["text"].Bitmap.DrawText(Lines[i], 0, (LineHeight ?? Font.Size + 2) * i, TextColor, DrawOptions);
            }
            Sprites["text"].Bitmap.BlendMode = BlendMode ?? Bitmap.DefaultBlendMode;
            Sprites["text"].Bitmap.Lock();
        }
    }
}
