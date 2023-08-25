using System.Collections.Generic;

namespace amethyst;

public interface IMenuItem { }

public class MenuItem : IMenuItem
{
    public string Text;
    public List<IMenuItem> Items;
    public string HelpText;
    public string Shortcut;
    public odl.BaseEvent OnClicked;
    public odl.BoolEvent IsClickable;
    public bool HasChildren => Items.Count > 0;
    public bool IsCheckable = false;
    public odl.BoolEvent IsChecked;

    public MenuItem(string Text)
    {
        this.Text = Text;
        Items = new List<IMenuItem>();
    }

    public MenuItem(string Text, odl.BaseEvent OnClicked)
    {
        this.Text = Text;
        Items = new List<IMenuItem>();
        this.OnClicked = OnClicked;
    }
}

public class MenuSeparator : IMenuItem { }
