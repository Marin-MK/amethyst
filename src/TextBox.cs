using odl;

namespace amethyst;

public class TextBox : ActivatableWidget
{
    public string Text => TextArea.Text;
    public int CaretIndex => TextArea.CaretIndex;
    public int SelectionStartIndex => TextArea.SelectionStartIndex;
    public int SelectionEndIndex => TextArea.SelectionEndIndex;
    public Color TextColor => TextArea.TextColor;
    public int TextY => TextArea.TextY;
    public int CaretHeight => TextArea.CaretHeight;
    public bool ShowDisabledText => TextArea.ShowDisabledText;
    public bool DeselectOnEnterPressed => TextArea.DeselectOnEnterPressed;

    public TextArea TextArea;

    public TextEvent OnTextChanged { get => TextArea.OnTextChanged; set => TextArea.OnTextChanged = value; }
    public BaseEvent OnEnterPressed { get => TextArea.OnEnterPressed; set => TextArea.OnEnterPressed = value; }

    public TextBox(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(Viewport);
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

    public void SetShowDisabledText(bool ShowDisabledText)
    {
        TextArea.SetShowDisabledText(ShowDisabledText);
    }

    public void SetDeselectOnEnterPressed(bool DeselectOnEnterPressed)
    {
        TextArea.SetDeselectOnEnterPress(DeselectOnEnterPressed);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(Size.Width - 6, Size.Height - 6);
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Mouse.Inside && TextArea.SelectedWidget)
        {
            Window.UI.SetSelectedWidget(null);
        }
    }

    public TextAreaState GetState()
    {
        return TextArea.GetState();
    }

    public void SetState(TextAreaState State)
    {
        TextArea.SetState(State);
    }
}
