using System;
using System.Collections.Generic;
using System.Linq;
using odl;

namespace amethyst;

public class Widget : IDisposable, IContainer
{
    /// <summary>
    /// The viewport of this widget. Influenced by position, size, parent position and size, scroll values, etc.
    /// </summary>
    public Viewport Viewport { get; set; }

    /// <summary>
    /// Full size of this widget. Can be smaller if viewport exceeds parent container, but never bigger.
    /// </summary>
    public Size Size { get; set; } = new Size(50, 50);

    /// <summary>
    /// Relative position to parent container.
    /// </summary>
    public Point Position { get; protected set; } = new Point(0, 0);

    /// <summary>
    /// Main window associated with this widget.
    /// </summary>
    public UIWindow Window { get; protected set; }

    /// <summary>
    /// Whether or not the widget should automatically resize based on its children's positions and sizes.
    /// </summary>
    public bool AutoResize = false;

    /// <summary>
    /// Minimum possible size for this widget.
    /// </summary>
    public Size MinimumSize { get; set; } = new Size(1, 1);

    /// <summary>
    /// Maximum possible size for this widget.
    /// </summary>
    public Size MaximumSize { get; set; } = new Size(-1, -1);

    /// <summary>
    /// Background color of this widget.
    /// </summary>
    public Color BackgroundColor { get; protected set; } = new Color(255, 255, 255, 0);

    /// <summary>
    /// The list of sprites that create the graphics of this widget. All sprites MUST be stored in here to be properly displayed.
    /// </summary>
    public Dictionary<string, Sprite> Sprites { get; protected set; } = new Dictionary<string, Sprite>();

    /// <summary>
    /// Children widgets of this widget.
    /// </summary>
    public List<Widget> Widgets { get; protected set; } = new List<Widget>();

    /// <summary>
    /// This object aids in fetching mouse input.
    /// </summary>
    public MouseManager Mouse { get; protected set; }

    /// <summary>
    /// The parent of this widget.
    /// </summary>
    public IContainer Parent { get; set; }

    /// <summary>
    /// Whether or not this widget has been disposed.
    /// </summary>
    public bool Disposed { get; protected set; } = false;

    /// <summary>
    /// Used for determining the viewport boundaries.
    /// </summary>
    public Point AdjustedPosition { get; protected set; } = new Point(0, 0);

    /// <summary>
    /// Whether or not this widget itself is visible. Actual visibility may vary based on parent visibility.
    /// </summary>
    public bool Visible { get; protected set; } = true;

    /// <summary>
    /// Whether or not this widget is the currently active and selected widget.
    /// </summary>
    public bool SelectedWidget = false;

    private int _ZIndex = 0;
    /// <summary>
    /// Relative Z Index of this widget and its viewport. Actual Z Index may vary based on parent Z Index.
    /// </summary>
    public int ZIndex
    {
        get
        {
            if (Parent is UIManager) return _ZIndex;
            return (Parent as Widget).ZIndex + _ZIndex;
        }
        protected set { _ZIndex = value; }
    }

    /// <summary>
    /// The list of right-click menu options to show when this widget is right-clicked.
    /// </summary>
    public List<IMenuItem> ContextMenuList { get; protected set; }

    /// <summary>
    /// Whether or not to show the context menu if this widget is right-clicked.
    /// </summary>
    public bool ShowContextMenu { get; protected set; } = false;

    /// <summary>
    /// The help text to show when hovering over this widget.
    /// </summary>
    public string HelpText { get; protected set; }

    public HelpText HelpTextWidget { get; protected set; }

    /// <summary>
    /// The list of keyboard shortcuts associated with this widget. Can be global shortcuts.
    /// </summary>
    public List<Shortcut> Shortcuts { get; protected set; } = new List<Shortcut>();

    /// <summary>
    /// Whether or not this widget should be considered when determining scrollbar size and position for autoscroll.
    /// </summary>
    public bool ConsiderInAutoScrollCalculation = true;

    /// <summary>
    /// Whether or not this widget should be affected by the autoscroll of the parent widget.
    /// </summary>
    public bool ConsiderInAutoScrollPositioning = true;

    /// <summary>
    /// The number of pixels the widget and all its parents have been clipped off on the left side.
    /// </summary>
    public int LeftCutOff { get; protected set; } = 0;

    /// <summary>
    /// The number of pixels the widget and all its parents have been clipped off on the top side.
    /// </summary>
    public int TopCutOff { get; protected set; } = 0;

    /// <summary>
    /// Whether the widget is docked horizontally to its parent.
    /// </summary>
    public bool HDocked { get; protected set; }

    /// <summary>
    /// Whether the widget is docked vertically to its parent.
    /// </summary>
    public bool VDocked { get; protected set; }

    /// <summary>
    /// Whether the docking will stick to the left side of the parent widget.
    /// </summary>
    public bool LeftDocked { get; protected set; } = true;

    /// <summary>
    /// Whether the docking will stick to the right side of the parent widget.
    /// </summary>
    public bool RightDocked { get; protected set; } = false;

    /// <summary>
    /// Whether the docking will stick to the top side of the parent widget.
    /// </summary>
    public bool TopDocked { get; protected set; } = true;

    /// <summary>
    /// Whether the docking will stick to the bottom side of the parent widget.
    /// </summary>
    public bool BottomDocked { get; protected set; } = false;

    private int _WindowLayer = 0;
    /// <summary>
    /// Which pseudo-window or layer this widget is on.
    /// </summary>
    public int WindowLayer
    {
        get
        {
            return Parent.WindowLayer > _WindowLayer ? Parent.WindowLayer : _WindowLayer;
        }
        set
        {
            _WindowLayer = value;
        }
    }

    /// <summary>
    /// Whether or not this widget should scroll horizontally if its children exceeds this widget's boundaries.
    /// </summary>
    public bool HAutoScroll = false;

    /// <summary>
    /// Whether or not this widget should scroll vertically if its children exceeds this widget's boundaries.
    /// </summary>
    public bool VAutoScroll = false;

    /// <summary>
    /// How far this widget has scrolled horizontally with autoscroll.
    /// </summary>
    public int ScrolledX { get; set; } = 0;

    /// <summary>
    /// How far this widget has scrolled horizontally with autoscroll.
    /// </summary>
    public int ScrolledY { get; set; } = 0;

    /// <summary>
    /// Relative position of this widget including scroll values.
    /// </summary>
    public Point ScrolledPosition
    {
        get
        {
            return new Point(
                this.Position.X - Parent.ScrolledX,
                this.Position.Y - Parent.ScrolledY
            );
        }
    }

    /// <summary>
    /// The total width occupied by this widget's children.
    /// </summary>
    public int MaxChildWidth = 0;

