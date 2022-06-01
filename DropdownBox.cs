using System;
using System.Collections.Generic;
using odl;

namespace amethyst;

public class DropdownBox : TextBox
{
    public bool ReadOnly { get { return TextArea.ReadOnly; } }
    public int SelectedIndex { get; protected set; } = 0;
    public List<ListItem> Items { get; protected set; } = new List<ListItem>();
    public bool Enabled { get; protected set; } = true;
    public int DropdownWidth = 31;

    public BaseEvent OnDropDownClicked;
    public BaseEvent OnSelectionChanged;

    public DropdownBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(this.Size.Width - DropdownWidth, this.Size.Height - 3);
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            Redraw();
            this.TextArea.SetEnabled(Enabled);
        }
    }

    public void SetReadOnly(bool ReadOnly)
    {
        TextArea.SetReadOnly(ReadOnly);
        Redraw();
    }

    public void SetSelectedIndex(int Index)
    {
        if (this.SelectedIndex != Index)
        {
            this.TextArea.SetText(Index >= Items.Count || Index == -1 ? "" : Items[Index].Name);
            this.SelectedIndex = Index;
            this.OnSelectionChanged?.Invoke(new BaseEventArgs());
        }
    }

    public void SetItems(List<ListItem> Items)
    {
        this.Items = Items;
        this.TextArea.SetText(SelectedIndex >= Items.Count || SelectedIndex == -1 ? "" : Items[SelectedIndex].Name);
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!TextArea.Mouse.Inside && TextArea.SelectedWidget)
        {
            Window.UI.SetSelectedWidget(null);
        }
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        if (rx >= Size.Width - DropdownWidth && rx < Size.Width - 1 &&
            ry >= 1 && ry < Size.Height - 1 && this.Enabled)
        {
            this.OnDropDownClicked?.Invoke(new BaseEventArgs());
            if (this.Items.Count > 0)
            {
                DropdownWidget dropdown = new DropdownWidget(Window.UI, this.Size.Width, this.Items);
                dropdown.SetPosition(this.Viewport.X, this.Viewport.Y + this.Viewport.Height);
                dropdown.SetSelected(SelectedIndex);
                dropdown.OnDisposed += delegate (BaseEventArgs e)
                {
                    if (dropdown.SelectedIndex != -1)
                    {
                        this.SetSelectedIndex(dropdown.SelectedIndex);
                    }
                };
            }
        };
    }
}

public class DropdownWidget : Widget
{
    public int SelectedIndex { get; protected set; }
    public int HoveringIndex { get; protected set; }

    public DropdownWidget(IContainer Parent, int Width, List<ListItem> Items) : base(Parent)
    {
        SetZIndex(Window.ActiveWidget is UIManager ? 9 : (Window.ActiveWidget as Widget).ZIndex + 9);
        SetSize(Width, Items.Count * 18 + 4);
        Sprites["box"] = new Sprite(this.Viewport);
        Sprites["box"].Bitmap = new Bitmap(this.Size);
        Sprites["box"].Bitmap.Unlock();
        Sprites["box"].Bitmap.DrawLine(1, 0, Size.Width - 2, 0, Color.BLACK);
        Sprites["box"].Bitmap.DrawLine(Size.Width - 1, 1, Size.Width - 1, Size.Height - 2, Color.BLACK);
        Sprites["box"].Bitmap.DrawLine(0, 1, 0, Size.Height - 2, Color.BLACK);
        Sprites["box"].Bitmap.DrawLine(1, Size.Height - 1, Size.Width - 2, Size.Height - 1, Color.BLACK);
        Sprites["box"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, new Color(45, 69, 107));
        Sprites["box"].Bitmap.Lock();
        Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(Width - 2, 18, new Color(86, 108, 134)));
        Sprites["selector"].X = 1;
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].Bitmap = new Bitmap(this.Size);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = Font.Get("assets/fonts/ProductSans-M", 9);
        for (int i = 0; i < Items.Count; i++)
        {
            Sprites["text"].Bitmap.DrawText(Items[i].Name, 6, i * 18 + 2, Color.WHITE);
        }
        Sprites["text"].Bitmap.Lock();
        Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 18, new Color(55, 187, 255)));
        Sprites["hover"].X = 1;
        Sprites["hover"].Visible = false;
        WindowLayer = Window.ActiveWidget.WindowLayer + 1;
        Window.SetActiveWidget(this);
    }

    public override void Dispose()
    {
        if (this.Window.ActiveWidget == this)
        {
            this.Window.Widgets.RemoveAt(Window.Widgets.Count - 1);
            this.Window.SetActiveWidget(Window.Widgets[Window.Widgets.Count - 1]);
        }
        base.Dispose();
    }

    public void SetSelected(int Index)
    {
        Sprites["selector"].Y = 2 + 18 * Index;
        Sprites["selector"].Visible = true;
        SelectedIndex = Index;
    }

    public void SetHovering(int Index)
    {
        if (Index == -1)
        {
            Sprites["hover"].Visible = false;
        }
        else
        {
            Sprites["hover"].Y = 2 + 18 * Index;
            Sprites["hover"].Visible = true;
        }
        HoveringIndex = Index;
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Mouse.Inside) SelectedIndex = -1;
        else SelectedIndex = HoveringIndex;
        Dispose();
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (Mouse.Inside)
        {
            int ry = e.Y - Viewport.Y;
            if (ry < 2 || ry >= this.Size.Height - 2) SetHovering(-1);
            else SetHovering((int)Math.Floor((ry - 2) / 18d));
        }
        else SetHovering(-1);
    }
}
