using System;

namespace amethyst;

public class ListItem
{
    public string Name;
    public object Object;

    public ListItem(string Name, object Object)
    {
        this.Name = Name;
        this.Object = Object;
    }

    public ListItem(string Name)
    {
        this.Name = Name;
        Object = Name;
    }

    public ListItem(object Object)
    {
        this.Object = Object;
    }

    public override string ToString()
    {
        return Name ?? (Object is null ? "" : Object.ToString());
    }

    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        if (obj is ListItem)
        {
            ListItem i = (ListItem)obj;
            return Name == i.Name && Object.Equals(i.Object);
        }
        return false;
    }
}