    /// <summary>
    /// The total height occupied by this widget's children.
    /// </summary>
    public int MaxChildHeight = 0;

    /// <summary>
    /// The ScrollBar for scrolling horizontally.
    /// </summary>
    public ScrollBar HScrollBar { get; protected set; }

    /// <summary>
    /// The ScrollBar for scrolling vertically.
    /// </summary>
    public ScrollBar VScrollBar { get; protected set; }

    /// <summary>
    /// The margin to use for grids and stackpanels.
    /// </summary>
    public Margins Margins { get; protected set; } = new Margins();

    /// <summary>
    /// Which grid row this widget starts in.
    /// </summary>
    public int GridRowStart = 0;

    /// <summary>
    /// Which grid row this widget ends in.
    /// </summary>
    public int GridRowEnd = 0;

    /// <summary>
    /// Which grid column this widget starts in.
    /// </summary>
    public int GridColumnStart = 0;

    /// <summary>
    /// Which grid column this widget ends in.
    /// </summary>
    public int GridColumnEnd = 0;

    /// <summary>
    /// A list of timers used for time-sensitive events
    /// </summary>
    public List<Timer> Timers { get; protected set; } = new List<Timer>();

    /// <summary>
    /// Whether or not to redraw this widget.
    /// </summary>
    protected bool Drawn = false;

    /// <summary>
    /// Whether or not the mouse is always active, even when a menu or window is above this widget.
    /// </summary>
    public bool MouseAlwaysActive = false;

    /// <summary>
    /// Extra Data field, used to pass around certain information depending on the context.
    /// </summary>
    public object ObjectData;

    public bool EvaluatedLastMouseEvent { get; set; }

    /// <summary>
    /// Called whenever the mouse moves across the window.
    /// </summary>
    public MouseEvent OnMouseMoving { get; set; }

    /// <summary>
    /// Called whenever the mouse moves in or out of the widget.
    /// </summary>
    public MouseEvent OnHoverChanged { get; set; }

    /// <summary>
    /// Called whenever a mouse button is pressed down.
    /// </summary>
    public MouseEvent OnMouseDown { get; set; }

    /// <summary>
    /// Called whenever a mouse button is released.
    /// </summary>
    public MouseEvent OnMouseUp { get; set; }

    /// <summary>
    /// Called while a mouse button is being held down.
    /// </summary>
    public MouseEvent OnMousePress { get; set; }

    /// <summary>
    /// Called whenever the left mouse button is pressed down.
    /// </summary>
    public MouseEvent OnLeftMouseDown { get; set; }

    /// <summary>
    /// Called whenever the left mouse button is pressed down inside the widget.
    /// </summary>
    public MouseEvent OnLeftMouseDownInside { get; set; }

    /// <summary>
    /// Called whenever the left mouse button is released.
    /// </summary>
    public MouseEvent OnLeftMouseUp { get; set; }

    /// <summary>
    /// Called while the left mouse button is being held down.
    /// </summary>
    public MouseEvent OnLeftMousePress { get; set; }

    /// <summary>
    /// Called whenever the right mouse button is pressed down.
    /// </summary>
    public MouseEvent OnRightMouseDown { get; set; }

    /// <summary>
    /// Called whenever the right mouse button is pressed down inside the widget.
    /// </summary>
    public MouseEvent OnRightMouseDownInside { get; set; }

    /// <summary>
    /// Called whenever the right mouse button is released.
    /// </summary>
    public MouseEvent OnRightMouseUp { get; set; }

    /// <summary>
    /// Called while the right mouse button is being held down.
    /// </summary>
    public MouseEvent OnRightMousePress { get; set; }

    /// <summary>
    /// Called whenever the middle mouse button is pressed down.
    /// </summary>
    public MouseEvent OnMiddleMouseDown { get; set; }

    /// <summary>
    /// Called whenever the middle mouse button is pressed down inside the widget.
    /// </summary>
    public MouseEvent OnMiddleMouseDownInside { get; set; }

    /// <summary>
    /// Called whenever the middle mouse button is released.
    /// </summary>
    public MouseEvent OnMiddleMouseUp { get; set; }

    /// <summary>
    /// Called while the middle mouse button is being held down.
    /// </summary>
    public MouseEvent OnMiddleMousePress { get; set; }

    /// <summary>
    /// Called whenever the mouse wheel is scrolled.
    /// </summary>
    public MouseEvent OnMouseWheel { get; set; }

    /// <summary>
    /// Called whenever the left mouse button is pressed down twice inside the widget in short succession.
    /// </summary>
    public MouseEvent OnDoubleLeftMouseDownInside { get; set; }

    /// <summary>
    /// Called when this widget becomes the active widget.
    /// </summary>
    public BaseEvent OnWidgetSelected;

    /// <summary>
    /// Called when this widget is no longer the active widget.
    /// </summary>
    public BaseEvent OnWidgetDeselected;

    /// <summary>
    /// Called when a button is being is pressed.
    /// </summary>
    public TextEvent OnTextInput;

    /// <summary>
    /// Called when this widget's relative position changes.
    /// </summary>
    public BaseEvent OnPositionChanged;

    /// <summary>
    /// Called when this widget's relative size changes.
    /// </summary>
    public BaseEvent OnSizeChanged;

    /// <summary>
    /// Called when this widget's parent relative size changes.
    /// </summary>
    public BaseEvent OnParentSizeChanged;

    /// <summary>
    /// Called when a child's relative size changes.
    /// </summary>
    public ObjectEvent OnChildBoundsChanged;

    /// <summary>
    /// Called before this widget is disposed.
    /// </summary>
    public BoolEvent OnDisposing;

    /// <summary>
    /// Called after this widget is disposed.
    /// </summary>
    public BaseEvent OnDisposed;

    /// <summary>
    /// Called when the autoscroll scrollbars are being scrolled.
    /// </summary>
    public BaseEvent OnScrolling;

    /// <summary>
    /// Called before the right-click menu would open. Is cancellable.
    /// </summary>
    public BoolEvent OnContextMenuOpening;

    /// <summary>
    /// Called upon creation of the help text widget.
    /// </summary>
    public BaseEvent OnHelpTextWidgetCreated;

    /// <summary>
    /// Called whenever the help text is to be retrieved.
    /// </summary>
    public StringEvent OnFetchHelpText;

    /// <summary>
    /// Called whenever SetVisibility() is called.
    /// </summary>
    public BaseEvent OnVisibilityChanged;

    /// <summary>
    /// Called whenever SetZIndex is called.
    /// </summary>
    public BaseEvent OnZIndexChanged;

