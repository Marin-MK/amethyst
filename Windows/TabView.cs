using System;
using System.Collections.Generic;
using System.Text;
using odl;

namespace amethyst.Windows;

public class TabView : Widget
{
    public List<TabContainer> Tabs = new List<TabContainer>();
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.BLACK;

    protected List<int> HeaderSizes = new List<int>();

    public int HoveringIndex { get; protected set; } = -1;
    public int SelectedIndex { get; protected set; } = -1;

    public BaseEvent OnTabChanged;

    public TabView(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(this.Viewport);
        Sprites["titles"] = new Sprite(this.Viewport);
        Sprites["filler"] = new Sprite(this.Viewport);
        Sprites["filler"].Y = 21;
        this.OnWidgetSelected += WidgetSelected;
    }

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            this.Redraw();
        }
    }

    public void SetTextColor(Color TextColor)
    {
        if (this.TextColor != TextColor)
        {
            this.TextColor = TextColor;
            this.Redraw();
        }
    }

    public void SetSelectedIndex(int SelectedIndex)
    {
        if (this.SelectedIndex != SelectedIndex)
        {
            int OldIndex = this.SelectedIndex;
            this.SelectedIndex = SelectedIndex;
            if (OldIndex != -1)
            {
                Tabs[OldIndex].SetVisible(false);
            }
            if (this.SelectedIndex != -1)
            {
                Tabs[SelectedIndex].SetVisible(true);
            }
            this.OnTabChanged?.Invoke(new BaseEventArgs());
            this.Redraw();
        }
    }

    protected override void Draw()
    {
        HeaderSizes.Clear();
        Sprites["titles"].Bitmap?.Dispose();
        int width = SelectedIndex == 0 ? 0 : 2;
        for (int i = 0; i < Tabs.Count; i++)
        {
            Size s = this.Font.TextSize(Tabs[i].Title);
            width += s.Width + 10;
            HeaderSizes.Add(s.Width + 10);
        }
        Sprites["titles"].Bitmap = new Bitmap(width, 20);
        Sprites["titles"].Bitmap.Unlock();
        Sprites["titles"].Bitmap.Font = this.Font;
        int textx = SelectedIndex == 0 ? 0 : 2;
        for (int i = 0; i < Tabs.Count; i++)
        {
            bool Selected = i == this.SelectedIndex;
            int w = this.Font.TextSize(Tabs[i].Title).Width + 10;
            Sprites["titles"].Bitmap.DrawText(Tabs[i].Title, textx + w / 2, Selected ? 2 : 4, this.TextColor, DrawOptions.CenterAlign);
            textx += w;
        }
        Sprites["titles"].Bitmap.Lock();
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap(Math.Max(Size.Width, width), 21);
        Sprites["box"].Bitmap.Unlock();
        int boxx = SelectedIndex == 0 ? 0 : 2;
        if (SelectedIndex != 0) Sprites["box"].Bitmap.DrawLine(0, 20, 2, 20, SystemColors.LightBorderColor);
        for (int i = 0; i < Tabs.Count; i++)
        {
            int w = this.Font.TextSize(Tabs[i].Title).Width + 10;
            bool Selected = this.SelectedIndex == i;
            int y = Selected ? 0 : 2;
            int h = Selected ? 21 : 19;
            Sprites["box"].Bitmap.DrawRect(boxx, y, w, h, SystemColors.LightBorderColor);
            if (Selected)
            {
                Sprites["box"].Bitmap.FillRect(boxx + 1, y + 1, w - 2, h - 1, SystemColors.ControlBackground);
            }
            else if (HoveringIndex == i)
            {
                Sprites["box"].Bitmap.FillRect(boxx + 1, y + 1, w - 2, h - 2, SystemColors.BorderlessColorHovering);
            }
            else
            {
                Sprites["box"].Bitmap.FillRect(boxx + 1, y + 1, w - 2, h - 2, SystemColors.LightBorderFiller);
            }
            boxx += w - 1;
        }
        if (boxx < Size.Width) Sprites["box"].Bitmap.DrawLine(boxx, 20, Size.Width - 1, 20, SystemColors.LightBorderColor);
        Sprites["box"].Bitmap.Lock();
        Sprites["filler"].Bitmap?.Dispose();
        Sprites["filler"].Bitmap = new Bitmap(Size.Width, Size.Height - 21);
        Sprites["filler"].Bitmap.Unlock();
        Sprites["filler"].Bitmap.DrawLine(0, 0, 0, Size.Height - 22, SystemColors.LightBorderColor);
        Sprites["filler"].Bitmap.DrawLine(1, Size.Height - 22, Size.Width - 1, Size.Height - 22, SystemColors.LightBorderColor);
        Sprites["filler"].Bitmap.DrawLine(Size.Width - 1, 0, Size.Width - 1, Size.Height - 23, SystemColors.LightBorderColor);
        Sprites["filler"].Bitmap.FillRect(1, 0, Size.Width - 2, Size.Height - 22, SystemColors.ControlBackground);
        Sprites["filler"].Bitmap.Lock();
        base.Draw();
    }

    public TabContainer CreateTab(string Title)
    {
        TabContainer tc = new TabContainer(this);
        tc.SetPosition(1, 21);
        tc.SetTitle(Title);
        tc.SetSize(this.Size.Width - 2, this.Size.Height - 22);
        tc.SetVisible(Tabs.Count == SelectedIndex);
        this.Tabs.Add(tc);
        if (SelectedIndex == -1) SetSelectedIndex(0);
        return tc;
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        for (int i = 0; i < Tabs.Count; i++)
        {
            Tabs[i].SetSize(this.Size.Width - 2, this.Size.Height - 22);
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        if (!WidgetIM.Hovering || ry >= 21)
        {
            if (HoveringIndex != -1)
            {
                HoveringIndex = -1;
                this.Redraw();
            }
            return;
        }
        int w = rx;
        int OldIndex = HoveringIndex;
        HoveringIndex = -1;
        for (int i = 0; i < HeaderSizes.Count; i++)
        {
            w -= HeaderSizes[i];
            if (w <= 0)
            {
                HoveringIndex = i;
                break;
            }
        }
        if (OldIndex != HoveringIndex) this.Redraw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (HoveringIndex != -1)
        {
            this.SetSelectedIndex(HoveringIndex);
        }
    }
}

public class TabContainer : Widget
{
    public string Title { get; protected set; }

    public TabContainer(IContainer Parent) : base(Parent)
    {

    }

    public void SetTitle(string Title)
    {
        if (this.Title != Title)
        {
            this.Title = Title;
            if (Parent is TabView) ((TabView)Parent).Redraw();
        }
    }
}
