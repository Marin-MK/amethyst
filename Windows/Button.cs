using odl;

namespace amethyst.Windows;

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

    public Button(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(this.Viewport);
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].Name = "text sprite";
    }

    protected override void DrawText()
    {
        Sprites["text"].Bitmap?.Dispose();
        if (!string.IsNullOrEmpty(this.Text))
        {
            Size s = this.Font.TextSize(this.Text);
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
        for (int i = 0; i < thickness; i++) Sprites["box"].Bitmap.DrawRect(i, i, Size.Width - i * 2, Size.Height - i * 2, bordercolor);
        Sprites["box"].Bitmap.FillRect(thickness, thickness, Size.Width - thickness * 2, Size.Height - thickness * 2, fillercolor);
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (!string.IsNullOrEmpty(this.Text) && this.Font != null)
        {
            Size s = this.Font.TextSize(this.Text);
            Sprites["text"].X = Size.Width / 2 - s.Width / 2;
            Sprites["text"].Y = Size.Height / 2 - s.Height / 2 - 1;
        }
        Redraw();
    }
}