    /// <summary>
    /// Indicates whether the context menu is set to be opened on next update.
    /// </summary>
    public bool OpenContextMenuOnNextUpdate = false;


    /// <summary>
    /// Creates a new Widget object.
    /// </summary>
    /// <param name="Parent">The Parent widget.</param>
    /// <param name="Name">The unique name to give this widget by which to store it.</param>
    /// <param name="Index">Optional index parameter. Used internally for stackpanels.</param>
    public Widget(IContainer Parent)
    {
        this.SetParent(Parent);
        // Create new viewport directly on the window's renderer.
        this.Viewport = new Viewport(this.Window, 0, 0, this.Size);
        // Z index by default copies parent viewport's z. If changed later, SetZIndex will modify the value.
        this.Viewport.Z = this.ZIndex;
        if (this.Parent is Widget) this.Viewport.Visible = (this.Parent as Widget).IsVisible() ? this.Visible : false;
        // The background sprite responsible for the BackgroundColor.
        this.Sprites["_bg"] = new Sprite(this.Viewport);
        this.Sprites["_bg"].Bitmap = new SolidBitmap(this.Size, this.BackgroundColor);
        // In the same viewport as all other sprites, but at a very large negative Z index.
        this.Sprites["_bg"].Z = -999999999;
        // Set some default events.

        this.OnMouseMoving = MouseMoving;
        this.OnHoverChanged = HoverChanged;
        this.OnMouseDown = MouseDown;
        this.OnMouseUp = MouseUp;
        this.OnMousePress = MousePress;
        this.OnLeftMouseDown = LeftMouseDown;
        this.OnLeftMouseDownInside = LeftMouseDownInside;
        this.OnLeftMouseUp = LeftMouseUp;
        this.OnLeftMousePress = LeftMousePress;
        this.OnRightMouseDown = RightMouseDown;
        this.OnRightMouseDownInside = RightMouseDownInside;
        this.OnRightMouseUp = RightMouseUp;
        this.OnRightMousePress = RightMousePress;
        this.OnMiddleMouseDown = MiddleMouseDown;
        this.OnMiddleMouseDownInside = MiddleMouseDownInside;
        this.OnMiddleMouseUp = MiddleMouseUp;
        this.OnMiddleMousePress = MiddleMousePress;
        this.OnMouseWheel = MouseWheel;
        this.OnDoubleLeftMouseDownInside = DoubleLeftMouseDownInside;
        this.OnWidgetDeselected = WidgetDeselected;
        this.OnTextInput = TextInput;
        this.OnPositionChanged = PositionChanged;
        this.OnParentSizeChanged = ParentSizeChanged;
        this.OnSizeChanged = SizeChanged;
        this.OnChildBoundsChanged = ChildBoundsChanged;
        // Creates the input manager object responsible for fetching mouse input.
        this.Mouse = new MouseManager(this);
        this.OnFetchHelpText = FetchHelpText;
        this.SetVisible(true);
    }

    /// <summary>
    /// Initializes the list of right-click menu options.
    /// </summary>
    /// <param name="Items">The list of menu items.</param>
    public virtual void SetContextMenuList(List<IMenuItem> Items)
    {
        AssertUndisposed();
        this.ContextMenuList = Items;
        this.ShowContextMenu = Items.Count > 0;
    }

    /// <summary>
    /// Sets the help message that appears when hovering over the widget.
    /// </summary>
    /// <param name="Text"></param>
    public void SetHelpText(string Text)
    {
        this.HelpText = Text;
    }

    /// <summary>
    /// Initializes the list of shortcuts.
    /// </summary>
    /// <param name="Shortcuts">The list of shortcuts.</param>
    public virtual void RegisterShortcuts(List<Shortcut> Shortcuts)
    {
        AssertUndisposed();
        // De-register old global shortcuts in the UIManager object.
        foreach (Shortcut s in this.Shortcuts)
        {
            if (s.GlobalShortcut) this.Window.UI.DeregisterShortcut(s);
            else
            {
                if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
                if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
            }
            Console.WriteLine($"Deregistered existing shortcut");
        }
        this.Shortcuts = Shortcuts;
        // Register global shortcuts in the UIManager object.
        foreach (Shortcut s in this.Shortcuts)
        {
            if (s.GlobalShortcut) this.Window.UI.RegisterShortcut(s);
        }
    }

    /// <summary>
    /// Sets the Z Index of this widget and viewport.
    /// </summary>
    /// <param name="ZIndex">The new Z Index.</param>
    public virtual void SetZIndex(int ZIndex)
    {
        AssertUndisposed();
        this.ZIndex = ZIndex;
        // this.ZIndex takes parent Z Index into account.
        this.Viewport.Z = this.ZIndex;
        this.Widgets.ForEach(w => w.SetZIndex(w.ZIndex));
        this.OnZIndexChanged?.Invoke(new BaseEventArgs());
    }

    /// <summary>
    /// Registers this widget under the parent widget.
    /// </summary>
    /// <param name="Parent">The Parent widget.</param>
    /// <param name="Index">Optional index for stackpanel parents.</param>
    public virtual void SetParent(IContainer Parent)
    {
        AssertUndisposed();
        bool New = true;
        // De-registers this widget from former parent if present.
        if (this.Parent != null)
        {
            this.Parent.Remove(this);
            New = false;
        }
        // MainEditorWindow isn't a widget, instead use its UI (UIManager) field.
        if (Parent is UIManager)
        {
            this.Window = (Parent as UIManager).Window;
            this.Parent = Parent;
        }
        else if (Parent is Widget)
        {
            this.Window = (Parent as Widget).Window;
            this.Parent = Parent;
        }
        this.Parent.Add(this);
        if (this.Viewport != null)
        {
            this.Viewport.Z = this.ZIndex;
            this.Viewport.Visible = (this.Parent as Widget).IsVisible() ? this.Visible : false;
        }
        if (!New) UpdateBounds(); // Update position for the new parent
    }

    /// <summary>
    /// Returns the pseudo-parent window. PopupWindow objects are Widgets, but seen as "Pseudo-windows".
    /// </summary>
    public virtual object GetParentWindow()
    {
        AssertUndisposed();
        if (Parent is IPopupWindow || Parent is UIWindow) return Parent;
        else if (Parent is UIManager) return Window;
        else return (Parent as Widget).GetParentWindow();
    }

    /// <summary>
    /// Set visibility for this widget and all children.
    /// </summary>
    /// <param name="Visible">Boolean visibility value.</param>
    public virtual void SetVisible(bool Visible)
    {
        AssertUndisposed();
        this.Visible = Visible;
        Widget parent = Parent as Widget;
        if (parent == null || (parent as Widget).IsVisible())
        {
            Viewport.Visible = Visible;
        }
        else
        {
            Viewport.Visible = false;
        }
        SetViewportVisible(Visible);
        this.OnVisibilityChanged?.Invoke(new BaseEventArgs());
    }

