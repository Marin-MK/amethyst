using odl;

namespace amethyst.src.Windows;

public class CheckBox : ActivatableTextWidget
{
    public bool Checked { get; protected set; } = false;
    public BlendMode? BlendMode { get; protected set; }

    public BaseEvent OnCheckChanged;

    public CheckBox(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(Viewport);
        Sprites["box"].Bitmap = new Bitmap(13, 13);
        Sprites["text"] = new Sprite(Viewport);
        Sprites["text"].X = 18;
        Sprites["text"].Y = -3;
        SetSize(71, 13);

        OnPressed += delegate (BaseEventArgs e)
        {
            SetChecked(!Checked);
        };
    }

    public void SetChecked(bool Checked)
    {
        if (this.Checked != Checked)
        {
            this.Checked = Checked;
            Redraw();
            OnCheckChanged?.Invoke(new BaseEventArgs());
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

    protected override void DrawText()
    {
        Sprites["text"].Bitmap?.Dispose();
        if (!string.IsNullOrEmpty(Text))
        {
            Size s = Font.TextSize(Text);
            int w = Sprites["text"].X + s.Width;
            int h = 13;
            SetWidth(Sprites["text"].X + s.Width + 5);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = Font;
            Sprites["text"].Bitmap.DrawText(Text, TextColor);
            Sprites["text"].Bitmap.BlendMode = BlendMode ?? Bitmap.DefaultBlendMode;
            Sprites["text"].Bitmap.Lock();
        }
        base.DrawText();
    }

    protected override void Draw()
    {
        Sprites["box"].Bitmap.Unlock();
        Sprites["box"].Bitmap.Clear();
        Bitmap bmp = Sprites["box"].Bitmap;
        if (Pressing)
        {
            bmp.DrawRect(0, 0, 13, 13, SystemColors.ControlBorderColorPressing);
            bmp.DrawLine(0, 0, 12, 0, SystemColors.ControlBorderColorHovering);
            bmp.FillRect(1, 1, 11, 11, SystemColors.ControlFillerColorPressing);
        }
        else if (Hovering)
        {
            bmp.DrawRect(0, 0, 13, 13, SystemColors.ControlBorderColorHovering);
            bmp.FillRect(1, 1, 11, 11, SystemColors.ControlFillerColorHovering);
        }
        else
        {
            bmp.DrawRect(0, 0, 13, 13, SystemColors.DarkControlBorderColorInactive);
            bmp.FillRect(1, 1, 11, 11, SystemColors.WindowBackground);
            if (Checked)
            {
                bmp.DrawLine(2, 6, 4, 8, new Color(72, 72, 72));
                bmp.DrawLine(5, 8, 10, 3, new Color(72, 72, 72));
                bmp.DrawLine(2, 7, 4, 9, new Color(107, 107, 107));
                bmp.DrawLine(5, 9, 10, 4, new Color(107, 107, 107));
                bmp.DrawLine(3, 6, 4, 7, new Color(226, 226, 226));
                bmp.DrawLine(5, 7, 9, 3, new Color(226, 226, 226));
                bmp.SetPixel(1, 6, new Color(184, 184, 184));
                bmp.SetPixel(11, 3, new Color(184, 184, 184));
            }
        }
        if (Checked && (Pressing || Hovering))
        {
            Color darkframe = Checked ? new Color(0, 84, 153) : new Color(0, 120, 215);
            Color darkleft = Checked ? new Color(64, 118, 173) : new Color(83, 150, 223);
            Color uppershadow = Checked ? new Color(106, 148, 191) : new Color(134, 178, 230);
            Color lowershadow = Checked ? new Color(150, 182, 215) : new Color(188, 211, 240);
            Color lightextra = Checked ? new Color(178, 206, 231) : new Color(223, 233, 248);
            bmp.DrawLine(2, 6, 4, 8, darkframe);
            bmp.DrawLine(5, 8, 10, 3, darkframe);
            bmp.DrawLine(2, 7, 4, 9, darkleft);
            bmp.DrawLine(5, 9, 10, 4, lowershadow);
            bmp.SetPixel(1, 5, lightextra);
            bmp.DrawLine(2, 5, 4, 7, lightextra);
            bmp.DrawLine(5, 7, 9, 3, uppershadow);
            bmp.SetPixel(1, 6, uppershadow);
            bmp.SetPixel(11, 3, lightextra);
        }
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }
}
