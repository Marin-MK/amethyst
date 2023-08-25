using System.Collections.Generic;
using odl;

namespace amethyst.src;

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

    int LeftCutOff { get; }
    int TopCutOff { get; }

    IContainer Parent { get; }
    int WindowLayer { get; set; }

    MouseManager Mouse { get; }

    MouseEvent OnMouseMoving { get; set; }
    MouseEvent OnHoverChanged { get; set; }
    MouseEvent OnMouseDown { get; set; }
    MouseEvent OnMouseUp { get; set; }
    MouseEvent OnMousePress { get; set; }
    MouseEvent OnLeftMouseDown { get; set; }
    MouseEvent OnLeftMouseDownInside { get; set; }
    MouseEvent OnLeftMouseUp { get; set; }
    MouseEvent OnLeftMousePress { get; set; }
    MouseEvent OnRightMouseDown { get; set; }
    MouseEvent OnRightMouseDownInside { get; set; }
    MouseEvent OnRightMouseUp { get; set; }
    MouseEvent OnRightMousePress { get; set; }
    MouseEvent OnMiddleMouseDown { get; set; }
    MouseEvent OnMiddleMouseDownInside { get; set; }
    MouseEvent OnMiddleMouseUp { get; set; }
    MouseEvent OnMiddleMousePress { get; set; }
    MouseEvent OnMouseWheel { get; set; }
    MouseEvent OnDoubleLeftMouseDownInside { get; set; }

    bool EvaluatedLastMouseEvent { get; set; }

    void Add(Widget w, int index);
    Widget Remove(Widget w);
}
