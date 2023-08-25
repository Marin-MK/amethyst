namespace amethyst.src;

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
        Left = left;
        Up = up;
        Right = right;
        Down = down;
    }

    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        if (obj is Padding)
        {
            Padding p = (Padding)obj;
            return Left == p.Left &&
                   Up == p.Up &&
                   Right == p.Right &&
                   Down == p.Down;
        }
        return false;
    }
}
