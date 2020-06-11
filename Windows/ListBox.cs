using System;
using System.Collections.Generic;
using System.Text;
using odl;

namespace amethyst.Windows
{
    public class ListBox : Widget
    {
        protected ListDrawer ListDrawer;

        public List<ListItem> Items { get { return ListDrawer.Items; } }

        public BaseEvent OnSelectionChanged { get { return ListDrawer.OnSelectionChanged; } set { ListDrawer.OnSelectionChanged = value; } }

        public int SelectedIndex { get { return ListDrawer.SelectedIndex; } }

        public ListBox(IContainer Parent) : base(Parent)
        {
            Sprites["box"] = new Sprite(this.Viewport);

            ListDrawer = new ListDrawer(this);
            ListDrawer.SetPosition(2, 2);
        }

        public void SetSelectedIndex(int SelectedIndex)
        {
            ListDrawer.SetSelectedIndex(SelectedIndex);
        }

        public void SetItems(List<ListItem> Items)
        {
            ListDrawer.SetItems(Items);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            Sprites["box"].Bitmap?.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.DrawRect(Size, SystemColors.ContainerBorderColor);
            Sprites["box"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, SystemColors.ControlBackground);
            Sprites["box"].Bitmap.Lock();

            ListDrawer.SetSize(Size.Width - 2, Size.Height - 4);
        }
    }

    public class ListDrawer : Widget
    {
        protected Container ItemContainer;
        protected Container ItemPanel;
        protected bool ClickedInList = false;

        public List<ListItem> Items = new List<ListItem>();
        public int LineSize = 14;

        public Color TextColor { get; protected set; } = Color.BLACK;
        public Color SelectedBackgroundColor { get; protected set; } = SystemColors.SelectionColor;
        public Color SelectedTextColor { get; protected set; } = Color.WHITE;
        public Font Font { get; protected set; } = Font.Get("Windows/segoeui", 12);
        public bool DrawSelectionBold = false;

        public int SelectedIndex { get; protected set; } = -1;

        public BaseEvent OnSelectionChanged;

        public ListDrawer(IContainer Parent) : base(Parent)
        {
            ItemContainer = new Container(this);
            VScrollBar vs = new VScrollBar(this);
            ItemContainer.VAutoScroll = true;
            ItemContainer.SetVScrollBar(vs);
            ItemPanel = new Container(ItemContainer);
            ItemPanel.Sprites["items"] = new Sprite(this.Viewport);
            this.OnWidgetSelected += WidgetSelected;
            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.UP), delegate (BaseEventArgs e) { GoUp(); }),
                new Shortcut(this, new Key(Keycode.DOWN), delegate (BaseEventArgs e) { GoDown(); }),
                new Shortcut(this, new Key(Keycode.PAGEUP), delegate (BaseEventArgs e) { GoUp(true); }),
                new Shortcut(this, new Key(Keycode.PAGEDOWN), delegate (BaseEventArgs e) { GoDown(true); })
            });
        }

        public void GoUp(bool Page = false)
        {
            if (Page)
            {
                SetSelectedIndex(SelectedIndex - (int) Math.Floor((double) Size.Height / LineSize) + 1);
            }
            else SetSelectedIndex(SelectedIndex - 1);
            if (SelectedIndex < 0) SetSelectedIndex(0);
            if (SelectedIndex * LineSize < ItemContainer.ScrolledY)
            {
                ItemContainer.ScrolledY = SelectedIndex * LineSize;
                ItemContainer.UpdateAutoScroll();
            }
        }

        public void GoDown(bool Page = false)
        {
            if (Page)
            {
                SetSelectedIndex(SelectedIndex + (int) Math.Floor((double) Size.Height / LineSize) - 1);
            }
            else SetSelectedIndex(SelectedIndex + 1);
            if (SelectedIndex >= Items.Count) SetSelectedIndex(Items.Count - 1);
            if (SelectedIndex * LineSize >= ItemContainer.Size.Height - ItemContainer.ScrolledY)
            {
                ItemContainer.ScrolledY = SelectedIndex * LineSize - ItemContainer.Size.Height + LineSize;
                ItemContainer.UpdateAutoScroll();
            }
        }

        public void SetSelectedIndex(int SelectedIndex)
        {
            if (this.SelectedIndex != SelectedIndex)
            {
                this.SelectedIndex = SelectedIndex;
                this.Redraw();
                OnSelectionChanged?.Invoke(new BaseEventArgs());
            }
        }

        public void SetItems(List<ListItem> Items)
        {
            this.Items = Items;
            if (this.SelectedIndex == -1 && Items.Count > 0) SetSelectedIndex(0);
            this.Redraw();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            ItemContainer.SetSize(this.Size);
            ItemContainer.VScrollBar.SetPosition(Size.Width - ItemContainer.VScrollBar.Size.Width - 1, 0);
            ItemContainer.VScrollBar.SetHeight(Size.Height);
        }

        protected override void Draw()
        {
            base.Draw();
            ItemPanel.Sprites["items"].Bitmap?.Dispose();
            int width = Size.Width;
            if (Items.Count * LineSize >= Size.Height) width -= ItemContainer.VScrollBar.Size.Width + 2;
            ItemPanel.SetSize(width, Items.Count * LineSize);
            // Chunk size doesn't matter as we're redrawing the entire bitmap anyway
            // Only used to be able to exceed the texture size limit to allow for infinite lists
            ItemPanel.Sprites["items"].Bitmap = new LargeBitmap(width, Items.Count * LineSize, Graphics.MaxTextureSize);
            ItemPanel.Sprites["items"].Bitmap.Unlock();
            ItemPanel.Sprites["items"].Bitmap.Font = Font;
            for (int i = 0; i < Items.Count; i++)
            {
                DrawOptions options = DrawOptions.LeftAlign;
                if (i == SelectedIndex)
                {
                    ItemPanel.Sprites["items"].Bitmap.FillRect(0, i * LineSize, width, LineSize, SelectedBackgroundColor);
                    if (DrawSelectionBold) options |= DrawOptions.Bold;
                }
                ItemPanel.Sprites["items"].Bitmap.DrawText(Items[i].ToString(), 3, i * LineSize - 2, i == SelectedIndex ? SelectedTextColor : TextColor, options);
            }
            ItemPanel.Sprites["items"].Bitmap.Lock();
            ItemContainer.UpdateAutoScroll();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (e.LeftButton != e.OldLeftButton && e.LeftButton)
            {
                ClickedInList = false;
                int rx = e.X - ItemPanel.Viewport.X;
                int ry = e.Y - ItemPanel.Viewport.Y + ItemPanel.Position.Y - ItemPanel.ScrolledPosition.Y;
                if (rx < 0 || ry < 0 || rx >= ItemPanel.Size.Width || ry >= ItemPanel.Size.Height) return;
                ClickedInList = true;
            }
            MouseMoving(e);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            if (e.LeftButton != e.OldLeftButton && !e.LeftButton)
            {
                ClickedInList = false;
            }
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            if (e.LeftButton && WidgetIM.Hovering && ClickedInList)
            {
                int rx = e.X - ItemPanel.Viewport.X;
                int ry = e.Y - ItemPanel.Viewport.Y + ItemPanel.Position.Y - ItemPanel.ScrolledPosition.Y;
                if (rx < 0 || ry < 0 || rx >= ItemPanel.Size.Width || ry >= ItemPanel.Size.Height) return;
                int idx = (int) Math.Floor((double) ry / LineSize);
                if (idx != SelectedIndex) SetSelectedIndex(idx);
            }
        }
    }
}
