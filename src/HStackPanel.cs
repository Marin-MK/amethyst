using odl;

namespace amethyst;

public class HStackPanel : Widget, ILayout
{
    public bool NeedUpdate { get; set; } = true;

    public HStackPanel(IContainer Parent) : base(Parent)
    {
        Size = new Size(this.Parent.Size);
    }

    public override void Update()
    {
        if (NeedUpdate)
        {
            UpdateLayout();
            NeedUpdate = false;
        }
        base.Update();
    }

    public override void UpdateLayout()
    {
        int x = 0;
        for (int i = 0; i < Widgets.Count; i++)
        {
            Widget w = Widgets[i];
            if (!w.Visible) continue;
            x += w.Margins.Left;
            int y = w.Margins.Up;
            int height = Size.Height - y - w.Margins.Down;
            w.SetHeight(height);
            w.SetPosition(x, y);
            x += w.Size.Width;
            x += w.Margins.Right;
        }
    }

    public override Widget SetSize(Size size)
    {
        base.SetSize(size);
        for (int i = 0; i < Widgets.Count; i++)
        {
            Widget w = Widgets[i];
            w.SetHeight(size.Height);
        }
        UpdateLayout();
        return this;
    }
}
