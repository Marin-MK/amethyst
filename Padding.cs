namespace amethyst;

public class Padding
{
    public int Left { get; protected set; }
    public int Up { get; protected set; }
    public int Right { get; protected set; }
    public int Down { get; protected set; }

    public Padding() : this(0, 0, 0, 0) { }
    public Padding(int all) : this(all, all, all, all) { }
    public Padding(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical) { }
    public Padding(int left, int up, int right, int down)
    {
        this.Left = left;
        this.Up = up;
        this.Right = right;
        this.Down = down;
    }
}
