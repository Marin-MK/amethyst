using odl;

namespace amethyst;

public class ActivatableTextWidget : ActivatableWidget
{
    public string Text { get; protected set; } = "";
    public Color TextColor { get; protected set; } = Color.BLACK;
    public Font Font { get; protected set; }

    protected bool DrawnText = false;

    public ActivatableTextWidget(IContainer Parent) : base(Parent)
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
}
