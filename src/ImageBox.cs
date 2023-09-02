using System;
using odl;

namespace amethyst;

public class ImageBox : Widget
{
    Sprite Sprite;
    public Bitmap Bitmap => Sprite.Bitmap;
    public int X => Sprite.X;
    public int Y => Sprite.Y;
    public int Z => Sprite.Z;
    public byte Opacity => Sprite.Opacity;
    public int Angle => Sprite.Angle;
    public bool MirrorX => Sprite.MirrorX;
    public bool MirrorY => Sprite.MirrorY;
    public int OX => Sprite.OX;
    public int OY => Sprite.OY;
    public bool DestroyBitmap => Sprite.DestroyBitmap;
    public double ZoomX => Sprite.ZoomX;
    public double ZoomY => Sprite.ZoomY;
    public Color Color => Sprite.Color;
    public Rect SrcRect => Sprite.SrcRect;
    public FillMode FillMode { get; protected set; }

    public ImageBox(IContainer Parent) : base(Parent)
    {
        Sprites["sprite"] = Sprite = new Sprite(Viewport);
        OnSizeChanged += _ => UpdateSize();
    }

    void UpdateSize()
    {
        Sprite.MultiplePositions.Clear();
        if (Bitmap == null) SetSize(1, 1);
        else
        {
            switch (FillMode)
            {
                case FillMode.None:
                    SetSize(X + (int)Math.Round(SrcRect.Width * ZoomX), Y + (int)Math.Round(SrcRect.Height * ZoomY));
                    break;
                case FillMode.Center:
                    SetSize((int)Math.Round(SrcRect.Width * ZoomX), (int)Math.Round(SrcRect.Height * ZoomY));
                    SetPosition((int)Math.Round((double)Parent.Size.Width / 2 - Size.Width / 2), (int)Math.Round((double)Parent.Size.Height / 2 - Size.Height / 2));
                    break;
                case FillMode.Fill:
                    Sprite.ZoomX = (double)Parent.Size.Width / SrcRect.Width;
                    Sprite.ZoomY = (double)Parent.Size.Height / SrcRect.Height;
                    SetSize(X + (int)Math.Round(SrcRect.Width * ZoomX), Y + (int)Math.Round(SrcRect.Height * ZoomY));
                    break;
                case FillMode.FillMaintainAspect:
                    double fx1 = (double)Parent.Size.Width / SrcRect.Width;
                    double fy1 = (double)Parent.Size.Height / SrcRect.Height;
                    double f1 = Math.Max(fx1, fy1);
                    Sprite.ZoomX = Sprite.ZoomY = f1;
                    SetSize(Parent.Size);
                    break;
                case FillMode.FillMaintainAspectAndCenter:
                    double fx2 = (double)Parent.Size.Width / SrcRect.Width;
                    double fy2 = (double)Parent.Size.Height / SrcRect.Height;
                    double f2 = Math.Max(fx2, fy2);
                    Sprite.ZoomX = Sprite.ZoomY = f2;
                    SetSize(Parent.Size);
                    Sprite.X = (int)Math.Round(Parent.Size.Width / 2 - ZoomX * SrcRect.Width / 2);
                    Sprite.Y = (int)Math.Round(Parent.Size.Height / 2 - ZoomY * SrcRect.Height / 2);
                    break;
                case FillMode.Tile:
                    int wcount1 = (int)Math.Ceiling((double)Parent.Size.Width / SrcRect.Width);
                    int hcount1 = (int)Math.Ceiling((double)Parent.Size.Height / SrcRect.Height);
                    if (X > 0)
                    {
                        int xc = (int)Math.Ceiling((double)X / SrcRect.Width);
                        wcount1 += xc;
                        Sprite.X -= xc * SrcRect.Width;
                    }
                    if (wcount1 * SrcRect.Width + X < Parent.Size.Width)
                    {
                        int dx = Parent.Size.Width - (wcount1 * SrcRect.Width + X);
                        wcount1 += (int)Math.Ceiling((double)dx / SrcRect.Width);
                    }
                    if (Y > 0)
                    {
                        int yc = (int)Math.Ceiling((double)Y / SrcRect.Height);
                        hcount1 += yc;
                        Sprite.Y -= yc * SrcRect.Height;
                    }
                    if (hcount1 * SrcRect.Height + Y < Parent.Size.Height)
                    {
                        int dy = Parent.Size.Height - (hcount1 * SrcRect.Height + Y);
                        hcount1 += (int)Math.Ceiling((double)dy / SrcRect.Height);
                    }
                    for (int y = 0; y < hcount1; y++)
                    {
                        for (int x = 0; x < wcount1; x++)
                        {
                            Sprite.MultiplePositions.Add(new Point(X + x * SrcRect.Width, Y + y * SrcRect.Height));
                        }
                    }
                    SetSize(Parent.Size);
                    break;
                case FillMode.TileAndCenter:
                    int wcount2 = (int)Math.Ceiling((double)Parent.Size.Width / SrcRect.Width);
                    int hcount2 = (int)Math.Ceiling((double)Parent.Size.Height / SrcRect.Height);
                    Sprite.X = (int)Math.Round(Parent.Size.Width / 2 - ZoomX * SrcRect.Width / 2);
                    Sprite.Y = (int)Math.Round(Parent.Size.Height / 2 - ZoomY * SrcRect.Height / 2);
                    if (X > 0)
                    {
                        int xc = (int)Math.Ceiling((double)X / SrcRect.Width);
                        wcount2 += xc;
                        Sprite.X -= xc * SrcRect.Width;
                    }
                    if (wcount2 * SrcRect.Width + X < Parent.Size.Width)
                    {
                        int dx = Parent.Size.Width - (wcount2 * SrcRect.Width + X);
                        wcount2 += (int)Math.Ceiling((double)dx / SrcRect.Width);
                    }
                    if (Y > 0)
                    {
                        int yc = (int)Math.Ceiling((double)Y / SrcRect.Height);
                        hcount2 += yc;
                        Sprite.Y -= yc * SrcRect.Height;
                    }
                    if (hcount2 * SrcRect.Height + Y < Parent.Size.Height)
                    {
                        int dy = Parent.Size.Height - (hcount2 * SrcRect.Height + Y);
                        hcount2 += (int)Math.Ceiling((double)dy / SrcRect.Height);
                    }
                    for (int y = 0; y < hcount2; y++)
                    {
                        for (int x = 0; x < wcount2; x++)
                        {
                            Sprite.MultiplePositions.Add(new Point(X + x * SrcRect.Width, Y + y * SrcRect.Height));
                        }
                    }
                    SetSize(Parent.Size);
                    break;
            }
        }
    }

