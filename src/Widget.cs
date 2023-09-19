using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using amethyst.Animations;
using odl;


namespace amethyst;

public class Widget : IDisposable, IContainer
{
	private static bool _ShowWidgetOutlines;
	public static bool ShowWidgetOutlines
	{
		get
		{
			return _ShowWidgetOutlines;
		}
		set
		{
			_ShowWidgetOutlines = value;
			void updaterecursive(Widget w)
			{
				w.UpdateOutlines();
				w.Widgets.ForEach(wdgt => updaterecursive(wdgt));
			}
			UIWindow.Windows.ForEach(win =>
			{
				win.UI.Widgets.ForEach(w =>
				{
					updaterecursive(w);
				});
			});
		}
	}

	public static Font DefaultContextMenuFont;

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
	/// Relative padding that is applied to all of this widget's children.
	/// </summary>
	public Padding ChildPadding = new Padding(0);

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
	/// The opacity of the widget's viewport.
	/// </summary>
	public byte Opacity { get; protected set; } = 255;

	/// <summary>
	/// The horizontal zoom of the widget's viewport.
	/// </summary>
	public float ZoomX { get; protected set; } = 1;

	/// <summary>
	/// The vertical zoom of the widget's viewport.
	/// </summary>
	public float ZoomY { get; protected set; } = 1;

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
	/// The font to be used in the context menu.
	/// </summary>
	public Font ContextMenuFont { get; protected set; }

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
	/// The list of keyboard shortcuts that have seen at least one update round. This is useful if a shortcut took a long time to run, and by the next update cycle, the initial timer has already passed.
	/// </summary>
	private List<string> ValidShortcutInputs = new List<string>();

	/// <summary>
	/// Whether or not this widget should be considered when determining scrollbar size and position for autoscroll.
	/// </summary>
	public bool ConsiderInAutoScrollCalculation = true;

	/// <summary>
	/// Whether or not this widget should be affected by the x autoscroll of the parent widget.
	/// </summary>
	public bool ConsiderInAutoScrollPositioningX = true;

	/// <summary>
	/// Whether or not this widget should be affected by the y autoscroll of the parent widget.
	/// </summary>
	public bool ConsiderInAutoScrollPositioningY = true;

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

	/// <summary>
	/// Which pseudo-window or layer this widget is on.
	/// </summary>
	public int WindowLayer { get; set; }

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
	/// How far this widget has scrolled vertically with autoscroll.
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
				Position.X - Parent.ScrolledX,
				Position.Y - Parent.ScrolledY
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
	/// The margin to use as a position offset.
	/// </summary>
	public Margins Margins { get; protected set; } = new Margins();

	/// <summary>
	/// The padding with which to offset and shrink the widget.
	/// </summary>
	public Padding Padding { get; protected set; } = new Padding();

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
	/// A list of timer names and associated callbacks
	/// </summary>
	public List<(string TimerName, Action Callback)> Callbacks = new List<(string, Action)>();

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
	/// A list of all active animations.
	/// </summary>
	public List<IAnimation> Animations { get; set; } = new List<IAnimation>();

	/// <summary>
	/// Called whenever the mouse moves across the window.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnMouseMoving { get; set; }

	/// <summary>
	/// Called whenever the mouse moves in or out of the widget.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnHoverChanged { get; set; }

	/// <summary>
	/// Called whenever a mouse button is pressed down.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnMouseDown { get; set; }

	/// <summary>
	/// Called whenever a mouse button is released.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnMouseUp { get; set; }

	/// <summary>
	/// Called while a mouse button is being held down.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnMousePress { get; set; }

	/// <summary>
	/// Called whenever the left mouse button is pressed down.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnLeftMouseDown { get; set; }

	/// <summary>
	/// Called whenever the left mouse button is pressed down inside the widget.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnLeftMouseDownInside { get; set; }

	/// <summary>
	/// Called whenever the left mouse button is released.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnLeftMouseUp { get; set; }

	/// <summary>
	/// Called while the left mouse button is being held down.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnLeftMousePress { get; set; }

	/// <summary>
	/// Called whenever the right mouse button is pressed down.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnRightMouseDown { get; set; }

	/// <summary>
	/// Called whenever the right mouse button is pressed down inside the widget.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnRightMouseDownInside { get; set; }

	/// <summary>
	/// Called whenever the right mouse button is released.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnRightMouseUp { get; set; }

	/// <summary>
	/// Called while the right mouse button is being held down.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnRightMousePress { get; set; }

	/// <summary>
	/// Called whenever the middle mouse button is pressed down.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnMiddleMouseDown { get; set; }

	/// <summary>
	/// Called whenever the middle mouse button is pressed down inside the widget.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnMiddleMouseDownInside { get; set; }

	/// <summary>
	/// Called whenever the middle mouse button is released.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnMiddleMouseUp { get; set; }

	/// <summary>
	/// Called while the middle mouse button is being held down.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnMiddleMousePress { get; set; }

	/// <summary>
	/// Called whenever the mouse wheel is scrolled.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnMouseWheel { get; set; }

	/// <summary>
	/// Called whenever the left mouse button is pressed down twice inside the widget in short succession.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MouseEvent OnDoubleLeftMouseDownInside { get; set; }

	/// <summary>
	/// Called when this widget becomes the active widget.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BaseEvent OnWidgetSelected { get; set; }

	/// <summary>
	/// Called when this widget is no longer the active widget.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BaseEvent OnWidgetDeselected { get; set; }

	/// <summary>
	/// Called when a button is being is pressed.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public TextEvent OnTextInput { get; set; }

	/// <summary>
	/// Called when this widget's relative position changes.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public ObjectEvent OnPositionChanged { get; set; }

	/// <summary>
	/// Called when this widget's relative size changes.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public ObjectEvent OnSizeChanged { get; set; }

	/// <summary>
	/// Called when this widget's parent relative size changes.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public ObjectEvent OnParentSizeChanged { get; set; }

	/// <summary>
	/// Called when a child's relative size changes.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public ObjectEvent OnChildBoundsChanged { get; set; }

	/// <summary>
	/// Called before this widget is disposed.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BoolEvent OnDisposing { get; set; }

	/// <summary>
	/// Called after this widget is disposed.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BaseEvent OnDisposed { get; set; }

	/// <summary>
	/// Called when the autoscroll scrollbars are being scrolled.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BaseEvent OnScrolling { get; set; }

	/// <summary>
	/// Called before the right-click menu would open. Is cancellable.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BoolEvent OnContextMenuOpening { get; set; }

