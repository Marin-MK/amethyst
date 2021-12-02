using odl;
using System;
using System.Collections.Generic;
using System.Text;

namespace amethyst.Windows;

public class RadioBox : ActivatableTextWidget
{
    public bool Checked { get; protected set; } = false;

    public BaseEvent OnCheckChanged;

    public RadioBox(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(this.Viewport);
        Sprites["box"].Bitmap = new Bitmap(13, 13);
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].X = 18;
        Sprites["text"].Y = -3;
        MinimumSize.Height = 13;
        MaximumSize.Height = 13;
        SetHeight(13);
        this.OnPressed += delegate (BaseEventArgs e)
        {
            this.SetChecked(true);
        };
    }

    public void SetChecked(bool Checked)
    {
        if (this.Checked != Checked)
        {
            this.Checked = Checked;
            if (this.Checked)
            {
                foreach (Widget w in Parent.Widgets)
                {
                    if (w != this && w is RadioBox && ((RadioBox)w).Checked) ((RadioBox)w).SetChecked(false);
                }
            }
            this.OnCheckChanged?.Invoke(new BaseEventArgs());
            this.Redraw();
        }
    }

    protected override void Draw()
    {
        base.Draw();
        Bitmap box = Sprites["box"].Bitmap;
        box.Unlock();
        box.Clear();
        #region Draw box
        Color c0 = Pressing ? new Color(204, 228, 247) : Hovering ? new Color(255, 255, 255) : Color.ALPHA;
        Color c1 = Pressing ? new Color(236, 237, 238) : Hovering ? new Color(236, 238, 239) : new Color(237, 237, 237);
        Color c2 = Pressing ? new Color(94, 145, 187) : Hovering ? new Color(94, 167, 225) : new Color(125, 125, 125);
        Color c3 = Pressing ? new Color(87, 146, 193) : Hovering ? new Color(109, 178, 232) : new Color(138, 138, 138);
        Color c4 = Pressing ? new Color(100, 149, 189) : Hovering ? new Color(100, 170, 226) : new Color(130, 130, 130);
        Color c5 = Pressing ? new Color(203, 227, 247) : Hovering ? new Color(254, 254, 255) : new Color(254, 254, 254);
        Color c6 = Pressing ? new Color(48, 118, 175) : Hovering ? new Color(60, 152, 224) : new Color(99, 99, 99);
        Color c7 = Pressing ? new Color(184, 204, 220) : Hovering ? new Color(184, 212, 235) : new Color(196, 196, 196);
        Color c8 = Pressing ? new Color(132, 177, 214) : Hovering ? new Color(165, 207, 241) : new Color(183, 183, 183);
        Color c9 = Pressing ? new Color(83, 138, 183) : Hovering ? new Color(83, 162, 224) : new Color(116, 116, 116);
        Color c10 = Pressing ? new Color(182, 213, 237) : Hovering ? new Color(228, 241, 251) : new Color(233, 233, 233);
        Color c11 = Pressing ? new Color(25, 100, 162) : Hovering ? new Color(25, 132, 217) : new Color(71, 71, 71);
        Color c12 = Pressing ? new Color(199, 225, 245) : Hovering ? new Color(249, 252, 254) : new Color(250, 250, 250);
        Color c13 = Pressing ? new Color(6, 88, 155) : Hovering ? new Color(6, 123, 216) : new Color(56, 56, 56);
        box.FillRect(1, 1, 11, 11, c0);
        box.SetPixel(1, 1, c1);
        box.SetPixel(1, 2, c2);
        box.SetPixel(2, 2, c3);
        box.SetPixel(2, 1, c4);
        box.SetPixel(3, 2, c5);
        box.SetPixel(3, 1, c6);
        box.SetPixel(3, 0, c7);
        box.SetPixel(4, 1, c8);
        box.SetPixel(4, 0, c9);
        box.SetPixel(5, 1, c10);
        box.SetPixel(5, 0, c11);
        box.SetPixel(6, 1, c12);
        box.SetPixel(6, 0, c13);
        box.SetPixel(7, 1, c10);
        box.SetPixel(7, 0, c11);
        box.SetPixel(8, 1, c8);
        box.SetPixel(8, 0, c9);
        box.SetPixel(9, 2, c5);
        box.SetPixel(9, 1, c6);
        box.SetPixel(9, 0, c7);
        box.SetPixel(10, 2, c3);
        box.SetPixel(10, 1, c4);
        box.SetPixel(11, 1, c1);
        box.SetPixel(11, 2, c2);
        box.SetPixel(11, 3, c6);
        box.SetPixel(12, 3, c7);
        box.SetPixel(11, 4, c8);
        box.SetPixel(12, 4, c9);
        box.SetPixel(11, 5, c10);
        box.SetPixel(12, 5, c11);
        box.SetPixel(11, 6, c12);
        box.SetPixel(12, 6, c13);
        box.SetPixel(11, 7, c10);
        box.SetPixel(12, 7, c11);
        box.SetPixel(11, 8, c8);
        box.SetPixel(12, 8, c9);
        box.SetPixel(12, 9, c7);
        box.SetPixel(11, 9, c6);
        box.SetPixel(10, 9, c5);
        box.SetPixel(11, 10, c4);
        box.SetPixel(10, 10, c3);
        box.SetPixel(9, 10, c5);
        box.SetPixel(11, 11, c1);
        box.SetPixel(10, 11, c4);
        box.SetPixel(9, 11, c6);
        box.SetPixel(9, 12, c7);
        box.SetPixel(8, 11, c8);
        box.SetPixel(8, 12, c9);
        box.SetPixel(7, 11, c10);
        box.SetPixel(7, 12, c11);
        box.SetPixel(6, 11, c12);
        box.SetPixel(6, 12, c13);
        box.SetPixel(5, 11, c10);
        box.SetPixel(5, 12, c11);
        box.SetPixel(4, 11, c8);
        box.SetPixel(4, 12, c9);
        box.SetPixel(3, 12, c7);
        box.SetPixel(3, 11, c6);
        box.SetPixel(3, 10, c5);
        box.SetPixel(2, 11, c4);
        box.SetPixel(2, 10, c3);
        box.SetPixel(2, 9, c5);
        box.SetPixel(1, 11, c1);
        box.SetPixel(1, 10, c4);
        box.SetPixel(1, 9, c6);
        box.SetPixel(0, 9, c7);
        box.SetPixel(1, 8, c8);
        box.SetPixel(0, 8, c9);
        box.SetPixel(1, 7, c1);
        box.SetPixel(0, 7, c11);
        box.SetPixel(1, 6, c12);
        box.SetPixel(0, 6, c13);
        box.SetPixel(1, 5, c10);
        box.SetPixel(0, 5, c11);
        box.SetPixel(1, 4, c8);
        box.SetPixel(0, 4, c9);
        box.SetPixel(1, 3, c6);
        box.SetPixel(0, 3, c7);
        #endregion
        if (this.Checked)
        {
            #region Draw check
            c1 = Pressing ? new Color(0, 84, 153) : Hovering ? new Color(0, 120, 215) : new Color(51, 51, 51);
            c2 = Pressing ? new Color(5, 87, 155) : Hovering ? new Color(6, 123, 216) : new Color(56, 56, 56);
            c3 = Pressing ? new Color(34, 108, 169) : Hovering ? new Color(43, 143, 222) : new Color(85, 85, 85);
            c4 = Pressing ? new Color(137, 181, 216) : Hovering ? new Color(171, 211, 242) : new Color(186, 186, 186);
            box.FillRect(4, 4, 5, 5, c1);
            box.SetPixel(6, 3, c2);
            box.SetPixel(3, 6, c2);
            box.SetPixel(9, 6, c2);
            box.SetPixel(6, 9, c2);
            box.SetPixel(5, 3, c3);
            box.SetPixel(7, 3, c3);
            box.SetPixel(3, 5, c3);
            box.SetPixel(3, 7, c3);
            box.SetPixel(5, 9, c3);
            box.SetPixel(7, 9, c3);
            box.SetPixel(9, 5, c3);
            box.SetPixel(9, 7, c3);
            box.SetPixel(4, 3, c4);
            box.SetPixel(8, 3, c4);
            box.SetPixel(3, 4, c4);
            box.SetPixel(3, 8, c4);
            box.SetPixel(4, 9, c4);
            box.SetPixel(8, 9, c4);
            box.SetPixel(9, 4, c4);
            box.SetPixel(9, 8, c5);
            #endregion
        }
        box.Lock();

        Size s = this.Font.TextSize(this.Text);
        Sprites["text"].Bitmap?.Dispose();
        Sprites["text"].Bitmap = new Bitmap(s);
        Sprites["text"].Bitmap.Font = this.Font;
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.DrawText(this.Text, this.TextColor);
        Sprites["text"].Bitmap.Lock();

        this.SetSize(s.Width + 18, 13);
    }
}
