using System;
using System.Collections.Generic;
using odl;

namespace amethyst;

public interface IContainer
{
    Viewport Viewport { get; }
    Point Position { get; }
    Size Size { get; }
    List<Widget> Widgets { get; }
    Point AdjustedPosition { get; }
    int ZIndex { get; }

    int ScrolledX { get; set; }
    int ScrolledY { get; set; }
    Point ScrolledPosition { get; }
    ScrollBar HScrollBar { get; }
    ScrollBar VScrollBar { get; }

    IContainer Parent { get; }
    int WindowLayer { get; set; }

    void Add(Widget w);
    Widget Remove(Widget w);
}
