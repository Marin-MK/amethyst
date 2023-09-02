using System;
using System.Collections.Generic;
using System.Linq;
using odl;

namespace amethyst;

public class Grid : Widget, ILayout
{
    public bool NeedUpdate { get; set; } = true;
    public List<GridSize> Rows = new List<GridSize>();
    public List<GridSize> Columns = new List<GridSize>();
    public Size[] Sizes;
    public Point[] Positions;

    private bool RedrawContainers = true;

    public Grid(IContainer Parent) : base(Parent)
    {
        SetDocked(true);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        UpdateContainers();
        UpdateLayout();
    }

    public override void Update()
    {
        if (RedrawContainers)
        {
            UpdateContainers();
            RedrawContainers = false;
        }
        if (NeedUpdate)
        {
            UpdateLayout();
            NeedUpdate = false;
        }
        base.Update();
    }

    public new void UpdateLayout()
    {
        if (Sizes == null || Sizes.Length == 0) UpdateContainers();
        for (int i = 0; i < Widgets.Count; i++)
        {
            Widget w = Widgets[i];
            int width = 0;
            int height = 0;
            if (w.GridRowStart >= Rows.Count || w.GridRowEnd >= Rows.Count)
            {
                throw new Exception("Widget GridRow value exceeds amount of defined rows");
            }
            if (w.GridColumnStart >= Columns.Count || w.GridColumnEnd >= Columns.Count)
            {
                throw new Exception("Widget GridColumn value exceeds amount of defined columns");
            }
            for (int j = w.GridRowStart; j <= w.GridRowEnd; j++)
            {
                height += Sizes[j * Columns.Count + w.GridColumnStart].Height;
            }
            for (int j = w.GridColumnStart; j <= w.GridColumnEnd; j++)
            {
                width += Sizes[w.GridRowStart * Columns.Count + j].Width;
            }
            Point p = Positions[w.GridRowStart * Columns.Count + w.GridColumnStart];
            int x = p.X;
            int y = p.Y;
            x += w.Margins.Left;
            width -= w.Margins.Left + w.Margins.Right;
            y += w.Margins.Up;
            height -= w.Margins.Up + w.Margins.Down;
            w.SetPosition(x, y);
            w.SetSize(width, height);
        }
    }

    public void UpdateContainers()
    {
        if (Sizes != null) Sizes = null;
        if (Positions != null) Positions = null;
        if (Rows.Count == 0) Rows.Add(new GridSize(1, Unit.Relative));
        if (Columns.Count == 0) Columns.Add(new GridSize(1, Unit.Relative));

        Sizes = new Size[Rows.Count * Columns.Count];
        Positions = new Point[Rows.Count * Columns.Count];

        int WidthToSpread = Size.Width;
        double TotalWidthPoints = 0;
        int HeightToSpread = Size.Height;
        double TotalHeightPoints = 0;
        for (int i = 0; i < Rows.Count; i++)
        {
            GridSize s = Rows[i];
            if (s.Unit == Unit.Pixels)
            {
                HeightToSpread -= (int)s.Value;
            }
            else if (s.Unit == Unit.Relative)
            {
                TotalHeightPoints += s.Value;
            }
        }
        for (int i = 0; i < Columns.Count; i++)
        {
            GridSize s = Columns[i];
            if (s.Unit == Unit.Pixels)
            {
                WidthToSpread -= (int)s.Value;
            }
            else if (s.Unit == Unit.Relative)
            {
                TotalWidthPoints += s.Value;
            }
        }

        double WidthPerPoint = WidthToSpread / TotalWidthPoints;
        double HeightPerPoint = HeightToSpread / TotalHeightPoints;

        int x = 0;
        int y = 0;
        for (int i = 0; i < Rows.Count; i++)
        {
            GridSize row = Rows[i];
            int height = 0;
            if (row.Unit == Unit.Pixels) height = (int)row.Value;
            else if (row.Unit == Unit.Relative) height = (int)Math.Round(HeightPerPoint * row.Value);

            for (int j = 0; j < Columns.Count; j++)
            {
                GridSize column = Columns[j];
                int width = 0;
                if (column.Unit == Unit.Pixels) width = (int)column.Value;
                else if (column.Unit == Unit.Relative) width = (int)Math.Round(WidthPerPoint * column.Value);
                Sizes[i * Columns.Count + j] = new Size(width, height);
                Positions[i * Columns.Count + j] = new Point(x, y);
                x += width;
            }

            x = 0;
            y += height;
        }

        int maxw = Positions.Last().X + Sizes.Last().Width;
        int maxh = Positions.Last().Y + Sizes.Last().Height;
        SetSize(maxw, maxh);

        NeedUpdate = true;
    }

    public void SetRows(params GridSize[] Rows)
    {
        this.Rows = Rows.ToList();
        RedrawContainers = true;
    }

    public void SetColumns(params GridSize[] Columns)
    {
        this.Columns = Columns.ToList();
        RedrawContainers = true;
    }

    public override void Add(Widget w, int Index = -1)
    {
        base.Add(w, Index);
        NeedUpdate = true;
    }
}
