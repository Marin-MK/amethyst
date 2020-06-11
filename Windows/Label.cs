using odl;
using System;
using System.Collections.Generic;
using System.Text;

namespace amethyst.Windows
{
    public class Label : TextWidget
    {
        public Label(IContainer Parent) : base(Font.Get("Windows/segoeui", 12), Parent)
        {
            Sprites["text"] = new Sprite(this.Viewport);
        }

        protected override void DrawText()
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                Size s = this.Font.TextSize(this.Text);
                this.SetSize(s);
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
    }
}
