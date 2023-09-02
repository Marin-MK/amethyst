using odl;
using System;

namespace amethyst.Windows;

public class ProgressBar : Widget
{
    public float Progress { get; protected set; }

    public ProgressBar(IContainer Parent) : base(Parent)
    {
        Sprites["outline"] = new Sprite(Viewport);
        Sprites["filler"] = new Sprite(Viewport, new SolidBitmap(1, 1, new Color(6, 176, 37)));
        Sprites["filler"].X = Sprites["filler"].Y = 1;
        Sprites["filler"].Visible = false;
        RedrawOutline();
    }

    public void SetProgress(float progress)
    {
        progress = Math.Clamp(progress, 0, 1);
        if (Progress != progress)
        {
            Progress = progress;
            RedrawFiller();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RedrawOutline();
        RedrawFiller();
    }

    private void RedrawOutline()
    {
        Sprites["outline"].Bitmap?.Dispose();
        Sprites["outline"].Bitmap = new Bitmap(Size);
        Sprites["outline"].Bitmap.Unlock();
        Sprites["outline"].Bitmap.DrawRect(Size, new Color(188, 188, 188));
        Sprites["outline"].Bitmap.Lock();
    }

    private void RedrawFiller()
    {
        if (Disposed) return;
        SolidBitmap bmp = (SolidBitmap)Sprites["filler"].Bitmap;
        int fillerWidth = (int)Math.Round(Progress * (Size.Width - 2));
        bmp.SetSize(fillerWidth, Size.Height - 2);
        Sprites["filler"].Visible = true;
    }
}
