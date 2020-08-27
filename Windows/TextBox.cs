using System;
using odl;

namespace amethyst.Windows
{
    public class TextBox : ActivatableWidget
    {
        public string Text { get { return TextArea.Text; } }
        public int CaretIndex { get { return TextArea.CaretIndex; } }
        public int SelectionStartIndex { get { return TextArea.SelectionStartIndex; } }
        public int SelectionEndIndex { get { return TextArea.SelectionEndIndex; } }

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

        public void SetInitialText(string Text)
        {
            if (this.Text != Text)
                TextArea.SetInitialText(Text);
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

        protected override void Draw()
        {
            Console.WriteLine($"Selected: {SelectedWidget}");
            if (Sprites["box"].Bitmap != null) Sprites["box"].Bitmap.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            Color outline = SelectedWidget || Pressing ? SystemColors.SelectionColor : Hovering ? new Color(23, 23, 23) : new Color(122, 122, 122);
            Color filler = Color.WHITE;
            Sprites["box"].Bitmap.DrawRect(Size, outline);
            Sprites["box"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, filler);
            Sprites["box"].Bitmap.Lock();
            base.Draw();
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
            this.SetInitialText((string) Value);
        }
    }
}
