﻿using odl;
using System;
using System.Collections.Generic;
using System.Text;

namespace amethyst
{
    public class TextWidget : Widget
    {
        public string Text { get; protected set; } = "";
        public Color TextColor { get; protected set; } = Color.BLACK;
        public Font Font { get; protected set; }

        protected bool DrawnText = false;

        public TextWidget(Font Font, IContainer Parent) : base(Parent)
        {
            this.SetFont(Font);
            this.SetText(this.GetType().Name);
        }

        public void SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                this.RedrawText();
            }
        }

        public void SetTextColor(Color TextColor)
        {
            if (this.TextColor != TextColor)
            {
                this.TextColor = TextColor;
                this.RedrawText();
            }
        }

        public void SetFont(Font Font)
        {
            if (this.Font != Font)
            {
                this.Font = Font;
                this.RedrawText();
            }
        }

        protected virtual void DrawText()
        {
            this.DrawnText = true;
        }

        protected override void Draw()
        {
            if (!this.DrawnText) DrawText();
            base.Draw();
        }

        public void RedrawText()
        {
            this.Redraw();
            this.DrawnText = false;
        }
    }
}
