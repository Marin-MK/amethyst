﻿using odl;

namespace amethyst;

public class VStackPanel : Widget, ILayout
{
    public bool NeedUpdate { get; set; } = true;
    public bool HDockWidgets = true;

    public VStackPanel(IContainer Parent) : base(Parent)
    {
        this.Size = new Size(this.Parent.Size.Width, 1);
    }

    public override void Update()
    {
        if (this.NeedUpdate)
        {
            this.UpdateLayout();
            this.NeedUpdate = false;
        }
        base.Update();
    }

    public override void UpdateLayout()
    {
        int maxw = 0;
        int y = 0;
        for (int i = 0; i < this.Widgets.Count; i++)
        {
            Widget w = this.Widgets[i];
            if (!w.Visible) continue;
            y += w.Margins.Up;
            int x = w.Margins.Left;
            int width = this.Size.Width - x - w.Margins.Right;
            if (HDockWidgets) w.SetWidth(width);
            w.SetPosition(x, y);
            y += w.Size.Height;
            y += w.Margins.Down;
            if (!HDockWidgets && w.ConsiderInAutoScrollCalculation && w.Position.X + w.Margins.Left + w.Size.Width > maxw) maxw = w.Position.X + w.Margins.Left + w.Size.Width;
        }
        if (HDockWidgets) SetHeight(y);
        else SetSize(maxw, y);
    }

    public override Widget SetSize(Size size)
    {
        base.SetSize(size);
        if (HDockWidgets)
        {
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                Widget w = this.Widgets[i];
                w.SetWidth(size.Width);
            }
        }
        return this;
    }

    public override void ChildBoundsChanged(BaseEventArgs e)
    {
        base.ChildBoundsChanged(e);
        UpdateHeight();
    }

    public void UpdateHeight()
    {
        int maxheight = 0;
        foreach (Widget w in this.Widgets)
        {
            int h = w.Position.Y + w.Size.Height;
            if (h > maxheight) maxheight = h;
        }
        this.SetHeight(maxheight);
    }

    public override void Add(Widget w, int Index = -1)
    {
        base.Add(w, Index);
        this.NeedUpdate = true;
    }
}
