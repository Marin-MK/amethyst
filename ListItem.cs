using System;
using System.Collections.Generic;
using System.Text;

namespace amethyst
{
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
            this.Object = Name;
        }

        public ListItem(object Object)
        {
            this.Object = Object;
        }

        public override string ToString()
        {
            return Name ?? (Object is null ? "" : Object.ToString());
        }
    }
}