    /// <summary>
    /// Used for setting viewport visibility without changing actual visible property (used for children-parent visbility)
    /// </summary>
    /// <param name="Visible">Boolean visibilty value.</param>
    protected void SetViewportVisible(bool Visible, bool Initial = false)
    {
        Widget parent = Parent as Widget;
        if (parent == null || (parent as Widget).IsVisible())
        {
            if (this.Visible && Viewport.Visible != Visible)
                Viewport.Visible = Visible;
        }
        else this.Viewport.Visible = false;
        this.Widgets.ForEach(w => w.SetViewportVisible(Visible));
    }

    /// <summary>
    /// Returns whether this widget is visible.
    /// </summary>
    public bool IsVisible()
    {
        AssertUndisposed();
        return Viewport.Visible;
    }

    /// <summary>
    /// Updates the scrollbars based on children boundaries.
    /// </summary>
    public void UpdateAutoScroll()
    {
        if (!HAutoScroll && !VAutoScroll && !AutoResize) return;
        // Calculate total child width
        int OldMaxChildWidth = MaxChildWidth;
        MaxChildWidth = 0;
        this.Widgets.ForEach(wdgt =>
        {
            if (!wdgt.Visible || !wdgt.ConsiderInAutoScrollCalculation) return;
            int w = wdgt.Size.Width;
            w += wdgt.Position.X;
            if (w > MaxChildWidth) MaxChildWidth = w;
        });
        // Calculate total child height
        int OldMaxChildHeight = MaxChildHeight;
        MaxChildHeight = 0;
        this.Widgets.ForEach(w =>
        {
            if (!w.Visible || !w.ConsiderInAutoScrollCalculation) return;
            int h = w.Size.Height;
            h += w.Position.Y;
            if (h > MaxChildHeight) MaxChildHeight = h;
        });
        if (AutoResize)
        {
            SetSize(MaxChildWidth, MaxChildHeight);
        }
        else
        {
            // ScrollBarX
            if (HAutoScroll)
            {
                if (MaxChildWidth > this.Size.Width)
                {
                    if (HScrollBar == null)
                    {
                        throw new Exception("Autoscroll was enabled, but no scrollbar has been defined.");
                    }
                    if (OldMaxChildWidth - this.Viewport.Width > 0 && this.ScrolledX > OldMaxChildWidth - this.Viewport.Width)
                    {
                        this.ScrolledX = OldMaxChildWidth - this.Viewport.Width;
                    }
                    if (this.ScrolledX > MaxChildWidth - this.Viewport.Width)
                    {
                        this.ScrolledX = MaxChildWidth - this.Viewport.Width;
                    }
                    HScrollBar.SetValue((double)this.ScrolledX / (MaxChildWidth - this.Viewport.Width), false);
                    HScrollBar.SetSliderSize((double)this.Viewport.Width / MaxChildWidth);
                    HScrollBar.MouseInputRect = this.Viewport.Rect;
                    HScrollBar.SetVisible(true);
                }
                else if (HScrollBar != null)
                {
                    HScrollBar.SetVisible(false);
                    ScrolledX = 0;
                }
            }
            // ScrollBarY
            if (VAutoScroll)
            {
                if (MaxChildHeight > this.Size.Height)
                {
                    if (VScrollBar == null)
                    {
                        throw new Exception("Autoscroll was enabled, but no scrollbar has been defined.");
                    }
                    if (OldMaxChildHeight - this.Viewport.Height > 0 && this.ScrolledY > OldMaxChildHeight - this.Viewport.Height)
                    {
                        this.ScrolledY = OldMaxChildHeight - this.Viewport.Height;
                    }
                    if (this.ScrolledY > MaxChildHeight - this.Viewport.Height)
                    {
                        this.ScrolledY = MaxChildHeight - this.Viewport.Height;
                    }
                    VScrollBar.SetValue((double)this.ScrolledY / (MaxChildHeight - this.Viewport.Height), false);
                    VScrollBar.SetSliderSize((double)this.Viewport.Height / MaxChildHeight);
                    VScrollBar.MouseInputRect = this.Viewport.Rect;
                    VScrollBar.SetVisible(true);
                }
                else if (VScrollBar != null)
                {
                    VScrollBar.SetVisible(false);
                    ScrolledY = 0;
                }
            }
        }
        // Update positions
        this.UpdateBounds();
    }

    /// <summary>
    /// Links the given HScrollBar with this widget.
    /// </summary>
    public void SetHScrollBar(ScrollBar hsb)
    {
        this.HScrollBar = hsb;
        hsb.MouseInputRect = this.Viewport.Rect;
        hsb.LinkedWidget = this;
    }

    /// <summary>
    /// Links the given VScrollBar with this widget.
    /// </summary>
    public void SetVScrollBar(ScrollBar vsb)
    {
        this.VScrollBar = vsb;
        vsb.MouseInputRect = this.Viewport.Rect;
        vsb.LinkedWidget = this;
    }

    /// <summary>
    /// Updates this viewport's boundaries if it exceeds the parent viewport's boundaries and applies scrolling.
    /// </summary>
    public void UpdateBounds()
    {
        AssertUndisposed();
        foreach (Sprite sprite in this.Sprites.Values)
        {
            sprite.OX = sprite.OY = 0;
        }

        int xoffset = ConsiderInAutoScrollPositioning ? Parent.ScrolledX : 0;
        int yoffset = ConsiderInAutoScrollPositioning ? Parent.ScrolledY : 0;

        this.Viewport.X = this.Parent.Viewport.X + this.Position.X + this.Margins.Left - this.Parent.LeftCutOff - xoffset;
        this.Viewport.Y = this.Parent.Viewport.Y + this.Position.Y + this.Margins.Up - this.Parent.TopCutOff - yoffset;
        this.Viewport.Width = this.Size.Width;
        this.Viewport.Height = this.Size.Height;

        // Handles width exceeding parent viewport
        if (this.Viewport.X + this.Size.Width > this.Parent.Viewport.X + this.Parent.Viewport.Width)
        {
            int Diff = this.Viewport.X + this.Size.Width - (this.Parent.Viewport.X + this.Parent.Viewport.Width);
            this.Viewport.Width -= Diff;
        }
        // Handles X being negative
        if (this.Viewport.X < this.Parent.Viewport.X)
        {
            int Diff = this.Parent.Viewport.X - this.Viewport.X;
            this.Viewport.X += Diff;
            this.Viewport.Width -= Diff;
            foreach (Sprite sprite in this.Sprites.Values)
            {
                sprite.OX += Diff / sprite.ZoomX;
            }
            LeftCutOff = Diff;
        }
        else LeftCutOff = 0;
        // Handles height exceeding parent viewport
        if (this.Viewport.Y + this.Size.Height > this.Parent.Viewport.Y + this.Parent.Viewport.Height)
        {
            int Diff = this.Viewport.Y + this.Size.Height - (this.Parent.Viewport.Y + this.Parent.Viewport.Height);
            this.Viewport.Height -= Diff;
        }
        // Handles Y being negative
        if (this.Viewport.Y < this.Parent.Viewport.Y)
        {
            int Diff = this.Parent.Viewport.Y - this.Viewport.Y;
            this.Viewport.Y += Diff;
            this.Viewport.Height -= Diff;
            foreach (Sprite sprite in this.Sprites.Values)
            {
                sprite.OY += Diff / sprite.ZoomY;
            }
            TopCutOff = Diff;
        }
        else TopCutOff = 0;

        this.Widgets.ForEach(child => child.UpdateBounds());
    }

