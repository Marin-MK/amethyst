using System;
using odl;

namespace amethyst;

public class TextBox : ActivatableWidget
{
    public string Text { get { return TextArea.Text; } }
    public int CaretIndex { get { return TextArea.CaretIndex; } }
    public int SelectionStartIndex { get { return TextArea.SelectionStartIndex; } }
    public int SelectionEndIndex { get { return TextArea.SelectionEndIndex; } }
    public Color TextColor { get { return TextArea.TextColor; } }
    public int TextY { get { return TextArea.TextY; } }
    public int CaretHeight { get { return TextArea.CaretHeight; } }

    public TextArea TextArea;

    public BaseEvent OnTextChanged { get { return TextArea.OnTextChanged; } set { TextArea.OnTextChanged = value; } }

    public TextBox(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(this.Viewport);
        TextArea = new TextArea(this);
        TextArea.SetPosition(3, 3);
        TextArea.SetCaretHeight(13);
        TextArea.SetZIndex(1);
    }

    public void SetFont(Font Font)
    {
        TextArea.SetFont(Font);
    }

    public void SetText(string Text)
    {
        TextArea.SetText(Text);
    }

    public void SetTextY(int TextY)
    {
        TextArea.SetTextY(TextY);
    }

    public void SetCaretHeight(int CaretHeight)
    {
        TextArea.SetCaretHeight(CaretHeight);
    }

    public void SetTextColor(Color TextColor)
    {
        TextArea.SetTextColor(TextColor);
    }

    public void SetCaretIndex(int Index)
    {
        TextArea.CaretIndex = Index;
        TextArea.RepositionSprites();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(Size.Width - 6, Size.Height - 6);
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!WidgetIM.Hovering && TextArea.SelectedWidget)
        {
            Window.UI.SetSelectedWidget(null);
        }
    }

    public override object GetValue(string Identifier)
    {
        return this.Text;
    }

    public override void SetValue(string Identifier, object Value)
    {
        this.SetText((string)Value);
    }
}
