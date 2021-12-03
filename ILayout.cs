namespace amethyst;

public interface ILayout
{
    bool NeedUpdate { get; set; }

    void UpdateLayout();
}
