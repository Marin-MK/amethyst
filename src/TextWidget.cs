using odl;

using System.Collections.Generic;

namespace amethyst;

public class TextWidget : Widget
{
    public string Text { get; protected set; } = "";
    public Color TextColor { get; protected set; } = Color.BLACK;
    public Font Font { get; protected set; }
    public BlendMode? BlendMode { get; protected set; }
    public DrawOptions DrawOptions { get; protected set; } = DrawOptions.LeftAlign;

    protected bool DrawnText = false;

    public TextWidget(IContainer Parent) : base(Parent)
    {
        SetText(GetType().Name);
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            RedrawText();
        }
    }

    public void SetTextColor(Color TextColor)
    {
        if (this.TextColor != TextColor)
        {
            this.TextColor = TextColor;
            RedrawText();
        }
    }

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            RedrawText();
        }
    }

    public void SetBlendMode(BlendMode? BlendMode)
    {
        if (this.BlendMode != BlendMode)
        {
            this.BlendMode = BlendMode;
            RedrawText();
        }
    }

    public void SetDrawOptions(DrawOptions DrawOptions)
    {
        if (this.DrawOptions != DrawOptions)
        {
            this.DrawOptions = DrawOptions;
            RedrawText();
        }
    }

    protected virtual void DrawText()
    {
        DrawnText = true;
    }

    protected override void Draw()
    {
        if (!DrawnText) DrawText();
        base.Draw();
    }

    public void RedrawText()
    {
        Redraw();
        DrawnText = false;
    }

    protected List<string> FormatString(Font f, string Text, int Width)
    {

        List<string> Lines = new List<string>();
        int startidx = 0;
        int lastsplittableindex = -1;
        for (int i = 0; i < Text.Length; i++)
        {
            char c = Text[i];
            string txt = Text.Substring(startidx, i - startidx + 1);
            Size s = f.TextSize(txt);
            if (c == '\n')
            {
                Lines.Add(Text.Substring(startidx, i - startidx));
                startidx = i + 1;
                if (i == Text.Length - 1) Lines.Add("");
                lastsplittableindex = -1;
            }
            else if (s.Width >= Width)
            {
                int endidx = lastsplittableindex == -1 ? i : lastsplittableindex + 1;
                Lines.Add(Text.Substring(startidx, endidx - startidx - 1));
                startidx = endidx - 1;
                lastsplittableindex = -1;
            }
            else if (c == ' ' || c == '-')
            {
                lastsplittableindex = i + 1;
            }
        }
        if (startidx != Text.Length)
        {
            Lines.Add(Text.Substring(startidx));
        }
        else if (Lines.Count == 0)
        {
            Lines.Add("");
        }
        return Lines;
    }
}