    public void SetBitmap(int Width, int Height)
    {
        SetBitmap(new Bitmap(Width, Height));
    }

    public void SetBitmap(string Filename)
    {
        SetBitmap(new Bitmap(Filename));
    }

    public void SetBitmap(Bitmap Bitmap)
    {
        Sprite.Bitmap = Bitmap;
        UpdateSize();
    }

    public void DisposeBitmap()
    {
        Sprite.Bitmap?.Dispose();
        Sprite.Bitmap = null;
    }

    public void ClearBitmap()
    {
        Sprite.Bitmap = null;
    }

    public void SetX(int X)
    {
        Sprite.X = X;
        UpdateSize();
    }

    public void SetY(int Y)
    {
        Sprite.Y = Y;
        UpdateSize();
    }

    public void SetZ(int Z)
    {
        Sprite.Z = Z;
    }

    public new void SetOpacity(byte Opacity)
    {
        Sprite.Opacity = Opacity;
    }

    public void SetAngle(int Angle)
    {
        Sprite.Angle = Angle;
    }

    public void SetMirrorX(bool MirrorX)
    {
        Sprite.MirrorX = MirrorX;
    }

    public void SetMirrorY(bool MirrorY)
    {
        Sprite.MirrorY = MirrorY;
    }

    public void SetOX(int OX)
    {
        Sprite.OX = OX;
    }

    public void SetOY(int OY)
    {
        Sprite.OY = OY;
    }

    public void SetDestroyBitmap(bool DestroyBitmap)
    {
        Sprite.DestroyBitmap = DestroyBitmap;
    }

    public void SetZoomX(double ZoomX)
    {
        Sprite.ZoomX = ZoomX;
        UpdateSize();
    }

    public void SetZoomY(double ZoomY)
    {
        Sprite.ZoomY = ZoomY;
        UpdateSize();
    }

    public void SetColor(Color Color)
    {
        Sprite.Color = Color;
    }

    public void SetSrcRect(Rect SrcRect)
    {
        Sprite.SrcRect = SrcRect;
        UpdateSize();
    }

    public void SetFillMode(FillMode FillMode)
    {
        this.FillMode = FillMode;
        UpdateSize();
    }
}

public enum FillMode
{
    None,
    Center,
    Fill,
    FillMaintainAspect,
    FillMaintainAspectAndCenter,
    Tile,
    TileAndCenter
}