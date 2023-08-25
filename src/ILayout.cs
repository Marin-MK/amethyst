namespace amethyst.src;

public interface ILayout
{
    bool NeedUpdate { get; set; }

    void UpdateLayout();
}