    /// <summary>
    /// Sets whether the widget should be docked to its parent.
    /// </summary>
    /// <param name="Docked">Vertical and horizontal docking.</param>
    public virtual void SetDocked(bool Docked)
    {
        SetHDocked(Docked);
        SetVDocked(Docked);
    }

    /// <summary>
    /// Sets whether the widget should be docked to its parent.
    /// </summary>
    /// <param name="HDocked">Horizontal docking.</param>
    /// <param name="VDocked">Vertical docking.</param>
    public virtual void SetDocked(bool HDocked, bool VDocked)
    {
        SetHDocked(HDocked);
        SetVDocked(VDocked);
    }

    /// <summary>
    /// Sets whether the widget should be docked to its parent.
    /// </summary>
    /// <param name="HDocked">Horizontal docking.</param>
    public virtual void SetHDocked(bool HDocked)
    {
        this.HDocked = HDocked;
        UpdatePositionAndSizeIfDocked();
    }

    /// <summary>
    /// Sets whether the widget should be docked to its parent.
    /// </summary>
    /// <param name="VDocked">Vertical docking.</param>
    public virtual void SetVDocked(bool VDocked)
    {
        this.VDocked = VDocked;
        UpdatePositionAndSizeIfDocked();
    }

    public virtual void SetLeftDocked(bool LeftDocked)
    {
        this.RightDocked = !LeftDocked;
        this.LeftDocked = LeftDocked;
        this.UpdatePositionAndSizeIfDocked();
    }

    public virtual void SetRightDocked(bool RightDocked)
    {
        this.RightDocked = RightDocked;
        this.LeftDocked = !RightDocked;
        this.UpdatePositionAndSizeIfDocked();
    }

    public virtual void SetTopDocked(bool TopDocked)
    {
        this.BottomDocked = !TopDocked;
        this.TopDocked = TopDocked;
        this.UpdatePositionAndSizeIfDocked();
    }

    public virtual void SetBottomDocked(bool BottomDocked)
    {
        this.BottomDocked = BottomDocked;
        this.TopDocked = !BottomDocked;
        this.UpdatePositionAndSizeIfDocked();
    }

    /// <summary>
    /// Sets the widget margins.
    /// </summary>
    /// <param name="All">Margins in all directions.</param>
    public virtual void SetMargins(int All)
    {
        this.SetMargins(new Margins(All));
    }

    /// <summary>
    /// Sets the widget margins.
    /// </summary>
    /// <param name="Horizontal">Horizontal margins.</param>
    /// <param name="Vertical">Vertical margins.</param>
    public virtual void SetMargins(int Horizontal, int Vertical)
    {
        this.SetMargins(new Margins(Horizontal, Vertical));
    }

    /// <summary>
    /// Sets the widget margins.
    /// </summary>
    /// <param name="Left">Left margins.</param>
    /// <param name="Up">Top margins.</param>
    /// <param name="Right">Right margins.</param>
    /// <param name="Down">Bottom margins.</param>
    public virtual void SetMargins(int Left, int Up, int Right, int Down)
    {
        this.SetMargins(new Margins(Left, Up, Right, Down));
    }

    /// <summary>
    /// Sets the widget margins.
    /// </summary>
    /// <param name="Margins">The widget's margins.</param>
    public virtual void SetMargins(Margins Margins)
    {
        this.Margins = Margins;
        UpdatePositionAndSizeIfDocked();
        UpdateLayout();
    }

    /// <summary>
    /// Updates the widget's size based on its docked state.
    /// </summary>
    public virtual void UpdatePositionAndSizeIfDocked()
    {
        if (this.HDocked || this.VDocked)
        {
            int neww = this.Size.Width;
            int newh = this.Size.Height;
            if (this.HDocked) neww = Parent.Size.Width - this.Position.X - this.Margins.Left - this.Margins.Right;
            if (this.VDocked) newh = Parent.Size.Height - this.Position.Y - this.Margins.Up - this.Margins.Down;
            this.SetSize(neww, newh);
        }
        if (this.RightDocked || this.BottomDocked)
        {
            if (this is PictureBox) this.Update();
            int newx = this.Position.X;
            int newy = this.Position.Y;
            if (this.RightDocked) newx = Parent.Size.Width - Size.Width - this.Margins.Right;
            if (this.BottomDocked) newy = Parent.Size.Height - Size.Height - this.Margins.Down;
            this.SetPosition(newx, newy);
        }
    }

    /// <summary>
    /// Changes the relative position of this widget.
    /// </summary>
    public void SetPosition(int X, int Y)
    {
        this.SetPosition(new Point(X, Y));
    }
    /// <summary>
    /// Changes the relative position of this widget.
    /// </summary>
    public virtual void SetPosition(Point p)
    {
        AssertUndisposed();
        if (this.Position.X != p.X || this.Position.Y != p.Y)
        {
            this.Position = p;
            this.UpdatePositionAndSizeIfDocked();
            this.UpdateBounds();
            this.OnPositionChanged(new BaseEventArgs());
        }
    }

