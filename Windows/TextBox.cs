using odl;
using System;
using System.Collections.Generic;
using System.Text;

namespace amethyst.Windows
{
    public class TextBox : amethyst.TextBox
    {
        public TextBox(IContainer Parent) : base(Parent)
        {
            SetTextColor(Color.BLACK);
            SetTextY(0);
            SetCaretHeight(14);
            MinimumSize.Height = 23;
            MaximumSize.Height = 23;
        }

        protected override void Draw()
        {
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
    }
}