	/// <summary>
	/// Called upon creation of the help text widget.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BaseEvent OnHelpTextWidgetCreated { get; set; }

	/// <summary>
	/// Called whenever the help text is to be retrieved.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public StringEvent OnFetchHelpText { get; set; }

	/// <summary>
	/// Called whenever SetVisibility() is called.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BoolEvent OnVisibilityChanged { get; set; }

	/// <summary>
	/// Called whenever SetZIndex is called.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BaseEvent OnZIndexChanged { get; set; }

	/// <summary>
	/// Called whenever the padding is changed.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public ObjectEvent OnPaddingChanged { get; set; }

	/// <summary>
	/// Called whenever an animation finishes.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public ObjectEvent OnAnimationFinished { get; set; }

	/// <summary>
	/// Called whenever the widget updates.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public BaseEvent OnUpdate { get; set; }

	/// <summary>
	/// Indicates whether the context menu is set to be opened on next update.
	/// </summary>
	public bool OpenContextMenuOnNextUpdate = false;

	/// <summary>
	/// The global mouse coordinates of the point at which the context menu was opened.
	/// </summary>
	public Point ContextMenuMouseOrigin;


	/// <summary>
	/// Creates a new Widget object.
	/// </summary>
	/// <param name="Parent">The Parent widget.</param>
	/// <param name="Name">The unique name to give this widget by which to store it.</param>
	/// <param name="Index">Optional index parameter. Used internally for stackpanels.</param>
	public Widget(IContainer Parent, int ParentWidgetIndex = -1)
	{
		SetParent(Parent, ParentWidgetIndex);
		// Create new viewport directly on the window's renderer.
		Viewport = new Viewport(Window, 0, 0, Size);
		// Z index by default copies parent viewport's z. If changed later, SetZIndex will modify the value.
		Viewport.Z = ZIndex;
		if (this.Parent is Widget) Viewport.Visible = (this.Parent as Widget).IsVisible() ? Visible : false;
		// The background sprite responsible for the BackgroundColor.
		Sprites["_bg"] = new Sprite(Viewport);
		Sprites["_bg"].Bitmap = new SolidBitmap(Size, BackgroundColor);
		// In the same viewport as all other sprites, but at a very large negative Z index.
		Sprites["_bg"].Z = -999999999;
		// Set some default events.

		OnMouseMoving = MouseMoving;
		OnHoverChanged = HoverChanged;
		OnMouseDown = MouseDown;
		OnMouseUp = MouseUp;
		OnMousePress = MousePress;
		OnLeftMouseDown = LeftMouseDown;
		OnLeftMouseDownInside = LeftMouseDownInside;
		OnLeftMouseUp = LeftMouseUp;
		OnLeftMousePress = LeftMousePress;
		OnRightMouseDown = RightMouseDown;
		OnRightMouseDownInside = RightMouseDownInside;
		OnRightMouseUp = RightMouseUp;
		OnRightMousePress = RightMousePress;
		OnMiddleMouseDown = MiddleMouseDown;
		OnMiddleMouseDownInside = MiddleMouseDownInside;
		OnMiddleMouseUp = MiddleMouseUp;
		OnMiddleMousePress = MiddleMousePress;
		OnMouseWheel = MouseWheel;
		OnDoubleLeftMouseDownInside = DoubleLeftMouseDownInside;
		OnWidgetDeselected = WidgetDeselected;
		OnTextInput = TextInput;
		OnPositionChanged = PositionChanged;
		OnParentSizeChanged = ParentSizeChanged;
		OnSizeChanged = SizeChanged;
		OnChildBoundsChanged = ChildBoundsChanged;
		// Creates the input manager object responsible for fetching mouse input.
		Mouse = new MouseManager(this);
		OnFetchHelpText = FetchHelpText;
		SetVisible(true);
	}

	~Widget()
	{
		if (!Disposed)
		{
			ODL.Logger?.Warn($"GC is collecting an undisposed widget: {GetType()}");
		}
	}

	/// <summary>
	/// Initializes the list of right-click menu options.
	/// </summary>
	/// <param name="Items">The list of menu items.</param>
	public virtual void SetContextMenuList(List<IMenuItem> Items)
	{
		AssertUndisposed();
		ContextMenuList = Items;
		ShowContextMenu = Items.Count > 0;
	}

	/// <summary>
	/// Sets the font of the context menu.
	/// </summary>
	/// <param name="Font">The font to use in the context menu.</param>
	public virtual void SetContextMenuFont(Font Font)
	{
		ContextMenuFont = Font;
	}

	/// <summary>
	/// Sets the help message that appears when hovering over the widget.
	/// </summary>
	/// <param name="Text"></param>
	public void SetHelpText(string Text)
	{
		HelpText = Text;
	}

	/// <summary>
	/// Initializes the list of shortcuts.
	/// </summary>
	/// <param name="Shortcuts">The list of shortcuts.</param>
	public virtual void RegisterShortcuts(List<Shortcut> Shortcuts, bool DeregisterExisting = true)
	{
		AssertUndisposed();
		if (DeregisterExisting)
		{
			// De-register old global shortcuts in the UIManager object.
			RemoveShortcuts();
			this.Shortcuts = Shortcuts;
		}
		else this.Shortcuts.AddRange(Shortcuts);
		// Register global shortcuts in the UIManager object.
		foreach (Shortcut s in Shortcuts)
		{
			if (s.GlobalShortcut) Window.UI.RegisterShortcut(s);
		}
	}

	/// <summary>
	/// Removes all registered shortcuts.
	/// </summary>
	public virtual void RemoveShortcuts()
	{
		foreach (Shortcut s in Shortcuts)
		{
			if (s.GlobalShortcut) Window.UI.DeregisterShortcut(s);
			else
			{
				if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
				if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
			}
		}
		ValidShortcutInputs.Clear();
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
		Viewport.Z = this.ZIndex;
		Widgets.ForEach(w => w.SetZIndex(w.ZIndex));
		OnZIndexChanged?.Invoke(new BaseEventArgs());
	}

