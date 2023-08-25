using odl;

namespace amethyst.src.Windows;

public class Button : ActivatableTextWidget
{
    public Color BorderColorInactive { get; protected set; } = SystemColors.ControlBorderColorInactive;
    public Color FillerColorInactive { get; protected set; } = SystemColors.ControlFillerColorInactive;

    public Color BorderColorHovering { get; protected set; } = SystemColors.ControlBorderColorHovering;
    public Color FillerColorHovering { get; protected set; } = SystemColors.ControlFillerColorHovering;

    public Color BorderColorPressing { get; protected set; } = SystemColors.ControlBorderColorPressing;
    public Color FillerColorPressing { get; protected set; } = SystemColors.ControlFillerColorPressing;

    public Color BorderColorSelected { get; protected set; } = SystemColors.ControlBorderColorSelected;
    public Color FillerColorSelected { get; protected set; } = SystemColors.ControlFillerColorSelected;

    public BlendMode? BlendMode { get; protected set; }
    public bool Enabled { get; protected set; } = true;

    public Button(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(Viewport);
        Sprites["text"] = new Sprite(Viewport);
        Sprites["text"].Name = "text sprite";
    }

    public void SetBlendMode(BlendMode? BlendMode)
    {
        if (this.BlendMode != BlendMode)
        {
            this.BlendMode = BlendMode;
            RedrawText();
        }
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            RedrawText();
        }
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
            Sprites["text"].Bitmap.DrawText(Text, new Color(TextColor.Red, TextColor.Green, TextColor.Blue, (byte)(Enabled ? 255 : 128)));
            Sprites["text"].Bitmap.BlendMode = BlendMode ?? Bitmap.DefaultBlendMode;
            Sprites["text"].Bitmap.Lock();
            Sprites["text"].X = Size.Width / 2 - s.Width / 2;
            Sprites["text"].Y = Size.Height / 2 - s.Height / 2 - 1;
        }
        base.DrawText();
    }

    protected override void Draw()
    {
        Sprites["box"].Bitmap?.Dispose();
        if (Size.Width < 2 || Size.Height < 2) return;
        Sprites["box"].Bitmap = new Bitmap(Size);
        Sprites["box"].Bitmap.Unlock();
        Color bordercolor = Pressing ? BorderColorPressing : Hovering ? BorderColorHovering : BorderColorInactive;
        Color fillercolor = Pressing ? FillerColorPressing : Hovering ? FillerColorHovering : FillerColorInactive;
        int thickness = 1;
        if (!Pressing && !Hovering && SelectedWidget)
        {
            bordercolor = BorderColorSelected;
            fillercolor = FillerColorSelected;
            thickness = 2;
        }
        if (!Enabled)
        {
            bordercolor = BorderColorInactive;
            fillercolor = FillerColorInactive;
            thickness = 1;
        }
        for (int i = 0; i < thickness; i++) Sprites["box"].Bitmap.DrawRect(i, i, Size.Width - i * 2, Size.Height - i * 2, bordercolor);
        Sprites["box"].Bitmap.FillRect(thickness, thickness, Size.Width - thickness * 2, Size.Height - thickness * 2, fillercolor);
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (!string.IsNullOrEmpty(Text) && Font != null)
        {
            Size s = Font.TextSize(Text);
            Sprites["text"].X = Size.Width / 2 - s.Width / 2;
            Sprites["text"].Y = Size.Height / 2 - s.Height / 2 - 1;
        }
        Redraw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        if (Enabled) base.MouseDown(e);
    }

    public override void MouseUp(MouseEventArgs e)
    {
        if (Enabled) base.MouseUp(e);
    }
}