    /// <summary>
    /// Sets the width of this widget.
    /// </summary>
    public Widget SetWidth(int Width)
    {
        return this.SetSize(Width, this.Size.Height);
    }
    /// <summary>
    /// Sets the height of this widget.
    /// </summary>
    public Widget SetHeight(int Height)
    {
        return this.SetSize(this.Size.Width, Height);
    }
    /// <summary>
    /// Sets the size of this widget.
    /// </summary>
    public Widget SetSize(int Width, int Height)
    {
        return this.SetSize(new Size(Width, Height));
    }
    /// <summary>
    /// Sets the size of this widget.
    /// </summary>
    public virtual Widget SetSize(Size size)
    {
        AssertUndisposed();
        Size oldsize = this.Size;
        // Ensures the set size matches the parent size if the widget is docked
        size.Width = HDocked ? Parent.Size.Width - this.Position.X - this.Margins.Left - this.Margins.Right : size.Width;
        size.Height = VDocked ? Parent.Size.Height - this.Position.Y - this.Margins.Up - this.Margins.Down : size.Height;
        // Ensures the new size doesn't exceed the set minimum and maximum values.
        if (size.Width < MinimumSize.Width) size.Width = MinimumSize.Width;
        else if (size.Width > MaximumSize.Width && MaximumSize.Width != -1) size.Width = MaximumSize.Width;
        if (size.Height < MinimumSize.Height) size.Height = MinimumSize.Height;
        else if (size.Height > MaximumSize.Height && MaximumSize.Height != -1) size.Height = MaximumSize.Height;
        if (oldsize.Width != size.Width || oldsize.Height != size.Height)
        {
            this.Size = size;
            // Update the background sprite's size
            (this.Sprites["_bg"].Bitmap as SolidBitmap).SetSize(this.Size);
            this.Viewport.Width = this.Size.Width;
            this.Viewport.Height = this.Size.Height;
            // Updates the viewport boundaries
            this.UpdateBounds();
            // Executes all events associated with resizing a widget.
            this.Widgets.ForEach(w =>
            {
                w.OnParentSizeChanged(new BaseEventArgs());
                w.OnSizeChanged(new BaseEventArgs());
            });
            this.OnSizeChanged(new BaseEventArgs());
            this.Widgets.ForEach(child => child.UpdatePositionAndSizeIfDocked());
            Redraw();
            if (this.Parent is Widget && !(this is ScrollBar))
            {
                Widget prnt = this.Parent as Widget;
                prnt.OnChildBoundsChanged(new ObjectEventArgs(this.Size));
            }
        }
        return this;
    }

    /// <summary>
    /// Sets the background color of this widget.
    /// </summary>
    public void SetBackgroundColor(byte r, byte g, byte b, byte a = 255)
    {
        this.SetBackgroundColor(new Color(r, g, b, a));
    }
    /// <summary>
    /// Sets the background color of this widget.
    /// </summary>
    public void SetBackgroundColor(Color c)
    {
        AssertUndisposed();
        this.BackgroundColor = c;
        (this.Sprites["_bg"].Bitmap as SolidBitmap).SetColor(c);
    }

    /// <summary>
    /// Sets a timer.
    /// </summary>
    /// <param name="identifier">Unique string identifier.</param>
    /// <param name="milliseconds">Number of milliseconds to run the timer for.</param>
    public void SetTimer(string identifier, long milliseconds)
    {
        Timers.Add(new Timer(identifier, DateTime.Now.Ticks, 10000 * milliseconds));
    }

    /// <summary>
    /// Returns whether or not the specified timer's time has elapsed.
    /// </summary>
    public bool TimerPassed(string identifier)
    {
        Timer t = Timers.Find(timer => timer.Identifier == identifier);
        if (t == null) return false;
        return DateTime.Now.Ticks >= t.StartTime + t.Timespan;
    }

    /// <summary>
    /// Returns whether or not the specified timer exists.
    /// </summary>
    public bool TimerExists(string identifier)
    {
        return Timers.Exists(t => t.Identifier == identifier);
    }

    /// <summary>
    /// Destroys the specified timer object.
    /// </summary>
    public void DestroyTimer(string identifier)
    {
        Timer t = Timers.Find(timer => timer.Identifier == identifier);
        if (t == null) throw new Exception("No timer by the identifier of '" + identifier + "' was found.");
        Timers.Remove(t);
    }

    /// <summary>
    /// Resets the specified timer with the former timespan.
    /// </summary>
    public void ResetTimer(string identifier)
    {
        Timer t = Timers.Find(timer => timer.Identifier == identifier);
        if (t == null) throw new Exception("No timer by the identifier of '" + identifier + "' was found.");
        t.StartTime = DateTime.Now.Ticks;
    }

    /// <summary>
    /// Adds a Widget object to this widget.
    /// </summary>
    public virtual void Add(Widget w)
    {
        this.Widgets.Add(w);
    }

    /// <summary>
    /// Removes a child widget and returns the deregistered widget.
    /// </summary>
    public virtual Widget Remove(Widget w)
    {
        for (int i = 0; i < this.Widgets.Count; i++)
        {
            if (this.Widgets[i] == w)
            {
                this.Widgets.RemoveAt(i);
                return w;
            }
        }
        return null;
    }

    /// <summary>
    /// Sets the grid row this widget is in. Used for grids.
    /// </summary>
    /// <param name="Row"></param>
    /// <returns></returns>
    public void SetGridRow(int Row)
    {
        this.SetGridRow(Row, Row);
    }
    /// <summary>
    /// Sets the grid row this widget starts and ends in. Used for grids.
    /// </summary>
    /// <param name="RowStart"></param>
    /// <param name="RowEnd"></param>
    /// <returns></returns>
    public void SetGridRow(int RowStart, int RowEnd)
    {
        this.GridRowStart = RowStart;
        this.GridRowEnd = RowEnd;
        this.UpdateLayout();
    }

    /// <summary>
    /// Sets the grid column this widget is in. Used for grids.
    /// </summary>
    /// <param name="Column"></param>
    /// <returns></returns>
    public void SetGridColumn(int Column)
    {
        this.SetGridColumn(Column, Column);
    }
    /// <summary>
    /// Sets the grid column this widget starts and ends in. Used for grids.
    /// </summary>
    public void SetGridColumn(int ColumnStart, int ColumnEnd)
    {
        this.GridColumnStart = ColumnStart;
        this.GridColumnEnd = ColumnEnd;
        this.UpdateLayout();
    }

    /// <summary>
    /// Sets the grid row and column this widget is in. Used for grids.
    /// </summary>
    /// <param name="Row"></param>
    /// <param name="Column"></param>
    /// <returns></returns>
    public void SetGrid(int Row, int Column)
    {
        this.SetGrid(Row, Row, Column, Column);
    }
    /// <summary>
    /// Sets the grid row and column this widget starts and ends in. Used for grids.
    /// </summary>
    public void SetGrid(int RowStart, int RowEnd, int ColumnStart, int ColumnEnd)
    {
        this.GridRowStart = RowStart;
        this.GridRowEnd = RowEnd;
        this.GridColumnStart = ColumnStart;
        this.GridColumnEnd = ColumnEnd;
        this.UpdateLayout();
    }

