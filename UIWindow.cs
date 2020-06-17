using System;
using System.Collections.Generic;
using System.Text;
using odl;

namespace amethyst
{
    public class UIWindow : Window
    {
        /// <summary>
        /// The main UI manager object.
        /// When used as widget parent, will force widget to be the size of the UI Manager.
        /// Use UI.Container instead if not desired.
        /// </summary>
        public UIManager UI { get; protected set; }
        /// <summary>
        /// The active Widget in the window. Used for higher priority popup windows that overlay the old active widget.
        /// </summary>
        public IContainer ActiveWidget;
        /// <summary>
        /// The list of former active widgets. Used to go back to an older active widget when the currently active widget closes.
        /// </summary>
        public List<IContainer> Widgets = new List<IContainer>();

        public UIWindow(bool Borderless = false) : base()
        {
            Initialize(true, false, Borderless);
            InitializeUI();
        }

        public void InitializeUI()
        {
            InitializeUI(Color.BLACK);
        }
        public void InitializeUI(Color Color)
        {
            InitializeUI(Color.Red, Color.Green, Color.Blue, Color.Alpha);
        }
        public void InitializeUI(byte R, byte G, byte B, byte A = 255)
        {
            this.UI = new UIManager(this);
            this.UI.SizeChanged(new BaseEventArgs());
            this.UI.SetBackgroundColor(R, G, B, A);
            this.UI.Container = new Container(UI);
            this.UI.Container.SetSize(Width, Height);
        }

        /// <summary>
        /// Sets the main active widget.
        /// </summary>
        /// <param name="Widget">The widget to set as the main widget.</param>
        public void SetActiveWidget(IContainer Widget)
        {
            this.ActiveWidget = Widget;
            if (!Widgets.Contains(Widget)) Widgets.Add(Widget);
            if (Graphics.LastMouseEvent is MouseEventArgs) Graphics.LastMouseEvent.Handled = true;
        }

        /// <summary>
        /// Sets the opacity of the main window overlay.
        /// </summary>
        public void SetOverlayOpacity(byte Opacity)
        {
            TopSprite.Opacity = Opacity;
        }

        /// <summary>
        /// Sets the Z index of the main window overlay's viewport.
        /// </summary>
        public void SetOverlayZIndex(int Z)
        {
            TopViewport.Z = Z;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            UI.MouseDown(e);
        }

        public override void MousePress(MouseEventArgs e)
        {
            base.MousePress(e);
            UI.MousePress(e);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            UI.MouseUp(e);
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            UI.MouseMoving(e);
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            base.MouseWheel(e);
            UI.MouseWheel(e);
        }

        public override void TextInput(TextEventArgs e)
        {
            base.TextInput(e);
            UI.TextInput(e);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            UI.SizeChanged(e);
        }

        /// <summary>
        /// Updates the UIManager, and subsequently all widgets.
        /// </summary>
        public override void Tick(BaseEventArgs e)
        {
            base.Tick(e);
            this.UI.Update();
        }

        public override void SetBackgroundColor(Color c)
        {
            base.SetBackgroundColor(c);
        }
    }
}
