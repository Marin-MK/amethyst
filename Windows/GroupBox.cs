using System;
using System.Collections.Generic;
using System.Text;
using odl;

namespace amethyst.Windows
{
    public class GroupBox : TextWidget
    {
        public GroupBox(IContainer Parent) : base(Font.Get("Windows/segoeui", 12), Parent)
        {
            Sprites["box"] = new Sprite(this.Viewport);
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].X = 9;
            Sprites["text"].Y = -2;
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
            }
            base.DrawText();
        }

        protected override void Draw()
        {
            base.Draw();
            Sprites["box"].Bitmap?.Dispose();
            Sprites["box"].Bitmap = new Bitmap(Size);
            Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.DrawRect(0, 6, Size.Width, Size.Height - 6, SystemColors.LightBorderColor);
            Size s = this.Font.TextSize(this.Text);
            Sprites["box"].Bitmap.DrawLine(7, 6, s.Width + 11, 6, Color.ALPHA);
            Sprites["box"].Bitmap.Lock();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            this.Redraw();
        }
    }
}