    /// <summary>
    /// Updates the grid or stackpanel's layout.
    /// </summary>
    public virtual void UpdateLayout()
    {
        if (this.Parent is ILayout)
        {
            (this.Parent as ILayout).NeedUpdate = true;
        }
    }

    /// <summary>
    /// Disposes this widget, its viewport and all its children.
    /// </summary>
    public virtual void Dispose()
    {
        AssertUndisposed();
        if (this.OnDisposing != null)
        {
            BoolEventArgs arg = new BoolEventArgs(true);
            this.OnDisposing.Invoke(arg);
            if (!arg.Value) return;
        }
        // Mark this widget as disposed.
        this.Disposed = true;
        foreach (Shortcut s in this.Shortcuts)
        {
            if (s.GlobalShortcut) s.PendingRemoval = true;
        }
        // Dispose the viewport and all its sprites.
        // Viewport may already be null if a child of a layoutcontainer has been disposed
        // because they share the same viewport.
        if (this.Viewport != null && !this.Viewport.Disposed)
            this.Viewport.Dispose();
        if (HelpTextWidget != null) HelpTextWidget.Dispose();
        // Dispose all child widgets
        for (int i = Widgets.Count - 1; i >= 0; i--) Widgets[i].Dispose();
        if (!this.Parent.Widgets.Remove(this))
        {
            if (this.Parent.Widgets.Contains(this))
            {
                throw new Exception("Failed to detach widget from parent.");
            }
        }
        // Set viewport and sprites to null to ensure no methods can use them anymore.
        this.Viewport = null;
        this.Sprites = null;
        this.OnDisposed?.Invoke(new BaseEventArgs());
    }

    /// <summary>
    /// Ensures this widget is not disposed by raising an exception if it is.
    /// </summary>
    protected void AssertUndisposed()
    {
        if (this.Disposed)
        {
            throw new ObjectDisposedException(this.ToString());
        }
    }

    /// <summary>
    /// Marks the sprites as needing an redraw next iteration.
    /// This allows you to call Redraw multiple times at once, as it will only redraw once in the next iteration.
    /// </summary>
    public virtual void Redraw()
    {
        AssertUndisposed();
        this.Drawn = false;
    }

    /// <summary>
    /// The method that redraws this widget and its sprites.
    /// </summary>
    protected virtual void Draw()
    {
        AssertUndisposed();
        this.Drawn = true;
    }

    /// <summary>
    /// Updates this widget and its children. Do not put excessive logic in here.
    /// </summary>
    public virtual void Update()
    {
        AssertUndisposed();

        if (OpenContextMenuOnNextUpdate)
        {
            OpenContextMenuOnNextUpdate = false;
            ContextMenu cm = new ContextMenu(Window.UI);
            cm.SetItems(ContextMenuList);
            Size s = cm.Size;
            int x = Graphics.LastMouseEvent.X;
            int y = Graphics.LastMouseEvent.Y;
            if (x + s.Width >= Window.Width) x -= s.Width;
            if (y + s.Height >= Window.Height) y -= s.Height;
            x = Math.Max(0, x);
            y = Math.Max(0, y);
            cm.SetPosition(x, y);
        }

        if (TimerPassed("helptext") && HelpTextWidget == null)
        {
            string text = "";
            if (OnFetchHelpText != null)
            {
                StringEventArgs e = new StringEventArgs();
                OnFetchHelpText(e);
                text = e.String;
            }
            if (!string.IsNullOrEmpty(text))
            {
                HelpTextWidget = new HelpText(Window.UI);
                HelpTextWidget.SetText(text);
                HelpTextWidget.SetPosition(Graphics.LastMouseEvent.X + 10, Graphics.LastMouseEvent.Y + 14);

                if (HelpTextWidget.Position.X + HelpTextWidget.Size.Width >= Window.Width)
                    HelpTextWidget.SetPosition(Graphics.LastMouseEvent.X - HelpTextWidget.Size.Width - 10, HelpTextWidget.Position.Y);

                if (HelpTextWidget.Position.Y + HelpTextWidget.Size.Height + 14 >= Window.Height)
                    HelpTextWidget.SetPosition(HelpTextWidget.Position.X, Graphics.LastMouseEvent.Y - HelpTextWidget.Size.Height - 14);

                OnHelpTextWidgetCreated?.Invoke(new BaseEventArgs());
            }
        }

        // If this widget is not active/accessible
        if (this.WindowLayer < this.Window.ActiveWidget.WindowLayer || !this.IsVisible())
        {
            if (TimerExists("helptext")) ResetTimer("helptext");
            if (HelpTextWidget != null) HelpTextWidget.Dispose();
            HelpTextWidget = null;
        }

        // If this widget is active and selected
        if (this.SelectedWidget && this.WindowLayer >= this.Window.ActiveWidget.WindowLayer && this.IsVisible())
        {
            // Execute shortcuts if their buttons is being triggered.
            foreach (Shortcut s in this.Shortcuts)
            {
                if (s.GlobalShortcut) continue; // Handled by the UIManager

                Key k = s.Key;
                bool Valid = false;
                if (Input.Press((odl.SDL2.SDL.SDL_Keycode)k.MainKey))
                {
                    if (TimerPassed($"key_{s.Key.ID}"))
                    {
                        ResetTimer($"key_{s.Key.ID}");
                        Valid = true;
                    }
                    else if (TimerPassed($"key_{s.Key.ID}_initial"))
                    {
                        SetTimer($"key_{s.Key.ID}", 50);
                        DestroyTimer($"key_{s.Key.ID}_initial");
                        Valid = true;
                    }
                    else if (!TimerExists($"key_{s.Key.ID}") && !TimerExists($"key_{s.Key.ID}_initial"))
                    {
                        if (Input.Trigger((odl.SDL2.SDL.SDL_Keycode) k.MainKey))
                        {
                            SetTimer($"key_{s.Key.ID}_initial", 300);
                            Valid = true;
                        }
                    }
                }
                else
                {
                    if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
                    if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
                }
                if (!Valid) continue;

                // Modifiers
                foreach (Keycode mod in k.Modifiers)
                {
                    bool onefound = false;
                    List<odl.SDL2.SDL.SDL_Keycode> codes = new List<odl.SDL2.SDL.SDL_Keycode>();
                    if (mod == Keycode.CTRL) { codes.Add(odl.SDL2.SDL.SDL_Keycode.SDLK_LCTRL); codes.Add(odl.SDL2.SDL.SDL_Keycode.SDLK_RCTRL); }
                    else if (mod == Keycode.SHIFT) { codes.Add(odl.SDL2.SDL.SDL_Keycode.SDLK_LSHIFT); codes.Add(odl.SDL2.SDL.SDL_Keycode.SDLK_RSHIFT); }
                    else if (mod == Keycode.ALT) { codes.Add(odl.SDL2.SDL.SDL_Keycode.SDLK_LALT); codes.Add(odl.SDL2.SDL.SDL_Keycode.SDLK_RALT); }
                    else codes.Add((odl.SDL2.SDL.SDL_Keycode)mod);

                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (Input.Press(codes[i]))
                        {
                            onefound = true;
                            break;
                        }
                    }

                    if (!onefound)
                    {
                        Valid = false;
                        break;
                    }
                }

                if (!Valid) continue;

                if (s.Condition != null)
                {
                    BoolEventArgs e = new BoolEventArgs(true);
                    s.Condition(e);
                    if (!e.Value) Valid = false;
                }

                // Execute this shortcut's event.
                if (Valid)
                {
                    s.Event(new BaseEventArgs());
                    // Remove any other key triggers for this iteration
                    Window.UI.ResetShortcutTimers(this);
                }
            }
        }

