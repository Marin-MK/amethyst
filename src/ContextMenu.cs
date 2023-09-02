using System.Collections.Generic;
using odl;


namespace amethyst;

public class ContextMenu : Widget
{
    public List<IMenuItem> Items { get; protected set; } = new List<IMenuItem>();
    public Color InnerColor { get; protected set; } = new Color(45, 69, 107);
    public Color OuterColor { get; protected set; } = Color.BLACK;
    public Font Font { get; protected set; }
    public bool CanMoveWithTab = false;
    public bool CanMoveWithUpDown = false;
    public int MoveIndex { get; protected set; } = 0;

    public MenuItem HoveringItem;
    public ContextMenu ChildMenu;
    public ContextMenu ParentMenu;
    public MenuItem ParentItem;

    bool AnyCheckables = false;

    public ContextMenu(IContainer Parent, ContextMenu ParentMenu = null, MenuItem ParentItem = null) : base(Parent)
    {
        Input.SetCursor(CursorType.Arrow);
        this.ParentMenu = ParentMenu;
        this.ParentItem = ParentItem;
        if (ParentMenu != null) SetZIndex(ParentMenu.ZIndex);
        else SetZIndex(Window.ActiveWidget is UIManager ? 9 : (Window.ActiveWidget as Widget).ZIndex + 9);
        Sprites["bg"] = new Sprite(Viewport);
        Sprites["bgsel"] = new Sprite(Viewport, new SolidBitmap(192, 18, new Color(28, 50, 73)));
        Sprites["bgsel"].Visible = false;
        Sprites["ext"] = new Sprite(Viewport);
        Sprites["selector"] = new Sprite(Viewport, new SolidBitmap(2, 23, new Color(55, 187, 255)));
        Sprites["selector"].X = 4;
        Sprites["selector"].Visible = false;
        Sprites["items"] = new Sprite(Viewport);
        OnHelpTextWidgetCreated += HelpTextWidgetCreated;
        OnFetchHelpText += FetchHelpText;

        if (ParentMenu != null) WindowLayer = ParentMenu.WindowLayer;
        else
        {
            WindowLayer = Window.ActiveWidget.WindowLayer + 1;
            Window.SetActiveWidget(this);
        }
        SetWidth(192);
        MinimumSize.Width = MaximumSize.Width = 192;

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.DOWN), _ => MoveDown(), true, e => e.Value = CanMoveWithUpDown),
            new Shortcut(this, new Key(Keycode.UP), _ => MoveUp(), true, e => e.Value = CanMoveWithUpDown),
            new Shortcut(this, new Key(Keycode.TAB), _ => MoveDown(), true, e => e.Value = CanMoveWithTab),
            new Shortcut(this, new Key(Keycode.TAB, Keycode.SHIFT), _ => MoveUp(), true, e => e.Value = CanMoveWithTab),
            new Shortcut(this, new Key(Keycode.DOWN, Keycode.CTRL), _ => MoveDown(), true, e => e.Value = CanMoveWithUpDown),
            new Shortcut(this, new Key(Keycode.UP, Keycode.CTRL), _ => MoveUp(), true, e => e.Value = CanMoveWithUpDown),
            new Shortcut(this, new Key(Keycode.TAB, Keycode.CTRL), _ => MoveDown(), true, e => e.Value = CanMoveWithTab),
            new Shortcut(this, new Key(Keycode.TAB, Keycode.CTRL, Keycode.SHIFT), _ => MoveUp(), true, e => e.Value = CanMoveWithTab),
        });
    }

    public void MoveUp()
    {
        int newIndex = MoveIndex - 1;
        // Keep going until we find a MenuItem
        while (newIndex >= 0)
        {
            if (Items[newIndex] is MenuItem) break;
            newIndex--;
        }
        if (newIndex < 0)
        {
            MoveIndex = Items.Count;
            MoveUp();
            return;
        }
        SetMoveIndex(newIndex);
    }

    public void MoveDown()
    {
        if (MoveIndex == 1)
        {

        }
        int newIndex = MoveIndex + 1;
        // Keep going until we find a MenuItem
        while (newIndex < Items.Count)
        {
            if (Items[newIndex] is MenuItem) break;
            newIndex++;
        }
        if (newIndex >= Items.Count)
        {
            MoveIndex = -1;
            MoveDown();
            return;
        }
        SetMoveIndex(newIndex);
    }

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            Redraw();
        }
    }

    public void SetMoveIndex(int moveIndex, bool force = false)
    {
        if (MoveIndex != moveIndex || force)
        {
            MoveIndex = moveIndex;
            Sprites["bgsel"].Visible = false;
            if (MoveIndex != -1)
            {
                Sprites["bgsel"].Y = GetCoordinateByIndex(MoveIndex);
                Sprites["bgsel"].Visible = true;
                HoveringItem = (MenuItem)Items[MoveIndex];
            }
        }
    }

    private int GetCoordinateByIndex(int index)
    {
        int y = 8;
        for (int i = 0; i <= index; i++)
        {
            if (Items[i] is MenuItem)
            {
                if (i == index) return y;
                y += 23;
            }
            else y += 5;
        }
        return 0;
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RedrawBox();
        ((SolidBitmap)Sprites["bgsel"].Bitmap).SetSize(Size.Width, 18);
    }

    private void RedrawBox()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, OuterColor);
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, InnerColor);
        Sprites["bg"].Bitmap.Lock();
    }

    public void SetInnerColor(byte R, byte G, byte B, byte A = 255)
    {
        SetInnerColor(new Color(R, G, B, A));
    }
    public void SetInnerColor(Color c)
    {
        if (InnerColor != c)
        {
            InnerColor = c;
            RedrawBox();
        }
    }

    public void SetOuterColor(byte R, byte G, byte B, byte A = 255)
    {
        SetOuterColor(new Color(R, G, B, A));
    }
    public void SetOuterColor(Color c)
    {
        if (OuterColor != c)
        {
            OuterColor = c;
            RedrawBox();
        }
    }

    public void SetItems(List<IMenuItem> Items)
    {
        this.Items = new List<IMenuItem>(Items);
        SetSize(192, CalcHeight() + 10);
        MinimumSize.Width = MaximumSize.Width = Size.Width;
        MinimumSize.Height = MaximumSize.Height = Size.Height;
        AnyCheckables = Items.Exists(i => i is MenuItem && ((MenuItem)i).IsCheckable);
    }

    protected override void Draw()
    {
        Sprites["items"].Bitmap?.Dispose();
        Sprites["items"].Bitmap = new Bitmap(192, CalcHeight() + 10);
        Sprites["items"].Bitmap.Font = Font;
        Sprites["items"].Bitmap.Unlock();

        Sprites["ext"].Bitmap?.Dispose();
        Sprites["ext"].Bitmap = new Bitmap(Sprites["items"].Bitmap.Width, Sprites["items"].Bitmap.Height);
        Sprites["ext"].Bitmap.Unlock();

        int x = AnyCheckables ? 22 : 10;
        int y = 5;
        for (int i = 0; i < Items.Count; i++)
        {
            IMenuItem item = Items[i];
            if (item is MenuItem)
            {
                MenuItem menuitem = item as MenuItem;

                bool Clickable = true;
                if (menuitem.IsClickable != null)
                {
                    BoolEventArgs e = new BoolEventArgs(true);
                    menuitem.IsClickable(e);
                    Clickable = e.Value;
                }

                // Draw check
                if (menuitem.IsCheckable)
                {
                    BoolEventArgs e = new BoolEventArgs(false);
                    menuitem.IsChecked(e);
                    if (e.Value)
                    {
                        int ox = 8;
                        int oy = y + 6;
                        Color w = Clickable ? Color.WHITE : new Color(120, 120, 120);
                        Sprites["ext"].Bitmap.DrawLine(ox, oy + 5, ox + 1, oy + 5, w);
                        Sprites["ext"].Bitmap.DrawLine(ox + 1, oy + 6, ox + 4, oy + 6, w);
                        Sprites["ext"].Bitmap.DrawLine(ox + 2, oy + 7, ox + 4, oy + 7, w);
                        Sprites["ext"].Bitmap.SetPixel(ox + 3, oy + 8, w);
                        Sprites["ext"].Bitmap.FillRect(ox + 4, oy + 4, 2, 2, w);
                        Sprites["ext"].Bitmap.FillRect(ox + 5, oy + 2, 2, 2, w);
                        Sprites["ext"].Bitmap.DrawLine(ox + 6, oy + 1, ox + 7, oy + 1, w);
                        Sprites["ext"].Bitmap.SetPixel(ox + 7, oy, w);
                    }
                }

                // Draw Text
                Color TextColor = Clickable ? Color.WHITE : new Color(155, 164, 178);
                Sprites["items"].Bitmap.DrawText(menuitem.Text, x, y + 4, TextColor);
                if (!string.IsNullOrEmpty(menuitem.Shortcut))
                    Sprites["items"].Bitmap.DrawText(menuitem.Shortcut, Size.Width - 9, y + 4, TextColor, DrawOptions.RightAlign);

                // Draw dropdown arrow
                if (menuitem.HasChildren)
                {
                    int ox = Size.Width - 10;
                    int oy = y + 10;
                    Color edge = new Color(131, 131, 131);
                    Color inside = new Color(214, 214, 214);
                    Sprites["ext"].Bitmap.DrawLine(ox, oy, ox + 3, oy + 3, edge);
                    Sprites["ext"].Bitmap.DrawLine(ox, oy + 6, ox + 2, oy + 4, edge);
                    Sprites["ext"].Bitmap.DrawLine(ox, oy + 1, ox, oy + 5, inside);
                    Sprites["ext"].Bitmap.DrawLine(ox + 1, oy + 2, ox + 1, oy + 4, inside);
                    Sprites["ext"].Bitmap.SetPixel(ox + 2, oy + 3, inside);
                }
                y += 23;
            }
            else if (item is MenuSeparator)
            {
                Sprites["items"].Bitmap.DrawLine(6, y + 2, Size.Width - 12, y + 2, 38, 56, 82);
                y += 5;
            }
        }
        Sprites["items"].Bitmap.Lock();
        Sprites["ext"].Bitmap.Lock();
        base.Draw();
    }

    public override void Dispose()
    {
        if (ChildMenu != null && !ChildMenu.Disposed) ChildMenu?.Dispose();
        ChildMenu = null;
        if (Window.ActiveWidget == this)
        {
            Window.Widgets.RemoveAt(Window.Widgets.Count - 1);
            Window.SetActiveWidget(Window.Widgets[Window.Widgets.Count - 1]);
        }
        base.Dispose();
    }

    private int CalcHeight(IMenuItem StopAtMenuItem = null)
    {
        int h = 0;
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] == StopAtMenuItem) break;
            if (Items[i] is MenuSeparator) h += 5;
            else h += 23;
        }
        return h;
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        Sprites["selector"].Visible = false;
        if (!Mouse.Inside || rx < 0 || rx > Size.Width) return;
        int y = 4;
        if (ry < y) return;
        MenuItem OldHovering = HoveringItem;
        HoveringItem = null;
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] is MenuItem)
            {
                if (y <= ry && y + 23 > ry)
                {
                    Sprites["selector"].Y = y + 2;
                    Sprites["selector"].Visible = true;
                    HoveringItem = (MenuItem)Items[i];
                    break;
                }
                y += 23;
            }
            else y += 5;
        }
        if (OldHovering != HoveringItem)
        {
            if (HelpTextWidget != null) HelpTextWidget.Dispose();
            HelpTextWidget = null;
            if (ChildMenu != null)
            {
                if (HoveringItem != null && ChildMenu.ParentItem != HoveringItem)
                {
                    // The hovered item is no longer the same as the child menu we have open
                    // So we will close the child menu shortly.
                    if (TimerExists("open_child_menu")) DestroyTimer("open_child_menu");
                    if (!TimerExists("close_child_menu")) SetTimer("close_child_menu", 400);
                }
            }
            else if (HoveringItem != null && HoveringItem.HasChildren)
            {
                // We're hovering over an item that has children, so open that menu momentarily.
                if (TimerExists("close_child_menu")) DestroyTimer("close_child_menu");
                if (TimerExists("open_child_menu")) DestroyTimer("open_child_menu");
                SetTimer("open_child_menu", 400);
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (TimerPassed("close_child_menu"))
        {
            DestroyTimer("close_child_menu");
            if (ChildMenu != null && HoveringItem != null && !ChildMenu.IsInsideSelfOrChild())
            {
                if (ChildMenu.ParentItem != HoveringItem)
                {
                    ChildMenu?.Dispose();
                    ChildMenu = null;
                    if (HoveringItem.HasChildren) OpenChildMenu();
                }
            }
            if (TimerExists("open_child_menu")) DestroyTimer("open_child_menu");
        }
        else if (TimerPassed("open_child_menu"))
        {
            if (HoveringItem != null && HoveringItem.HasChildren && (ChildMenu == null || ChildMenu.ParentItem != HoveringItem && !ChildMenu.IsInsideSelfOrChild()))
                OpenChildMenu();
            DestroyTimer("open_child_menu");
        }
    }

    private void OpenChildMenu()
    {
        ChildMenu?.Dispose();
        ChildMenu = null;
        if (!CanOpenHoveredItem()) return;
        ChildMenu = new ContextMenu(Parent, this, HoveringItem);
        ChildMenu.SetFont(Font);
        ChildMenu.SetInnerColor(InnerColor);
        ChildMenu.SetOuterColor(OuterColor);
        ChildMenu.SetItems(HoveringItem.Items);
        int x = Position.X + Size.Width;
        int y = Position.Y + CalcHeight(HoveringItem);
        int w = ChildMenu.Size.Width;
        int h = ChildMenu.Size.Height;
        ChildMenu.SetPosition(x, y);
    }

    public void OpenHoveredItem()
    {
        if (HoveringItem.HasChildren)
        {
            // Don't close child menu if that menu is the same as the currently open menu
            if (ChildMenu != null && ChildMenu.ParentItem == HoveringItem) return;
            OpenChildMenu();
        }
        else
        {
            // Disposes from the first ancestor downwards
            MoveIndex = Items.IndexOf(HoveringItem);
            DisposeFromTopDown();
            HoveringItem.OnClicked?.Invoke(new BaseEventArgs());
        }
    }

    public bool CanOpenHoveredItem()
    {
        BoolEventArgs e = new BoolEventArgs(true);
        HoveringItem.IsClickable?.Invoke(e);
        return e.Value;
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (Mouse.Inside)
        {
            if (Mouse.LeftMouseTriggered)
            {
                if (HoveringItem != null && CanOpenHoveredItem())
                {
                    OpenHoveredItem();
                    // Ensure no other events can be called from this mouse click
                    e.Handled = true;
                }
                else
                {
                    ChildMenu?.Dispose();
                    ChildMenu = null;
                }
            }
        }
        else if (ParentMenu == null) // Only do clicking outside menu check for the main parent menu
        {
            // Mouse was outside Main context menu; now check if mouse was also not inside any child menus
            bool WasInsideChild = false;
            if (ChildMenu != null && ChildMenu.IsInsideSelfOrChild()) WasInsideChild = true;
            if (!WasInsideChild) Dispose();
        }
    }

    public bool IsInsideSelfOrChild()
    {
        return Mouse.Inside || ChildMenu != null && ChildMenu.IsInsideSelfOrChild();
    }

    public void DisposeFromTopDown()
    {
        if (ParentMenu != null) ParentMenu.DisposeFromTopDown();
        else Dispose();
    }

    public void HelpTextWidgetCreated(BaseEventArgs e)
    {
        HelpTextWidget.SetZIndex(ZIndex);
    }

    public override void FetchHelpText(StringEventArgs e)
    {
        base.FetchHelpText(e);
        e.String = null;
        if (HoveringItem != null && !string.IsNullOrEmpty(HoveringItem.HelpText)) e.String = HoveringItem.HelpText;
    }
}
