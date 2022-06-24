using System.Collections.Generic;
using odl;

namespace amethyst;

public class HelpText : Widget
{
    public Font Font { get; protected set; } = Font.Get("ProductSans-M", 11);
    public int MaxWidth { get; protected set; } = 300;

    public HelpText(IContainer Parent) : base(Parent)
    {
        Sprites["shadow"] = new Sprite(this.Viewport);
        Sprites["filler"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(84, 80, 156)));
        Sprites["filler"].X = 7;
        Sprites["filler"].Y = 7;
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].X = Sprites["text"].Y = 14;
    }

    public virtual void SetText(string Text)
    {
        List<string> Lines = new List<string>();
        string lastline = "";
        string lastword = "";
        string splitters = " `~!@#$%^&*()-=+[]{}\\|;:'\",.<>/?\n";
        for (int i = 0; i < Text.Length; i++)
        {
            if (Text[i] == '\n')
            {
                Lines.Add(lastline + lastword);
                lastline = "";
                lastword = "";
                continue;
            }
            lastword += Text[i];
            if (splitters.Contains(Text[i]))
            {
                lastline += lastword;
                lastword = "";
            }
            if (Font.TextSize(lastline + lastword).Width > MaxWidth)
            {
                Lines.Add(lastline);
                lastline = lastword;
                lastword = "";
            }
        }
        if (!string.IsNullOrEmpty(lastword)) lastline += lastword;
        if (!string.IsNullOrEmpty(lastline)) Lines.Add(lastline);
        int w = 0;
        int h = Lines.Count * 24;
        for (int i = 0; i < Lines.Count; i++)
        {
            int linewidth = Font.TextSize(Lines[i]).Width;
            if (linewidth > w) w = linewidth;
        }
        if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
        Sprites["text"].Bitmap = new Bitmap(w, h);
        SetSize(w + 28, h + 20);
        Sprites["text"].Bitmap.Font = this.Font;
        Sprites["text"].Bitmap.Unlock();
        for (int i = 0; i < Lines.Count; i++)
        {
            Sprites["text"].Bitmap.DrawText(Lines[i], 0, i * 24, Color.WHITE);
        }
        Sprites["text"].Bitmap.Lock();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["shadow"].Bitmap?.Dispose();
        Sprites["shadow"].Bitmap = new Bitmap(Size);
        Sprites["shadow"].Bitmap.Unlock();
        Sprites["shadow"].Bitmap.FillGradientRectOutside(
            new Rect(Size),
            new Rect(7, 7, Size.Width - 14, Size.Height - 14),
            new Color(0, 0, 0, 200),
            Color.ALPHA,
            false
        );
        Sprites["shadow"].Bitmap.Lock();
        (Sprites["filler"].Bitmap as SolidBitmap).SetSize(Size.Width - 14, Size.Height - 14);
    }
}