        // If this widget needs a redraw, perform the redraw
        if (!this.Drawn) this.Draw();
        // Update child widgets
        for (int i = 0; i < this.Widgets.Count; i++)
        {
            if (this.Widgets[i].Disposed)
            {
                this.Widgets.RemoveAt(i);
                i--;
                continue;
            }
            this.Widgets[i].Update();
        }
    }

    public void ResetShortcutTimers(IContainer Exception)
    {
        if (this != Exception)
        {
            foreach (Shortcut s in this.Shortcuts)
            {
                if (s.GlobalShortcut) continue;
                if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
                if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
            }
        }
        for (int i = 0; i < this.Widgets.Count; i++)
        {
            this.Widgets[i].ResetShortcutTimers(Exception);
        }
    }

    public virtual void MouseDown(MouseEventArgs e) { }

    public virtual void MouseMoving(MouseEventArgs e)
    {
        if (TimerExists("helptext")) ResetTimer("helptext");
    }

    public virtual void HoverChanged(MouseEventArgs e)
    {
        if (Mouse.Inside)
        {
            SetTimer("helptext", 1000);
        }
        else if (TimerExists("helptext"))
        {
            if (HelpTextWidget != null) HelpTextWidget.Dispose();
            HelpTextWidget = null;
            DestroyTimer("helptext");
        }
    }

    public virtual void MouseUp(MouseEventArgs e)
    {
        if (Mouse.Inside && Mouse.RightMouseReleased && Mouse.RightStartedInside && ShowContextMenu && ContextMenuList != null && ContextMenuList.Count > 0)
        {
            bool cont = true;
            if (OnContextMenuOpening != null)
            {
                BoolEventArgs args = new BoolEventArgs();
                this.OnContextMenuOpening(args);
                if (args.Value) cont = false;
            }
            if (cont) OpenContextMenuOnNextUpdate = true;
        }
    }

    public virtual void MousePress(MouseEventArgs e) { }

    public virtual void LeftMouseDown(MouseEventArgs e) { }

    public virtual void LeftMouseDownInside(MouseEventArgs e)
    {
        if (TimerExists("double_left_inside"))
        {
            if (!TimerPassed("double_left_inside"))
            {
                this.OnDoubleLeftMouseDownInside?.Invoke(e);
                DestroyTimer("double_left_inside");
            }
            else ResetTimer("double_left_inside");
        }
        else SetTimer("double_left_inside", 300);
    }

    public virtual void LeftMouseUp(MouseEventArgs e) { }

    public virtual void LeftMousePress(MouseEventArgs e) { }

    public virtual void RightMouseDown(MouseEventArgs e) { }

    public virtual void RightMouseDownInside(MouseEventArgs e) { }

    public virtual void RightMouseUp(MouseEventArgs e) { }

    public virtual void RightMousePress(MouseEventArgs e) { }

    public virtual void MiddleMouseDown(MouseEventArgs e) { }

    public virtual void MiddleMouseDownInside(MouseEventArgs e) { }

    public virtual void MiddleMouseUp(MouseEventArgs e) { }

    public virtual void MiddleMousePress(MouseEventArgs e) { }

    public virtual void MouseWheel(MouseEventArgs e) { }

    public virtual void DoubleLeftMouseDownInside(MouseEventArgs e) { }

    public virtual void WidgetSelected(BaseEventArgs e)
    {
        this.Window.UI.SetSelectedWidget(this);
    }

    public virtual void WidgetDeselected(BaseEventArgs e) { }

    public virtual void TextInput(TextEventArgs e) { }

    public virtual void PositionChanged(BaseEventArgs e)
    {
        UpdateAutoScroll();
    }

    public virtual void ParentSizeChanged(BaseEventArgs e) { }

    public virtual void SizeChanged(BaseEventArgs e)
    {
        UpdateAutoScroll();
    }

    public virtual void ChildBoundsChanged(BaseEventArgs e)
    {
        UpdateAutoScroll();
    }

    public virtual void FetchHelpText(StringEventArgs e)
    {
        e.String = this.HelpText;
    }
}

public class Shortcut
{
    public Widget Widget;
    public Key Key;
    public BaseEvent Event;
    public BoolEvent? Condition;
    public bool GlobalShortcut = false;
    public bool PendingRemoval = false;

    /// <summary>
    /// Creates a new Shortcut object.
    /// </summary>
    /// <param name="Widget">The widget that needs to be visible and available for global shortcuts to work.</param>
    /// <param name="Key">The actual key combination required to activate this shortcut.</param>
    /// <param name="Event">The event to trigger when the key is pressed.</param>
    /// <param name="GlobalShortcut">Whether the given widget needs to be active, or if this is a global shortcut.</param>
    public Shortcut(Widget Widget, Key Key, BaseEvent Event, bool GlobalShortcut = false, BoolEvent? Condition = null)
    {
        this.Widget = Widget;
        this.Key = Key;
        this.Event = Event;
        this.GlobalShortcut = GlobalShortcut;
        this.Condition = Condition;
    }
}

public class Timer
{
    public string Identifier;
    public long StartTime;
    public long Timespan;

    /// <summary>
    /// Creates a new Timer object.
    /// </summary>
    /// <param name="identifier">The unique identifier string.</param>
    /// <param name="starttime">The time at which this Timer started.</param>
    /// <param name="timespan">The timespan this Timer is active for.</param>
    public Timer(string identifier, long starttime, long timespan)
    {
        this.Identifier = identifier;
        this.StartTime = starttime;
        this.Timespan = timespan;
    }
}