	/// <summary>
	/// Registers this widget under the parent widget.
	/// </summary>
	/// <param name="Parent">The Parent widget.</param>
	/// <param name="Index">Optional index for stackpanel parents.</param>
	public virtual void SetParent(IContainer Parent, int ParentWidgetIndex = -1)
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
			Window = (Parent as UIManager).Window;
			this.Parent = Parent;
		}
		else if (Parent is Widget)
		{
			Window = (Parent as Widget).Window;
			this.Parent = Parent;
		}
		this.Parent.Add(this, ParentWidgetIndex);
		if (Viewport != null)
		{
			Viewport.Z = ZIndex;
			Viewport.Visible = (this.Parent as Widget).IsVisible() ? Visible : false;
		}
		WindowLayer = Parent.WindowLayer;
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
		bool wasVisible = this.Visible;
		this.Visible = Visible;
		Widget parent = Parent as Widget;
		if (parent == null || parent.IsVisible())
		{
			Viewport.Visible = Visible;
		}
		else
		{
			Viewport.Visible = false;
		}
		SetViewportVisible(Visible);
		if (this.Visible != wasVisible) OnVisibilityChanged?.Invoke(new BoolEventArgs(this.Visible));
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
		else Viewport.Visible = false;
		Widgets.ForEach(w => w.SetViewportVisible(Visible));
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
		Widgets.ForEach(wdgt =>
		{
			if (!wdgt.Visible || !wdgt.ConsiderInAutoScrollCalculation) return;
			int w = wdgt.Size.Width;
			w += wdgt.Position.X;
			if (w > MaxChildWidth) MaxChildWidth = w;
		});
		// Calculate total child height
		int OldMaxChildHeight = MaxChildHeight;
		MaxChildHeight = 0;
		Widgets.ForEach(w =>
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
				if (MaxChildWidth > Size.Width - ChildPadding.Left + ChildPadding.Right)
				{
					if (HScrollBar == null)
					{
						throw new Exception("Autoscroll was enabled, but no scrollbar has been defined.");
					}
					int effW = Viewport.Width - ChildPadding.Left + ChildPadding.Right;
					if (OldMaxChildWidth - effW > 0 && ScrolledX > OldMaxChildWidth - effW)
					{
						ScrolledX = OldMaxChildWidth - effW;
					}
					if (ScrolledX > MaxChildWidth - effW)
					{
						ScrolledX = MaxChildWidth - effW;
					}
					HScrollBar.SetValue((double)ScrolledX / (MaxChildWidth - effW), false);
					HScrollBar.SetSliderSize((double) effW / MaxChildWidth);
					HScrollBar.MouseInputRect = Viewport.Rect;
					HScrollBar.SetVisible(!HScrollBar.KeepInvisible);
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
				if (MaxChildHeight > Size.Height - ChildPadding.Up + ChildPadding.Down)
				{
					if (VScrollBar == null)
					{
						throw new Exception("Autoscroll was enabled, but no scrollbar has been defined.");
					}
					int effH = Viewport.Height - ChildPadding.Up + ChildPadding.Down;
					if (OldMaxChildHeight - effH > 0 && ScrolledY > OldMaxChildHeight - effH)
					{
						ScrolledY = OldMaxChildHeight - effH;
					}
					if (ScrolledY > MaxChildHeight - effH)
					{
						ScrolledY = MaxChildHeight - effH;
					}
					VScrollBar.SetValue((double)ScrolledY / (MaxChildHeight - effH), false);
					VScrollBar.SetSliderSize((double) effH / MaxChildHeight);
					VScrollBar.MouseInputRect = Viewport.Rect;
					VScrollBar.SetVisible(!VScrollBar.KeepInvisible);
				}
				else if (VScrollBar != null)
				{
					VScrollBar.SetVisible(false);
					ScrolledY = 0;
				}
			}
		}
		// Update positions
		UpdateBounds();
	}

	/// <summary>
	/// Links the given HScrollBar with this widget.
	/// </summary>
	public void SetHScrollBar(ScrollBar hsb)
	{
		HScrollBar = hsb;
		hsb.MouseInputRect = Viewport.Rect;
		hsb.LinkedWidget = this;
	}

	/// <summary>
	/// Links the given VScrollBar with this widget.
	/// </summary>
	public void SetVScrollBar(ScrollBar vsb)
	{
		VScrollBar = vsb;
		vsb.MouseInputRect = Viewport.Rect;
		vsb.LinkedWidget = this;
	}

	protected bool PretendToBeAtOrigin = false;

	/// <summary>
	/// Updates this viewport's boundaries if it exceeds the parent viewport's boundaries and applies scrolling.
	/// </summary>
	public void UpdateBounds(bool PretendToBeAtOrigin = false)
	{
		this.PretendToBeAtOrigin = PretendToBeAtOrigin;
		AssertUndisposed();
		foreach (Sprite sprite in Sprites.Values) sprite.OX = sprite.OY = 0;

		int xoffset = ConsiderInAutoScrollPositioningX ? Parent.ScrolledX : 0;
		int yoffset = ConsiderInAutoScrollPositioningY ? Parent.ScrolledY : 0;

		if (PretendToBeAtOrigin)
		{
			Viewport.X = 0;
			Viewport.Y = 0;
		}
		else
		{
			bool ParentAtOrigin = Parent is Widget && ((Widget) Parent).PretendToBeAtOrigin;
			Viewport.X = Parent.Viewport.X + Position.X + Padding.Left - (ParentAtOrigin ? 0 : Parent.LeftCutOff) - xoffset;
			Viewport.Y = Parent.Viewport.Y + Position.Y + Padding.Up - (ParentAtOrigin ? 0 : Parent.TopCutOff) - yoffset;
		}
		int origY = Viewport.Y;
		Viewport.Width = Size.Width;
		Viewport.Height = Size.Height;
		int parentPosOffsetX = 0;
		int parentPosOffsetY = 0;
		int parentSizeOffsetWidth = 0;
		int parentSizeOffsetHeight = 0;
		if (Parent is Widget && this is not ScrollBar)
		{
			Viewport.X += parentPosOffsetX = ((Widget) Parent).ChildPadding.Left;
			Viewport.Y += parentPosOffsetY = ((Widget) Parent).ChildPadding.Up;
			parentSizeOffsetWidth = ((Widget) Parent).ChildPadding.Right;
			parentSizeOffsetHeight = ((Widget) Parent).ChildPadding.Down;
		}

		if (!PretendToBeAtOrigin)
		{
			// Handles width exceeding parent viewport
			if (Viewport.X + Size.Width > Parent.Viewport.X + Parent.Viewport.Width + parentSizeOffsetWidth)
			{
				int Diff = Viewport.X + Size.Width - (Parent.Viewport.X + Parent.Viewport.Width + parentSizeOffsetWidth);
				Viewport.Width -= Diff;
			}
			// Handles X being negative
			if (Viewport.X < Parent.Viewport.X + parentPosOffsetX)
			{
				int Diff = Parent.Viewport.X + parentPosOffsetX - Viewport.X;
				Viewport.X += Diff;
				Viewport.Width -= Diff;
				foreach (Sprite sprite in Sprites.Values) sprite.OX += (int)Math.Round(Diff / sprite.ZoomX);
				LeftCutOff = Diff;
			}
			else LeftCutOff = 0;
			// Handles height exceeding parent viewport
			if (Viewport.Y + Size.Height > Parent.Viewport.Y + Parent.Viewport.Height + parentSizeOffsetHeight)
			{
				int Diff = Viewport.Y + Size.Height - (Parent.Viewport.Y + Parent.Viewport.Height + parentSizeOffsetHeight);
				Viewport.Height -= Diff;
			}
			// Handles Y being negative
			if (Viewport.Y < Parent.Viewport.Y + parentPosOffsetY)
			{
				int Diff = Parent.Viewport.Y + parentPosOffsetY - Viewport.Y;
				Viewport.Y += Diff;
				Viewport.Height -= Diff;
				foreach (Sprite sprite in Sprites.Values) sprite.OY += (int)Math.Round(Diff / sprite.ZoomY);
				TopCutOff = Diff;
			}
			else TopCutOff = 0;
		}

		Widgets.ForEach(child => child.UpdateBounds());
	}

	private List<Viewport> GetAllChildViewports()
	{
		List<Viewport> Viewports = new List<Viewport>();
		foreach (Widget w in Widgets)
		{
			Viewports.Add(w.Viewport);
			Viewports.AddRange(w.GetAllChildViewports());
		}
		return Viewports;
	}

	public Bitmap ToBitmap(int xoffset = 0, int yoffset = 0, int width = 0, int height = 0)
	{
		List<Viewport> Viewports = new List<Viewport>() { Viewport };
		Viewports.AddRange(GetAllChildViewports());
		UpdateBounds(true);
		if (width == 0) width = Size.Width;
		if (height == 0) height = Size.Height;
		width += xoffset * 2;
		height += yoffset * 2;
		return Graphics.RenderToBitmap(Viewports, width, height, xoffset, yoffset);
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
		RightDocked = !LeftDocked;
		this.LeftDocked = LeftDocked;
		UpdatePositionAndSizeIfDocked();
	}

	public virtual void SetRightDocked(bool RightDocked)
	{
		this.RightDocked = RightDocked;
		LeftDocked = !RightDocked;
		UpdatePositionAndSizeIfDocked();
	}

	public virtual void SetTopDocked(bool TopDocked)
	{
		BottomDocked = !TopDocked;
		this.TopDocked = TopDocked;
		UpdatePositionAndSizeIfDocked();
	}

	public virtual void SetBottomDocked(bool BottomDocked)
	{
		this.BottomDocked = BottomDocked;
		TopDocked = !BottomDocked;
		UpdatePositionAndSizeIfDocked();
	}

	public void SetGlobalOpacity(byte Opacity)
	{
		SetOpacity(Opacity);
		Widgets.ForEach(w => w.SetGlobalOpacity(Opacity));
	}

	public void SetOpacity(byte Opacity)
	{
		if (this.Opacity != Opacity)
		{
			this.Opacity = Opacity;
			Viewport.Opacity = Opacity;
		}
	}

	public void SetZoomX(float ZoomX)
	{
		if (this.ZoomX != ZoomX)
		{
			this.ZoomX = ZoomX;
			Viewport.ZoomX = ZoomX;
		}
	}

	public void SetZoomY(float ZoomY)
	{
		if (this.ZoomY != ZoomY)
		{
			this.ZoomY = ZoomY;
			Viewport.ZoomY = ZoomY;
		}
	}

	public void SetZoom(float Zoom)
	{
		SetZoomX(Zoom);
		SetZoomY(Zoom);
	}

	public void SetGlobalZoom(float Zoom, Viewport OriginViewport = null, int OriginAddX = 0, int OriginAddY = 0)
	{
		SetZoomX(Zoom);
		SetZoomY(Zoom);
		if (OriginViewport == null) OriginViewport = Viewport;
		Widgets.ForEach(w =>
		{
			OriginAddX += w.Position.X + w.Padding.Left - w.ScrolledX;
			OriginAddY += w.Position.Y + w.Padding.Up - w.ScrolledY;
			int diffx = (int)Math.Round((1 - OriginViewport.ZoomX) * OriginAddX);
			int diffy = (int)Math.Round((1 - OriginViewport.ZoomY) * OriginAddY);
			w.Viewport.OX = diffx;
			w.Viewport.OY = diffy;
			w.SetGlobalZoom(Zoom, OriginViewport, OriginAddX, OriginAddY);
			OriginAddX -= w.Position.X + w.Padding.Left + w.ScrolledX;
			OriginAddY -= w.Position.Y + w.Padding.Up + w.ScrolledY;
		});
	}

	public void SetGlobalZoomX(float ZoomX, Viewport OriginViewport = null, int OriginAddX = 0)
	{
		SetZoomX(ZoomX);
		if (OriginViewport == null) OriginViewport = Viewport;
		Widgets.ForEach(w =>
		{
			OriginAddX += w.Position.X + w.Padding.Left;
			int diffx = (int)Math.Round((1 - OriginViewport.ZoomX) * OriginAddX);
			w.Viewport.OX = diffx;
			w.SetGlobalZoomX(ZoomX, OriginViewport, OriginAddX);
			OriginAddX -= w.Position.X + w.Padding.Left;
		});
	}

	public void SetGlobalZoomY(float ZoomY, Viewport OriginViewport = null, int OriginAddY = 0)
	{
		SetZoomY(ZoomY);
		if (OriginViewport == null) OriginViewport = Viewport;
		Widgets.ForEach(w =>
		{
			OriginAddY += w.Position.Y + w.Padding.Up;
			int diffy = (int)Math.Round((1 - OriginViewport.ZoomY) * OriginAddY);
			w.Viewport.OY = diffy;
			w.SetGlobalZoomY(ZoomY, OriginViewport, OriginAddY);
			OriginAddY -= w.Position.Y + w.Padding.Up;
		});
	}

	/// <summary>
	/// Sets the widget margins.
	/// </summary>
	/// <param name="All">Margins in all directions.</param>
	public virtual void SetMargins(int All)
	{
		SetMargins(new Margins(All));
	}

	/// <summary>
	/// Sets the widget margins.
	/// </summary>
	/// <param name="Horizontal">Horizontal margins.</param>
	/// <param name="Vertical">Vertical margins.</param>
	public virtual void SetMargins(int Horizontal, int Vertical)
	{
		SetMargins(new Margins(Horizontal, Vertical));
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
		SetMargins(new Margins(Left, Up, Right, Down));
	}

	/// <summary>
	/// Sets the widget margins.
	/// </summary>
	/// <param name="Padding">The widget's margins.</param>
	public virtual void SetMargins(Margins Margins)
	{
		this.Margins = Margins;
		UpdateLayout();
	}

	/// <summary>
	/// Sets the widget padding.
	/// </summary>
	/// <param name="All">Padding in all directions.</param>
	public virtual void SetPadding(int All)
	{
		SetPadding(new Padding(All));
	}

	/// <summary>
	/// Sets the widget padding.
	/// </summary>
	/// <param name="Horizontal">Horizontal padding.</param>
	/// <param name="Vertical">Vertical padding.</param>
	public virtual void SetPadding(int Horizontal, int Vertical)
	{
		SetPadding(new Padding(Horizontal, Vertical));
	}

	/// <summary>
	/// Sets the widget padding.
	/// </summary>
	/// <param name="Left">Left padding.</param>
	/// <param name="Up">Top padding.</param>
	/// <param name="Right">Right padding.</param>
	/// <param name="Down">Bottom padding.</param>
	public virtual void SetPadding(int Left, int Up, int Right, int Down)
	{
		SetPadding(new Padding(Left, Up, Right, Down));
	}

	/// <summary>
	/// Sets the widget padding.
	/// </summary>
	/// <param name="Padding">The widget's padding.</param>
	public virtual void SetPadding(Padding Padding)
	{
		if (!this.Padding.Equals(Padding))
		{
			Padding OldPadding = this.Padding;
			this.Padding = Padding;
			UpdatePositionAndSizeIfDocked();
			UpdateLayout();
			UpdateBounds();
			OnPaddingChanged?.Invoke(new ObjectEventArgs(this.Padding, OldPadding));
		}
	}

	/// <summary>
	/// Updates the widget's size based on its docked state.
	/// </summary>
	public virtual void UpdatePositionAndSizeIfDocked()
	{
		if (HDocked || VDocked)
		{
			int neww = Size.Width;
			int newh = Size.Height;
			if (HDocked) neww = Parent.Size.Width - Position.X - Padding.Left - Padding.Right;
			if (VDocked) newh = Parent.Size.Height - Position.Y - Padding.Up - Padding.Down;
			SetSize(neww, newh);
		}
		if (RightDocked || BottomDocked)
		{
			int newx = Position.X;
			int newy = Position.Y;
			if (RightDocked) newx = Parent.Size.Width - Size.Width - Padding.Right;
			if (BottomDocked) newy = Parent.Size.Height - Size.Height - Padding.Down;
			SetPosition(newx, newy);
		}
	}

	/// <summary>
	/// Changes the relative position of this widget.
	/// </summary>
	public void SetPosition(int X, int Y)
	{
		SetPosition(new Point(X, Y));
	}
	/// <summary>
	/// Changes the relative position of this widget.
	/// </summary>
	public virtual void SetPosition(Point p)
	{
		AssertUndisposed();
		if (Position.X != p.X || Position.Y != p.Y)
		{
			Point OldPosition = Position;
			Position = p;
			UpdatePositionAndSizeIfDocked();
			UpdateBounds();
			OnPositionChanged(new ObjectEventArgs(Position, OldPosition));
		}
	}

	/// <summary>
	/// Sets the width of this widget.
	/// </summary>
	public Widget SetWidth(int Width)
	{
		return SetSize(Width, Size.Height);
	}
	/// <summary>
	/// Sets the height of this widget.
	/// </summary>
	public Widget SetHeight(int Height)
	{
		return SetSize(Size.Width, Height);
	}
	/// <summary>
	/// Sets the size of this widget.
	/// </summary>
	public Widget SetSize(int Width, int Height)
	{
		return SetSize(new Size(Width, Height));
	}
	/// <summary>
	/// Sets the size of this widget.
	/// </summary>
	public virtual Widget SetSize(Size size)
	{
		AssertUndisposed();
		Size OldSize = Size;
		// Ensures the set size matches the parent size if the widget is docked
		size.Width = HDocked ? Parent.Size.Width - Position.X - Padding.Left - Padding.Right : size.Width;
		size.Height = VDocked ? Parent.Size.Height - Position.Y - Padding.Up - Padding.Down : size.Height;
		// Ensures the new size doesn't exceed the set minimum and maximum values.
		if (size.Width < MinimumSize.Width) size.Width = MinimumSize.Width;
		else if (size.Width > MaximumSize.Width && MaximumSize.Width != -1) size.Width = MaximumSize.Width;
		if (size.Height < MinimumSize.Height) size.Height = MinimumSize.Height;
		else if (size.Height > MaximumSize.Height && MaximumSize.Height != -1) size.Height = MaximumSize.Height;
		if (OldSize.Width != size.Width || OldSize.Height != size.Height)
		{
			Size = size;
			// Update the background sprite's size
			(Sprites["_bg"].Bitmap as SolidBitmap).SetSize(Size);
			Viewport.Width = Size.Width;
			Viewport.Height = Size.Height;
			// Updates the viewport boundaries
			UpdateBounds();
			// Executes all events associated with resizing a widget.
			Widgets.ForEach(w =>
			{
				w.OnParentSizeChanged(new ObjectEventArgs(Size, OldSize));
				w.OnSizeChanged(new ObjectEventArgs(w.Size, w.Size));
			});
			OnSizeChanged(new ObjectEventArgs(Size, OldSize));
			Widgets.ForEach(child => child.UpdatePositionAndSizeIfDocked());
			Redraw();
			if (Parent is Widget && !(this is ScrollBar))
			{
				Widget prnt = Parent as Widget;
				prnt.OnChildBoundsChanged(new ObjectEventArgs(Size));
			}
		}
		return this;
	}

	/// <summary>
	/// Sets the padding of this widget's children.
	/// </summary>
	/// <param name="left">The Left component of the padding.</param>
	/// <param name="up">The Up component of the padding.</param>
	/// <param name="right">The Right component of the padding.</param>
	/// <param name="down">The Down component of the padding.</param>
	public void SetChildPadding(int left, int up, int right, int down)
	{
		SetPadding(new Padding(left, up, right, down));
	}

	/// <summary>
	/// Sets the padding of this widget's children.
	/// </summary>
	/// <param name="horizontal">The horizontal component of the padding.</param>
	/// <param name="vertical">The vertical component of the padding.</param>
	public void SetChildPadding(int horizontal, int vertical)
	{
		SetChildPadding(new Padding(horizontal, vertical));
	}

	/// <summary>
	/// Sets the padding of this widget's children.
	/// </summary>
	/// <param name="all">All components of the padding.</param>
	public void SetChildPadding(int all)
	{
		SetChildPadding(new Padding(all));
	}

	/// <summary>
	/// Sets the padding of this widget's children.
	/// </summary>
	/// <param name="offset">The padding.</param>
	public void SetChildPadding(Padding childPadding)
	{
		if (!this.ChildPadding.Equals(ChildPadding))
		{
			this.ChildPadding = childPadding;
			this.UpdateBounds();
		}
	}

	/// <summary>
	/// Sets the background color of this widget.
	/// </summary>
	public void SetBackgroundColor(byte r, byte g, byte b, byte a = 255)
	{
		SetBackgroundColor(new Color(r, g, b, a));
	}
	/// <summary>
	/// Sets the background color of this widget.
	/// </summary>
	public void SetBackgroundColor(Color c)
	{
		AssertUndisposed();
		BackgroundColor = c;
		(Sprites["_bg"].Bitmap as SolidBitmap).SetColor(c);
	}

	/// <summary>
	/// Sets a timer.
	/// </summary>
	/// <param name="identifier">Unique string identifier.</param>
	/// <param name="milliseconds">Number of milliseconds to run the timer for.</param>
	public void SetTimer(string identifier, long milliseconds)
	{
		Timers.Add(new Timer(identifier, Stopwatch.GetTimestamp(), milliseconds * Amethyst.TicksPerMillisecond));
	}

	/// <summary>
	/// Returns whether or not the specified timer's time has elapsed.
	/// </summary>
	public bool TimerPassed(string identifier)
	{
		Timer t = Timers.Find(timer => timer.Identifier == identifier);
		if (t == null) return false;
		return Stopwatch.GetTimestamp() >= t.StartTime + t.Timespan;
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
		t.StartTime = Stopwatch.GetTimestamp();
	}

	public void SetCallback(long Milliseconds, Action Callback)
	{
		string timername = $"callback_{Random.Shared.Next()}";
		SetTimer(timername, Milliseconds);
		Callbacks.Add((timername, Callback));
	}

	/// <summary>
	/// Starts an animation on this widget.
	/// </summary>
	/// <param name="Animation">The animation to run.</param>
	public void StartAnimation(IAnimation Animation)
	{
		// Start the animation after one additional update cycle, so that we know all our widgets
		// have already been resized to whatever size they need to be.
		Graphics.Schedule(() =>
		{
			Animations.Add(Animation);
			Animation.Start();
		});
	}

	/// <summary>
	/// Pauses the animation with the specified ID.
	/// </summary>
	/// <param name="ID">The ID of the animation to pause.</param>
	public void PauseAnimation(string ID)
	{
		IAnimation Anim = Animations.Find(a => a.ID == ID);
		if (Anim == null) throw new Exception($"No animation could be found with the ID '{ID}'.");
		PauseAnimation(Anim);
	}

	/// <summary>
	/// Pauses the specified animation.
	/// </summary>
	/// <param name="Animation">The animation to pause.</param>
	public void PauseAnimation(IAnimation Animation)
	{
		Animation.Pause();
	}

	/// <summary>
	/// Resumes the animation with the specified ID.
	/// </summary>
	/// <param name="ID">The ID of the animation to resume.</param>
	public void ResumeAnimation(string ID)
	{
		IAnimation Anim = Animations.Find(a => a.ID == ID);
		if (Anim == null) throw new Exception($"No animation could be found with the ID '{ID}'.");
		ResumeAnimation(Anim);
	}

	/// <summary>
	/// Resumes the specified animation.
	/// </summary>
	/// <param name="Animation">The animation to resume.</param>
	public void ResumeAnimation(IAnimation Animation)
	{
		Animation.Resume();
	}

	/// <summary>
	/// Returns whether the animation with the specified ID is paused.
	/// </summary>
	/// <param name="ID">The ID of the animation.</param>
	/// <returns>Whether the animation is paused.</returns>
	public bool IsAnimationPaused(string ID)
	{
		IAnimation Anim = Animations.Find(a => a.ID == ID);
		if (Anim == null) throw new Exception($"No animation could be found with the ID '{ID}'.");
		return IsAnimationPaused(Anim);
	}

	/// <summary>
	/// Returns whether the specified animation is paused.
	/// </summary>
	/// <param name="Animation">The animation.</param>
	/// <returns>Whether the animation is paused.</returns>
	public bool IsAnimationPaused(IAnimation Animation)
	{
		return Animation.Paused;
	}

	/// <summary>
	/// Returns whether an animation exists with the specified ID.
	/// </summary>
	/// <param name="ID">The ID to test for.</param>
	/// <returns>Whether an animation with the specified ID exists.</returns>
	public bool AnimationExists(string ID)
	{
		return Animations.Any(anim => anim.ID == ID);
	}

	/// <summary>
	/// Stops the animation with the specified ID.
	/// </summary>
	/// <param name="ID">The ID of the animation to stop.</param>
	public void StopAnimation(string ID)
	{
		IAnimation Anim = Animations.Find(a => a.ID == ID);
		if (Anim == null) throw new Exception($"No animation could be found with the ID '{ID}'.");
		StopAnimation(Anim);
	}

	/// <summary>
	/// Stops the specified animation.
	/// </summary>
	/// <param name="Animation">The animation to stop.</param>
	public void StopAnimation(IAnimation Animation)
	{
		Animation.Stop();
		Animations.Remove(Animation);
	}

	/// <summary>
	/// Adds a Widget object to this widget.
	/// </summary>
	public virtual void Add(Widget w, int Index = -1)
	{
		if (Index == -1) Widgets.Add(w);
		else Widgets.Insert(Index, w);
	}

	/// <summary>
	/// Removes a child widget and returns the deregistered widget.
	/// </summary>
	public virtual Widget Remove(Widget w)
	{
		for (int i = 0; i < Widgets.Count; i++)
		{
			if (Widgets[i] == w)
			{
				Widgets.RemoveAt(i);
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
		SetGridRow(Row, Row);
	}
	/// <summary>
	/// Sets the grid row this widget starts and ends in. Used for grids.
	/// </summary>
	/// <param name="RowStart"></param>
	/// <param name="RowEnd"></param>
	/// <returns></returns>
	public void SetGridRow(int RowStart, int RowEnd)
	{
		GridRowStart = RowStart;
		GridRowEnd = RowEnd;
		UpdateLayout();
	}

	/// <summary>
	/// Sets the grid column this widget is in. Used for grids.
	/// </summary>
	/// <param name="Column"></param>
	/// <returns></returns>
	public void SetGridColumn(int Column)
	{
		SetGridColumn(Column, Column);
	}
	/// <summary>
	/// Sets the grid column this widget starts and ends in. Used for grids.
	/// </summary>
	public void SetGridColumn(int ColumnStart, int ColumnEnd)
	{
		GridColumnStart = ColumnStart;
		GridColumnEnd = ColumnEnd;
		UpdateLayout();
	}

	/// <summary>
	/// Sets the grid row and column this widget is in. Used for grids.
	/// </summary>
	/// <param name="Row"></param>
	/// <param name="Column"></param>
	/// <returns></returns>
	public void SetGrid(int Row, int Column)
	{
		SetGrid(Row, Row, Column, Column);
	}
	/// <summary>
	/// Sets the grid row and column this widget starts and ends in. Used for grids.
	/// </summary>
	public void SetGrid(int RowStart, int RowEnd, int ColumnStart, int ColumnEnd)
	{
		GridRowStart = RowStart;
		GridRowEnd = RowEnd;
		GridColumnStart = ColumnStart;
		GridColumnEnd = ColumnEnd;
		UpdateLayout();
	}

	/// <summary>
	/// Updates the grid or stackpanel's layout.
	/// </summary>
	public virtual void UpdateLayout()
	{
		if (Parent is ILayout)
		{
			(Parent as ILayout).NeedUpdate = true;
		}
	}

	/// <summary>
	/// Disposes this widget, its viewport and all its children.
	/// </summary>
	public virtual void Dispose()
	{
		AssertUndisposed();
		if (OnDisposing != null)
		{
			BoolEventArgs arg = new BoolEventArgs(true);
			OnDisposing.Invoke(arg);
			if (!arg.Value) return;
		}
		// Mark this widget as disposed.
		Disposed = true;
		foreach (Shortcut s in Shortcuts)
		{
			if (s.GlobalShortcut) s.PendingRemoval = true;
		}
		// Dispose the viewport and all its sprites.
		// Viewport may already be null if a child of a layoutcontainer has been disposed
		// because they share the same viewport.
		if (Viewport != null && !Viewport.Disposed)
			Viewport.Dispose();
		if (HelpTextWidget != null) HelpTextWidget.Dispose();
		// Dispose all child widgets
		for (int i = Widgets.Count - 1; i >= 0; i--) Widgets[i].Dispose();
		if (!Parent.Widgets.Remove(this))
		{
			if (Parent.Widgets.Contains(this))
			{
				throw new Exception("Failed to detach widget from parent.");
			}
		}
		// Set viewport and sprites to null to ensure no methods can use them anymore.
		Viewport = null;
		Sprites = null;
		OnDisposed?.Invoke(new BaseEventArgs());
	}

	/// <summary>
	/// Ensures this widget is not disposed by raising an exception if it is.
	/// </summary>
	protected void AssertUndisposed()
	{
		if (Disposed)
		{
			throw new ObjectDisposedException(ToString());
		}
	}

	/// <summary>
	/// Marks the sprites as needing an redraw next iteration.
	/// This allows you to call Redraw multiple times at once, as it will only redraw once in the next iteration.
	/// </summary>
	public virtual void Redraw()
	{
		AssertUndisposed();
		Drawn = false;
	}

	/// <summary>
	/// The method that redraws this widget and its sprites.
	/// </summary>
	protected virtual void Draw()
	{
		AssertUndisposed();
		Drawn = true;
	}

	/// <summary>
	/// Updates this widget and its children. Do not put excessive logic in here.
	/// </summary>
	public virtual void Update()
	{
		AssertUndisposed();

		OnUpdate?.Invoke(new BaseEventArgs());

		long Ticks = Stopwatch.GetTimestamp();
		int j = 0;
		while (j < Animations.Count)
		{
			if (Animations[j].Paused)
			{
				j++;
				continue;
			}
			double factor = Math.Clamp((Ticks - Animations[j].StartTicks) / (double)(Animations[j].EndTicks - Animations[j].StartTicks), 0, 1);
			Animations[j].Execute(factor);
			if (Ticks >= Animations[j].EndTicks)
			{
				Animations[j].OnFinished?.Invoke();
				OnAnimationFinished?.Invoke(new ObjectEventArgs(Animations[j]));
				Animations.RemoveAt(j);
			}
			else j++;
		}

		// Return if the widget was disposed after an animation
		if (Disposed) return;

		j = 0;
		while (j < Callbacks.Count)
		{
			string timername = Callbacks[j].TimerName;
			if (TimerExists(timername))
			{
				if (TimerPassed(timername))
				{
					Callbacks[j].Callback();
					Callbacks.RemoveAt(j);
				}
				else j++;
			}
			else Callbacks.RemoveAt(j);
		}

		if (OpenContextMenuOnNextUpdate)
		{
			OpenContextMenuOnNextUpdate = false;
			ContextMenu cm = new ContextMenu(Window.UI);
			cm.SetFont(ContextMenuFont ?? DefaultContextMenuFont);
			cm.SetItems(ContextMenuList);
			Size s = cm.Size;
			int x = ContextMenuMouseOrigin.X;
			int y = ContextMenuMouseOrigin.Y;
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
		if (HelpTextWidget != null || TimerExists("helptext") && (WindowLayer < Window.ActiveWidget.WindowLayer || !IsVisible()))
		{
			if (TimerExists("helptext")) ResetTimer("helptext");
			if (HelpTextWidget != null) HelpTextWidget.Dispose();
			HelpTextWidget = null;
		}

		// If this widget is active and selected
		if (SelectedWidget && WindowLayer >= Window.ActiveWidget.WindowLayer && IsVisible())
		{
			// Execute shortcuts if their buttons is being triggered.
			foreach (Shortcut s in Shortcuts)
			{
				if (s.GlobalShortcut) continue; // Handled by the UIManager

				Key k = s.Key;
				bool Valid = false;
				if (Input.Press(k.MainKey))
				{
					if (TimerPassed($"key_{k.ID}") && ValidShortcutInputs.Contains(k.ID))
					{
						ResetTimer($"key_{k.ID}");
						Valid = true;
					}
					else if (TimerPassed($"key_{k.ID}_initial") && ValidShortcutInputs.Contains(k.ID))
					{
						SetTimer($"key_{k.ID}", 50);
						DestroyTimer($"key_{k.ID}_initial");
						Valid = true;
					}
					else if (!TimerExists($"key_{k.ID}") && !TimerExists($"key_{k.ID}_initial"))
					{
						if (Input.Trigger(k.MainKey))
						{
							SetTimer($"key_{k.ID}_initial", 300);
							Valid = true;
						}
					}
					else
					{
						if (!ValidShortcutInputs.Contains(k.ID)) ValidShortcutInputs.Add(k.ID);
					}
				}
				else
				{
					if (TimerExists($"key_{k.ID}")) DestroyTimer($"key_{k.ID}");
					if (TimerExists($"key_{k.ID}_initial")) DestroyTimer($"key_{k.ID}_initial");
					if (ValidShortcutInputs.Contains(k.ID)) ValidShortcutInputs.Remove(k.ID);
				}
				if (!Valid) continue;

				// Modifiers
				if (Input.Press(Keycode.SHIFT) && !k.Modifiers.Contains(Keycode.SHIFT)) Valid = false;
				else if (Input.Press(Keycode.CTRL) && !k.Modifiers.Contains(Keycode.CTRL)) Valid = false;
				else if (Input.Press(Keycode.ALT) && !k.Modifiers.Contains(Keycode.ALT)) Valid = false;
				else if (!k.Modifiers.TrueForAll(m => Input.Press(m))) Valid = false;

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
					// Remove any other key triggers for this iteration
					Window.UI.ResetShortcutTimers(this);
					s.Event(new BaseEventArgs());
				}
			}
		}

		// A shortcut may have disposed this widget, quit if so
		if (Disposed) return;

		// If this widget needs a redraw, perform the redraw
		if (!Drawn) Draw();
		// Update child widgets
		for (int i = 0; i < Widgets.Count; i++)
		{
			if (Widgets[i].Disposed)
			{
				Widgets.RemoveAt(i);
				i--;
				continue;
			}
			Widgets[i].Update();
		}
	}

	public void ResetShortcutTimers(IContainer Exception)
	{
		if (this != Exception)
		{
			foreach (Shortcut s in Shortcuts)
			{
				if (s.GlobalShortcut) continue;
				if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
				if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
				if (ValidShortcutInputs.Contains(s.Key.ID)) ValidShortcutInputs.Remove(s.Key.ID);
			}
		}
		for (int i = 0; i < Widgets.Count; i++)
		{
			Widgets[i].ResetShortcutTimers(Exception);
		}
	}

	private void UpdateOutlines()
	{
		if (ShowWidgetOutlines)
		{
			if (!Sprites.ContainsKey("_a_"))
			{
				Sprites["_a_"] = new Sprite(Viewport);
				Sprites["_b_"] = new Sprite(Viewport);
				Sprites["_c_"] = new Sprite(Viewport);
				Sprites["_d_"] = new Sprite(Viewport);
				Sprites["_e_"] = new Sprite(Viewport);
			}
			Sprites["_a_"].Bitmap?.Dispose();
			Sprites["_a_"].Bitmap = new SolidBitmap(Size.Width, 1, Color.BLACK);
			Sprites["_b_"].Bitmap?.Dispose();
			Sprites["_b_"].Bitmap = new SolidBitmap(Size.Width, 1, Color.BLACK);
			Sprites["_b_"].Y = Size.Height - 1;
			Sprites["_c_"].Bitmap?.Dispose();
			Sprites["_c_"].Bitmap = new SolidBitmap(1, Size.Height, Color.BLACK);
			Sprites["_d_"].Bitmap?.Dispose();
			Sprites["_d_"].Bitmap = new SolidBitmap(1, Size.Height, Color.BLACK);
			Sprites["_d_"].X = Size.Width - 1;
			Sprites["_e_"].Bitmap?.Dispose();
			string txt = GetType().ToString().Split('.').Last();
			Font f = FontCache.GetOrCreate("Arial", 10);
			Size s = f.TextSize(txt);
			Sprites["_e_"].Bitmap = new Bitmap(s);
			Sprites["_e_"].Bitmap.Font = FontCache.GetOrCreate("Arial", 10);
			Sprites["_e_"].X = Viewport.Width / 2 - s.Width / 2;
			Sprites["_e_"].Y = Viewport.Height / 2 - s.Height / 2;
			Sprites["_e_"].Bitmap.Unlock();
			Sprites["_e_"].Bitmap.DrawText(txt, Color.BLACK);
			Sprites["_e_"].Bitmap.Lock();
		}
		else if (Sprites.ContainsKey("_a_"))
		{
			Sprites["_a_"].Dispose();
			Sprites["_b_"].Dispose();
			Sprites["_c_"].Dispose();
			Sprites["_d_"].Dispose();
			Sprites["_e_"].Dispose();
			Sprites.Remove("_a_");
			Sprites.Remove("_b_");
			Sprites.Remove("_c_");
			Sprites.Remove("_d_");
			Sprites.Remove("_e_");
		}
	}

	public virtual void CancelDoubleClick()
	{
		if (TimerExists("double_left_inside")) DestroyTimer("double_left_inside");
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
				OnContextMenuOpening(args);
				if (!args.Value) cont = false;
			}
			if (cont)
			{
				OpenContextMenuOnNextUpdate = true;
				ContextMenuMouseOrigin = new Point(e.X, e.Y);
			}
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
				OnDoubleLeftMouseDownInside?.Invoke(e);
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
		Window.UI.SetSelectedWidget(this);
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
		UpdateOutlines();
	}

	public virtual void ChildBoundsChanged(BaseEventArgs e)
	{
		UpdateAutoScroll();
	}

	public virtual void FetchHelpText(StringEventArgs e)
	{
		e.String = HelpText;
	}
}

public class Shortcut
{
	public Widget Widget;
	public Key Key;
	public BaseEvent Event;
	public BoolEvent Condition;
	public bool GlobalShortcut = false;
	public bool PendingRemoval = false;

	/// <summary>
	/// Creates a new Shortcut object.
	/// </summary>
	/// <param name="Widget">The widget that needs to be visible and available for global shortcuts to work.</param>
	/// <param name="Key">The actual key combination required to activate this shortcut.</param>
	/// <param name="Event">The event to trigger when the key is pressed.</param>
	/// <param name="GlobalShortcut">Whether the given widget needs to be active, or if this is a global shortcut.</param>
	public Shortcut(Widget Widget, Key Key, BaseEvent Event, bool GlobalShortcut = false, BoolEvent Condition = null)
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
		Identifier = identifier;
		StartTime = starttime;
		Timespan = timespan;
	}
}
