namespace amethyst;

public class Margins
{
    public int Left { get; protected set; }
    public int Up { get; protected set; }
    public int Right { get; protected set; }
    public int Down { get; protected set; }

    public Margins() : this(0, 0, 0, 0) { }
    public Margins(int all) : this(all, all, all, all) { }
    public Margins(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical) { }
    public Margins(int left, int up, int right, int down)
    {
        this.Left = left;
        this.Up = up;
        this.Right = right;
        this.Down = down;
    }
}
