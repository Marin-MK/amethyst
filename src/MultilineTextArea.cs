using odl;

using System;
using System.Collections.Generic;
using System.Linq;

namespace amethyst;

public class MultilineTextArea : Widget
{
    static Dictionary<Font, Dictionary<string, int>> FontStringWidths = new Dictionary<Font, Dictionary<string, int>>();

    public string Text { get; protected set; } = "";
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.WHITE;
    public Color TextColorSelected { get; protected set; } = Color.BLACK;
    public Color SelectionBackgroundColor { get; protected set; } = new Color(128, 128, 255);
    public DrawOptions DrawOptions { get; protected set; } = DrawOptions.None;
    public int LineHeight => _UserSetLineHeight == -1 ? _FontLineHeight : _UserSetLineHeight;
    public int LineMargins { get; protected set; } = 4;
    public bool OverlaySelectedText { get; protected set; } = true;
    public bool LineWrapping { get; protected set; } = true;
    public bool ReadOnly { get; protected set; } = false;
    public bool Interactable { get; protected set; } = true;

    public BaseEvent OnTextChanged;
    public BoolEvent OnCopy;
    public BoolEvent OnPaste;

    public List<Line> Lines;
    protected List<Sprite> LineSprites = new List<Sprite>();
    protected List<Sprite> SelBoxSprites = new List<Sprite>();
    protected List<TextAreaState> UndoableStates = new List<TextAreaState>();
    protected List<TextAreaState> RedoableStates = new List<TextAreaState>();
    public CaretIndex Caret;
    protected CaretIndex SelectionStart;
    protected CaretIndex SelectionEnd;
    protected CaretIndex SelectionLeft => SelectionStart.Index > SelectionEnd.Index ? SelectionEnd : SelectionStart;
    protected CaretIndex SelectionRight => SelectionStart.Index > SelectionEnd.Index ? SelectionStart : SelectionEnd;
    protected bool RequireRecalculation = false;
    protected bool RequireRedrawText = false;
    protected bool RequireCaretRepositioning = false;
    protected bool RequireUpdateUpDownAnchor = false;
    protected bool EnteringText = false;
    protected bool HasSelection => SelectionStart != null;
    protected bool SnapToWords = false;
    protected bool SnapToLines = false;
    protected bool StartedInParent = false;
    protected int? MinSelIndex;
    protected int? MaxSelIndex;
    protected int? LineSnapIndex;
    protected int LineWidthLimit => Size.Width;
    protected int OldTopLineIndex = 0;
    protected int OldBottomLineIndex = 0;
    protected int TopLineIndex => Parent.ScrolledY / (LineHeight + LineMargins);
    protected int BottomLineIndex => TopLineIndex + Parent.Size.Height / (LineHeight + LineMargins);
    protected int _FontLineHeight = -1;
    protected int _UserSetLineHeight = -1;
    protected bool HomeSnappedToFirstPrior = false;
    protected int MaxCaretPositionInLine = 0;
    protected int CaretWidth = 1;
    protected bool InsertMode = false;

    public MultilineTextArea(IContainer Parent) : base(Parent)
    {
        Lines = new List<Line>() { new Line(null) { LineIndex = 0, StartIndex = 0 } };
        Caret = new CaretIndex(this) { Index = 0 };
        Sprites["caret"] = new Sprite(Viewport, new SolidBitmap(1, 1, Color.WHITE));
        Sprites["caret"].Z = 2;
        Sprites["caret"].Visible = false;
        OnWidgetSelected += WidgetSelected;
        OnDisposed += _ =>
        {
            Window.UI.SetSelectedWidget(null);
            Input.SetCursor(CursorType.Arrow);
        };
        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.RIGHT), _ => MoveRight(false, false)),
            new Shortcut(this, new Key(Keycode.RIGHT, Keycode.SHIFT), _ => MoveRight(true, false)),
            new Shortcut(this, new Key(Keycode.RIGHT, Keycode.CTRL), _ => MoveRight(false, true)),
            new Shortcut(this, new Key(Keycode.RIGHT, Keycode.SHIFT, Keycode.CTRL), _ => MoveRight(true, true)),
            new Shortcut(this, new Key(Keycode.LEFT), _ => MoveLeft(false, false)),
            new Shortcut(this, new Key(Keycode.LEFT, Keycode.SHIFT), _ => MoveLeft(true, false)),
            new Shortcut(this, new Key(Keycode.LEFT, Keycode.CTRL), _ => MoveLeft(false, true)),
            new Shortcut(this, new Key(Keycode.LEFT, Keycode.SHIFT, Keycode.CTRL), _ => MoveLeft(true, true)),
            new Shortcut(this, new Key(Keycode.UP), _ => MoveUp(false, false)),
            new Shortcut(this, new Key(Keycode.UP, Keycode.SHIFT), _ => MoveUp(true, false)),
            new Shortcut(this, new Key(Keycode.UP, Keycode.CTRL), _ => MoveUp(false, true)),
            new Shortcut(this, new Key(Keycode.UP, Keycode.SHIFT, Keycode.CTRL), _ => MoveUp(true, true)),
            new Shortcut(this, new Key(Keycode.DOWN), _ => MoveDown(false, false)),
            new Shortcut(this, new Key(Keycode.DOWN, Keycode.SHIFT), _ => MoveDown(true, false)),
            new Shortcut(this, new Key(Keycode.DOWN, Keycode.CTRL), _ => MoveDown(false, true)),
            new Shortcut(this, new Key(Keycode.DOWN, Keycode.SHIFT, Keycode.CTRL), _ => MoveDown(true, true)),
            new Shortcut(this, new Key(Keycode.HOME), _ => MoveHome(false)),
            new Shortcut(this, new Key(Keycode.HOME, Keycode.SHIFT), _ => MoveHome(true)),
            new Shortcut(this, new Key(Keycode.END), _ => MoveEnd(false)),
            new Shortcut(this, new Key(Keycode.END, Keycode.SHIFT), _ => MoveEnd(true)),
            new Shortcut(this, new Key(Keycode.PAGEUP), _ => MovePageUp(false)),
            new Shortcut(this, new Key(Keycode.PAGEUP, Keycode.SHIFT), _ => MovePageUp(true)),
            new Shortcut(this, new Key(Keycode.PAGEDOWN), _ => MovePageDown(false)),
            new Shortcut(this, new Key(Keycode.PAGEDOWN, Keycode.SHIFT), _ => MovePageDown(true)),
            new Shortcut(this, new Key(Keycode.A, Keycode.CTRL), _ => SelectAll()),
            new Shortcut(this, new Key(Keycode.X, Keycode.CTRL), _ => CutSelection()),
            new Shortcut(this, new Key(Keycode.C, Keycode.CTRL), _ => CopySelection()),
            new Shortcut(this, new Key(Keycode.V, Keycode.CTRL), _ => Paste()),
            new Shortcut(this, new Key(Keycode.Z, Keycode.CTRL), _ => Undo()),
            new Shortcut(this, new Key(Keycode.Y, Keycode.CTRL), _ => Redo()),
            new Shortcut(this, new Key(Keycode.ESCAPE), _ => CancelSelection(), false, e => e.Value = HasSelection),
            new Shortcut(this, new Key(Keycode.INSERT), _ => ToggleInsertMode())
        });
        AddUndoState();

        Parent.OnLeftMouseDown += e => StartedInParent = MouseInsideTextArea(e);
        Parent.OnLeftMouseUp += _ => StartedInParent = false;
    }

    public virtual void SetText(string Text, bool SetCaretToEnd = false, bool ClearUndoStates = true)
    {
        if (this.Text != Text)
        {
            if (HasSelection) CancelSelection();
            Text = Text.Replace("\r", "");
            Caret.AtEndOfLine = false;
            Caret.Index = 0;
            this.Text = Text;
            if (HasSelection) CancelSelection();
            if (ClearUndoStates)
            {
                UndoableStates.Clear();
                AddUndoState();
            }
            if (SetCaretToEnd) Caret.Index = Text.Length;
            RecalculateLines();
        }
    }

    public virtual void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            _FontLineHeight = Font.Size + 5;
            ((SolidBitmap)Sprites["caret"].Bitmap).SetSize(CaretWidth, LineHeight);
            RecalculateLines();
        }
    }

    public virtual void SetDrawOptions(DrawOptions DrawOptions)
    {
        if (this.DrawOptions != DrawOptions)
        {
            this.DrawOptions = DrawOptions;
            RedrawText();
        }
    }

    public virtual void SetTextColor(Color TextColor)
    {
        if (this.TextColor != TextColor)
        {
            this.TextColor = TextColor;
            RedrawText();
        }
    }

    public virtual void SetLineHeight(int LineHeight)
    {
        if (this.LineHeight != LineHeight || _UserSetLineHeight == -1)
        {
            int OldHeight = this.LineHeight;
            _UserSetLineHeight = LineHeight;
            ((SolidBitmap)Sprites["caret"].Bitmap).SetSize(CaretWidth, this.LineHeight);
            if (OldHeight != this.LineHeight) RedrawText();
        }
    }

    public virtual void SetSelectionBackgroundColor(Color SelectionBackgroundColor)
    {
        if (this.SelectionBackgroundColor != SelectionBackgroundColor)
        {
            this.SelectionBackgroundColor = SelectionBackgroundColor;
            if (HasSelection) RedrawText();
        }
    }

    public virtual void SetOverlaySelectedText(bool OverlaySelectedText)
    {
        if (this.OverlaySelectedText != OverlaySelectedText)
        {
            this.OverlaySelectedText = OverlaySelectedText;
            if (HasSelection) RedrawText();
        }
    }

    public virtual void SetTextColorSelected(Color TextColorSelected)
    {
        if (this.TextColorSelected != TextColorSelected)
        {
            this.TextColorSelected = TextColorSelected;
            if (HasSelection) RedrawText();
        }
    }

    public virtual void SetLineWrapping(bool LineWrapping)
    {
        if (this.LineWrapping != LineWrapping)
        {
            this.LineWrapping = LineWrapping;
            RecalculateLines();
        }
    }

    public void SetReadOnly(bool ReadOnly)
    {
        if (this.ReadOnly != ReadOnly)
        {
            this.ReadOnly = ReadOnly;
        }
    }

    public void SetInteractable(bool Interactable)
    {
        if (this.Interactable != Interactable)
        {
            this.Interactable = Interactable;
            if (!this.Interactable)
            {
                if (Mouse.Inside) Input.SetCursor(CursorType.Arrow);
                if (SelectedWidget) Window.UI.SetSelectedWidget(null);
            }
        }
    }

    public virtual void SetLineMargins(int LineMargins)
    {
        if (this.LineMargins != LineMargins)
        {
            this.LineMargins = LineMargins;
            RecalculateLines();
        }
    }

    public override void Update()
    {
        base.Update();
        OwnUpdate();
    }

    protected virtual void OwnUpdate()
    {
        if (!SelectedWidget)
        {
            if (EnteringText) WidgetDeselected(new BaseEventArgs());
            if (Sprites["caret"].Visible) Sprites["caret"].Visible = false;
        }
        if (Parent.ScrolledY % (LineHeight + LineMargins) != 0)
        {
            Parent.ScrolledY -= Parent.ScrolledY % (LineHeight + LineMargins);
            ((Widget)Parent).UpdateAutoScroll();
        }
        if (OldTopLineIndex != TopLineIndex || OldBottomLineIndex != BottomLineIndex)
        {
            RequireRedrawText = true;
        }
        if (RequireRecalculation) RecalculateLines(true);
        if (RequireRedrawText) RedrawText(true);
        if (RequireUpdateUpDownAnchor) UpdateUpDownAnchor(true);
        OldTopLineIndex = TopLineIndex;
        OldBottomLineIndex = BottomLineIndex;
        if (RequireCaretRepositioning)
        {
            if (Sprites["caret"].X - Parent.ScrolledX >= Parent.Size.Width)
            {
                Parent.ScrolledX += Sprites["caret"].X - Parent.ScrolledX - Parent.Size.Width + 1;
                ((Widget)Parent).UpdateAutoScroll();
            }
            else if (Sprites["caret"].X - Parent.ScrolledX < 0)
            {
                Parent.ScrolledX += Sprites["caret"].X - Parent.ScrolledX - 1;
                ((Widget)Parent).UpdateAutoScroll();
            }
            if (Caret.Line.LineIndex < TopLineIndex) ScrollUp(TopLineIndex - Caret.Line.LineIndex);
            if (Caret.Line.LineIndex >= BottomLineIndex) ScrollDown(Caret.Line.LineIndex - BottomLineIndex + 1);
            RequireCaretRepositioning = false;
        }
        if (TimerPassed("idle") && SelectedWidget)
        {
            Sprites["caret"].Visible = !Sprites["caret"].Visible;
            ResetTimer("idle");
        }
        if (TimerPassed("state"))
        {
            ResetTimer("state");
            // Text changes since last timer passage of "state",
            // so we add the current state
            if (Text != UndoableStates.Last().Text) AddUndoState();
        }
    }

    public virtual void ScrollUpPixels(int px)
    {
        Parent.ScrolledY = Math.Max(Parent.ScrolledY - px, 0);
        ((Widget)Parent).UpdateAutoScroll();
    }

    public virtual void ScrollUp(int Count)
    {
        ScrollUpPixels(Count * (LineHeight + LineMargins));
    }

    public virtual void ScrollDownPixels(int px)
    {
        Parent.ScrolledY += px;
        ((Widget)Parent).UpdateAutoScroll();
    }

    public virtual void ScrollDown(int Count)
    {
        ScrollDownPixels(Count * (LineHeight + LineMargins));
    }

    public override void WidgetSelected(BaseEventArgs e)
    {
        base.WidgetSelected(e);
        if (!Interactable) return;
        EnteringText = true;
        Input.StartTextInput();
        if (!TimerExists("idle")) SetTimer("idle", 400);
        if (!TimerExists("state")) SetTimer("state", 500);
    }

    public override void WidgetDeselected(BaseEventArgs e)
    {
        base.WidgetDeselected(e);
        if (!Interactable) return;
        EnteringText = false;
        Input.StopTextInput();
        if (TimerExists("state")) DestroyTimer("state");
    }

    private static int GetTextWidth(Font Font, string Text)
    {
        if (!FontStringWidths.ContainsKey(Font)) FontStringWidths.Add(Font, new Dictionary<string, int>());
        if (FontStringWidths[Font].ContainsKey(Text)) return FontStringWidths[Font][Text];
        Size s = Font.TextSize(Text);
        FontStringWidths[Font].Add(Text, s.Width);
        return s.Width;
    }

    protected virtual void RecalculateLines(bool Now = false)
    {
        if (!Now)
        {
            RequireRecalculation = true;
            RequireRedrawText = true;
            RequireCaretRepositioning = true;
            return;
        }
        if (Font == null) return;
        int startidx = 0;
        int lastsplittableindex = -1;
        Lines.Clear();
        for (int i = 0; i < Text.Length; i++)
        {
            char c = Text[i];
            string txt = Text.Substring(startidx, i - startidx + 1);
            if (c == '\n')
            {
                Line l = new Line(Font);
                l.LineIndex = Lines.Count;
                l.StartIndex = startidx;
                l.AddText(Text.Substring(startidx, i - startidx + 1));
                Lines.Add(l);
                startidx = i + 1;
                if (i == Text.Length - 1)
                {
                    Lines.Add(new Line(Font)
                    {
                        LineIndex = Lines.Count,
                        StartIndex = startidx
                    });
                }
                lastsplittableindex = -1;
            }
            else if (LineWrapping && GetTextWidth(Font, txt) >= LineWidthLimit)
            {
                int endidx = lastsplittableindex == -1 ? i : lastsplittableindex + 1;
                Line l = new Line(Font);
                l.LineIndex = Lines.Count;
                l.StartIndex = startidx;
                l.AddText(Text.Substring(startidx, endidx - startidx - 1));
                Lines.Add(l);
                startidx = endidx - 1;
                lastsplittableindex = -1;
            }
            else if (c == ' ' || c == '-')
            {
                lastsplittableindex = i + 1;
            }
        }
        if (startidx != Text.Length)
        {
            Line l = new Line(Font);
            l.LineIndex = Lines.Count;
            l.StartIndex = startidx;
            l.AddText(Text.Substring(startidx));
            Lines.Add(l);
        }
        else if (Lines.Count == 0)
        {
            Lines.Add(new Line(Font)
            {
                LineIndex = 0,
                StartIndex = 0
            });
        }
        RequireRecalculation = false;
    }

    protected virtual void UpdateHeight()
    {
        int mc = (Lines.Count - 1) * LineMargins;
        int h = Lines.Count * LineHeight + mc + 3;
        if (h >= Parent.Size.Height) h += (h - Parent.Size.Height) % LineHeight;
        if (h % LineHeight != 0) h += LineHeight + LineMargins - h % (LineHeight + LineMargins);
        if (LineWrapping) SetHeight(h);
        else SetSize(Lines.Max(l => l.LineWidth) + 3, h);
    }

    protected virtual void RedrawText(bool Now = false)
    {
        if (!Now)
        {
            RequireRedrawText = true;
            return;
        }
        LineSprites.ForEach(s => s.Dispose());
        LineSprites.Clear();
        SelBoxSprites.ForEach(s => s.Dispose());
        SelBoxSprites.Clear();
        if (Font == null) return;
        UpdateHeight();
        Lines.ForEach(line =>
        {
            if (line.LineIndex < TopLineIndex || line.LineIndex > BottomLineIndex) return;
            Sprite sprite = new Sprite(Viewport);
            sprite.Y = line.LineIndex * LineHeight + line.LineIndex * LineMargins;
            sprite.Bitmap = new Bitmap(line.LineWidth, LineHeight + 4);
            sprite.Bitmap.Font = Font;
            sprite.Bitmap.Unlock();
            if (HasSelection)
            {
                // Since the font rendering has a lot of internal garbage related to positioning
                // like kerning, just summing the size of the glyphs will make the selected text
                // appear to shift. That's why instead of only drawing that selection, we draw the
                // full text in order to get identical kerning, and then we just draw the selected
                // part of that text. This way the text position will always be the same as the original text.
                // This is only necessary if there is non-selected text before the selected text, as that determines
                // the location of the selected text.
                if (SelectionLeft.Line.LineIndex == line.LineIndex && SelectionRight.Line.LineIndex == line.LineIndex)
                {
                    // Selection starts and ends on this line
                    int x = SelectionLeft.Line.WidthUpTo(SelectionLeft.IndexInLine);
                    int w = SelectionRight.Line.WidthUpTo(SelectionRight.IndexInLine) - x;
                    // Another failsafe for some specific kerning
                    if (x + w > sprite.Bitmap.Width) w = sprite.Bitmap.Width - x;
                    sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), TextColor, DrawOptions);
                    if (OverlaySelectedText)
                    {
                        sprite.Bitmap.FillRect(x, 0, w, sprite.Bitmap.Height, SelectionBackgroundColor);
                        Bitmap bmp = new Bitmap(sprite.Bitmap.Width, sprite.Bitmap.Height);
                        bmp.Font = Font;
                        bmp.Unlock();
                        bmp.DrawText(line.Text.Replace('\n', ' '), TextColorSelected, DrawOptions);
                        bmp.Lock();
                        sprite.Bitmap.Build(x, 0, bmp, new Rect(x, 0, w, bmp.Height));
                        bmp.Dispose();
                    }
                    else
                    {
                        Sprite s = new Sprite(Viewport);
                        s.X = x;
                        s.Y = sprite.Y;
                        s.Z = 1;
                        s.Bitmap = new SolidBitmap(w, sprite.Bitmap.Height, SelectionBackgroundColor);
                        SelBoxSprites.Add(s);
                        Sprites[$"box{line.LineIndex}"] = s;
                    }
                }
                else if (SelectionLeft.Line.LineIndex < line.LineIndex && SelectionRight.Line.LineIndex == line.LineIndex)
                {
                    // Selection ends on this line
                    int w = SelectionRight.Line.WidthUpTo(SelectionRight.IndexInLine);
                    sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), TextColor, DrawOptions);
                    if (w > sprite.Bitmap.Width) w = sprite.Bitmap.Width;
                    if (OverlaySelectedText)
                    {
                        sprite.Bitmap.FillRect(0, 0, w, sprite.Bitmap.Height, SelectionBackgroundColor);
                        sprite.Bitmap.DrawText(line.Text.Replace('\n', ' ').Substring(0, SelectionRight.IndexInLine), TextColorSelected, DrawOptions);
                    }
                    else
                    {
                        Sprite s = new Sprite(Viewport);
                        s.Y = sprite.Y;
                        s.Z = 1;
                        s.Bitmap = new SolidBitmap(w, sprite.Bitmap.Height, SelectionBackgroundColor);
                        SelBoxSprites.Add(s);
                        Sprites[$"box{line.LineIndex}"] = s;
                    }
                }
                else if (SelectionLeft.Line.LineIndex == line.LineIndex && SelectionRight.Line.LineIndex > line.LineIndex)
                {
                    // Selection starts on this line
                    int x = SelectionLeft.Line.WidthUpTo(SelectionLeft.IndexInLine);
                    int w = Font.TextSize(line.Text.Replace('\n', ' ')).Width - SelectionLeft.Line.WidthUpTo(SelectionLeft.IndexInLine);
                    sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), TextColor, DrawOptions);
                    if (x + w > sprite.Bitmap.Width) w = sprite.Bitmap.Width - x;
                    if (w > 0)
                    {
                        if (OverlaySelectedText)
                        {
                            sprite.Bitmap.FillRect(x, 0, w, sprite.Bitmap.Height, SelectionBackgroundColor);
                            Bitmap bmp = new Bitmap(sprite.Bitmap.Width, sprite.Bitmap.Height);
                            bmp.Font = Font;
                            bmp.Unlock();
                            bmp.DrawText(line.Text.Replace('\n', ' '), TextColorSelected, DrawOptions);
                            bmp.Lock();
                            sprite.Bitmap.Build(x, 0, bmp, new Rect(x, 0, w, bmp.Height));
                            bmp.Dispose();
                        }
                        else
                        {
                            Sprite s = new Sprite(Viewport);
                            s.X = x;
                            s.Y = sprite.Y;
                            s.Z = 1;
                            s.Bitmap = new SolidBitmap(w, sprite.Bitmap.Height, SelectionBackgroundColor);
                            SelBoxSprites.Add(s);
                            Sprites[$"box{line.LineIndex}"] = s;
                        }
                    }
                }
                else if (SelectionLeft.Line.LineIndex < line.LineIndex && SelectionRight.Line.LineIndex > line.LineIndex)
                {
                    // Full line is selected
                    int w = Font.TextSize(line.Text.Replace('\n', ' ')).Width;
                    if (w > sprite.Bitmap.Width) w = sprite.Bitmap.Width;
                    if (OverlaySelectedText)
                    {
                        sprite.Bitmap.FillRect(0, 0, w, sprite.Bitmap.Height, SelectionBackgroundColor);
                        sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), TextColorSelected, DrawOptions);
                    }
                    else
                    {
                        Sprite s = new Sprite(Viewport);
                        s.Y = sprite.Y;
                        s.Z = 1;
                        s.Bitmap = new SolidBitmap(w, sprite.Bitmap.Height, SelectionBackgroundColor);
                        SelBoxSprites.Add(s);
                        sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), TextColor, DrawOptions);
                        Sprites[$"box{line.LineIndex}"] = s;
                    }
                }
                else
                {
                    // This line is not included in any part of the selection
                    sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), TextColor, DrawOptions);
                }
            }
            else sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), TextColor, DrawOptions);
            sprite.Bitmap.Lock();
            Sprites[$"line{line.LineIndex}"] = sprite;
            LineSprites.Add(sprite);
        });
        RequireRedrawText = false;
        UpdateCaretPosition(false);
        UpdateBounds();
    }

    protected virtual void RedrawSelectionBoxes()
    {
        RedrawText(true);
    }

    protected virtual void UpdateCaretPosition(bool ResetScroll)
    {
        Sprites["caret"].X = Caret.Line.WidthUpTo(Caret.IndexInLine);
        Sprites["caret"].Y = Caret.Line.LineIndex * LineHeight + Caret.Line.LineIndex * LineMargins;
        if (ResetScroll) RequireCaretRepositioning = true;
    }

    protected virtual void ResetIdle()
    {
        Sprites["caret"].Visible = true;
        if (TimerExists("idle")) ResetTimer("idle");
    }

    public void SetSelection(int StartIndex, int Length, bool WrapAtLineEnd = true, bool CaretAtEnd = true)
    {
        if (!Interactable) return;
        SelectionStart = new CaretIndex(this);
        SelectionStart.Index = StartIndex;
        SelectionEnd = new CaretIndex(this);
        SelectionEnd.Index = StartIndex + Length;
        Caret.Index = CaretAtEnd ? SelectionEnd.Index : SelectionStart.Index;
        Caret.AtEndOfLine = false;
        RedrawSelectionBoxes();
        UpdateCaretPosition(true);
    }

    protected void StartSelection(int? Index = null)
    {
        if (!Interactable) return;
        SelectionStart = new CaretIndex(this);
        SelectionStart.Index = Index ?? Caret.Index;
        SelectionEnd = new CaretIndex(this);
        SelectionEnd.Index = Index ?? Caret.Index;
    }

    protected virtual void CancelSelection(bool Redraw = true, bool _ = false)
    {
        if (!HasSelection) return;
        SelectionStart = null;
        SelectionEnd = null;
        if (Redraw)
        {
            RedrawSelectionBoxes();
            UpdateCaretPosition(true);
        }
    }

    protected virtual void DeleteSelection()
    {
        int start = SelectionLeft.Index;
        int end = SelectionRight.Index;
        int count = end - start;
        RemoveText(start, count);
        CancelSelection();
        RecalculateLines();
    }

    private string GetSelectionText()
    {
        if (!HasSelection) return null;
        return Text.Substring(SelectionLeft.Index, SelectionRight.Index - SelectionLeft.Index);
    }

    private void SelectAll()
    {
        if (!Interactable) return;
        if (!HasSelection) StartSelection();
        SelectionStart.Index = 0;
        SelectionStart.AtEndOfLine = false;
        SelectionEnd.Index = Text.Length;
        SelectionEnd.AtEndOfLine = false;
        Caret.Index = Text.Length;
        Caret.AtEndOfLine = false;
        ResetIdle();
        RedrawSelectionBoxes();
        UpdateCaretPosition(true);
    }

    private void MoveRight(bool Shift, bool Control)
    {
        if (!Interactable) return;
        if (!Shift && HasSelection)
        {
            Caret.AtEndOfLine = SelectionRight.AtEndOfLine;
            Caret.Index = SelectionRight.Index;
            CancelSelection();
            UpdateCaretPosition(true);
            return;
        }
        if (Caret.Index < Text.Length)
        {
            if (Shift && !HasSelection) StartSelection();
            if (Control)
            {
                Caret.Index = TextArea.FindNextCtrlIndex(Text, Caret.Index, false);
                Caret.AtEndOfLine = false;
            }
            else
            {
                Caret.AtEndOfLine = !Caret.Line.EndsInNewline && Caret.Index == Caret.Line.EndIndex;
                Caret.Index++;
            }
            if (Shift)
            {
                CaretIndex SelectionCaret = SelectionStart;
                if (Caret.Index > SelectionRight.Index) SelectionCaret = SelectionEnd;
                SelectionCaret.AtEndOfLine = Caret.AtEndOfLine;
                SelectionCaret.Index = Caret.Index;
                if (SelectionEnd.Index == SelectionStart.Index) CancelSelection();
            }
            ResetIdle();
            if (Shift) RedrawSelectionBoxes();
            UpdateCaretPosition(true);
            MaxCaretPositionInLine = Caret.Line.WidthUpTo(Caret.IndexInLine);
        }
    }

    private void MoveLeft(bool Shift, bool Control)
    {
        if (!Interactable) return;
        if (!Shift && HasSelection)
        {
            Caret.AtEndOfLine = SelectionLeft.AtEndOfLine;
            Caret.Index = SelectionLeft.Index;
            CancelSelection();
            UpdateCaretPosition(true);
            return;
        }
        if (Caret.Index > 0)
        {
            if (Shift && !HasSelection) StartSelection();
            if (Control)
            {
                Caret.Index = TextArea.FindNextCtrlIndex(Text, Caret.Index, true);
            }
            else
            {
                Caret.Index--;
            }
            Caret.AtEndOfLine = false;
            if (Shift)
            {
                CaretIndex SelectionCaret = SelectionEnd;
                if (Caret.Index < SelectionLeft.Index) SelectionCaret = SelectionStart;
                SelectionCaret.AtEndOfLine = Caret.AtEndOfLine;
                SelectionCaret.Index = Caret.Index;
                if (SelectionEnd.Index == SelectionStart.Index) CancelSelection();
            }
            ResetIdle();
            if (Shift) RedrawSelectionBoxes();
            UpdateCaretPosition(true);
            MaxCaretPositionInLine = Caret.Line.WidthUpTo(Caret.IndexInLine);
        }
    }

    private void MoveUp(bool Shift, bool Ctrl)
    {
        if (!Interactable) return;
        if (Ctrl)
        {
            ScrollUp(1);
            return;
        }
        if (!Shift && HasSelection) CancelSelection();
        int OldIndex = Caret.Index;
        if (Caret.Line.LineIndex > 0)
        {
            Line l = Lines[Caret.Line.LineIndex - 1];
            int CurWidth = MaxCaretPositionInLine;
            Caret.Index = l.StartIndex + l.GetIndexAroundWidth(CurWidth);
            Caret.AtEndOfLine = !l.EndsInNewline && Caret.Index == l.EndIndex + 1;
        }
        else
        {
            Caret.Index = 0;
            Caret.AtEndOfLine = false;
        }
        if (OldIndex != Caret.Index)
        {
            if (Shift)
            {
                if (!HasSelection) StartSelection(OldIndex);
                CaretIndex SelectionCaret = SelectionEnd;
                if (Caret.Index < SelectionLeft.Index)
                {
                    if (OldIndex > SelectionLeft.Index)
                    {
                        // Swapped from below selection to the left of the selection
                        SelectionEnd.AtEndOfLine = SelectionLeft.AtEndOfLine;
                        SelectionEnd.Index = SelectionLeft.Index;
                        SelectionStart.AtEndOfLine = Caret.AtEndOfLine;
                        SelectionStart.Index = Caret.Index;
                        SelectionCaret = null;
                    }
                    else SelectionCaret = SelectionStart;
                }
                if (SelectionCaret != null)
                {
                    SelectionCaret.AtEndOfLine = Caret.AtEndOfLine;
                    SelectionCaret.Index = Caret.Index;
                }
            }
            ResetIdle();
            if (Shift) RedrawSelectionBoxes();
            UpdateCaretPosition(true);
        }
    }

    private void MoveDown(bool Shift, bool Ctrl)
    {
        if (!Interactable) return;
        if (Ctrl)
        {
            ScrollDown(1);
            return;
        }
        if (!Shift && HasSelection) CancelSelection();
        int OldIndex = Caret.Index;
        if (Caret.Line.LineIndex < Lines.Count - 1)
        {
            Line l = Lines[Caret.Line.LineIndex + 1];
            int CurWidth = MaxCaretPositionInLine;
            Caret.Index = l.StartIndex + l.GetIndexAroundWidth(CurWidth);
            Caret.AtEndOfLine = !Caret.Line.EndsInNewline && Caret.Index == l.EndIndex + 1;
        }
        else
        {
            Caret.Index = Text.Length;
            Caret.AtEndOfLine = !Caret.Line.EndsInNewline;
        }
        if (OldIndex != Caret.Index)
        {
            if (Shift)
            {
                if (!HasSelection) StartSelection(OldIndex);
                CaretIndex SelectionCaret = SelectionStart;
                if (Caret.Index > SelectionRight.Index)
                {
                    if (OldIndex < SelectionRight.Index)
                    {
                        // Swapped from above selection to the right of the selection
                        SelectionStart.AtEndOfLine = SelectionRight.AtEndOfLine;
                        SelectionStart.Index = SelectionRight.Index;
                        SelectionEnd.AtEndOfLine = Caret.AtEndOfLine;
                        SelectionEnd.Index = Caret.Index;
                        SelectionCaret = null;
                    }
                    else SelectionCaret = SelectionEnd;
                }
                if (SelectionCaret != null)
                {
                    SelectionCaret.AtEndOfLine = Caret.AtEndOfLine;
                    SelectionCaret.Index = Caret.Index;
                }
                if (SelectionEnd.Index == SelectionStart.Index) CancelSelection();
            }
            ResetIdle();
            if (Shift) RedrawSelectionBoxes();
            UpdateCaretPosition(true);
        }
    }

    private void MovePageUp(bool Shift)
    {
        if (!Interactable) return;
        if (!Shift && HasSelection) CancelSelection();
        int OldIndex = Caret.Index;
        if (Caret.Line.LineIndex > 0)
        {
            int CurWidth = Caret.Line.WidthUpTo(Caret.IndexInLine);
            int count = BottomLineIndex - TopLineIndex - 1;
            if (Caret.Line.LineIndex - count < 0) count = Caret.Line.LineIndex;
            Line l = Lines[Caret.Line.LineIndex - count];
            Caret.Index = l.StartIndex;
        }
        else Caret.Index = 0;
        MaxCaretPositionInLine = 0;
        Caret.AtEndOfLine = false;
        if (OldIndex != Caret.Index)
        {
            if (Shift)
            {
                if (!HasSelection) StartSelection(OldIndex);
                CaretIndex SelectionCaret = SelectionEnd;
                if (Caret.Index < SelectionLeft.Index)
                {
                    if (OldIndex > SelectionLeft.Index)
                    {
                        // Swapped from below selection to the left of the selection
                        SelectionEnd.AtEndOfLine = SelectionLeft.AtEndOfLine;
                        SelectionEnd.Index = SelectionLeft.Index;
                        SelectionStart.AtEndOfLine = Caret.AtEndOfLine;
                        SelectionStart.Index = Caret.Index;
                        SelectionCaret = null;
                    }
                    else SelectionCaret = SelectionStart;
                }
                if (SelectionCaret != null)
                {
                    SelectionCaret.AtEndOfLine = Caret.AtEndOfLine;
                    SelectionCaret.Index = Caret.Index;
                }
            }
            ResetIdle();
            if (Shift) RedrawSelectionBoxes();
            UpdateCaretPosition(true);
        }
    }

    private void MovePageDown(bool Shift)
    {
        if (!Interactable) return;
        if (!Shift && HasSelection) CancelSelection();
        int OldIndex = Caret.Index;
        if (Caret.Line.LineIndex < Lines.Count - 1)
        {
            int CurWidth = Caret.Line.WidthUpTo(Caret.IndexInLine);
            int count = BottomLineIndex - TopLineIndex - 1;
            if (Caret.Line.LineIndex + count >= Lines.Count) count = Lines.Count - Caret.Line.LineIndex - 1;
            Line l = Lines[Caret.Line.LineIndex + count];
            Caret.Index = l.EndIndex;
            if (!Caret.Line.EndsInNewline)
            {
                Caret.AtEndOfLine = true;
                Caret.Index++;
            }
        }
        else
        {
            Caret.Index = Text.Length;
            Caret.AtEndOfLine = !Caret.Line.EndsInNewline;
        }
        MaxCaretPositionInLine = Caret.Line.WidthUpTo(Caret.IndexInLine);
        if (OldIndex != Caret.Index)
        {
            if (Shift)
            {
                if (!HasSelection) StartSelection(OldIndex);
                CaretIndex SelectionCaret = SelectionStart;
                if (Caret.Index > SelectionRight.Index)
                {
                    if (OldIndex < SelectionRight.Index)
                    {
                        // Swapped from above selection to the right of the selection
                        SelectionStart.AtEndOfLine = SelectionRight.AtEndOfLine;
                        SelectionStart.Index = SelectionRight.Index;
                        SelectionEnd.AtEndOfLine = Caret.AtEndOfLine;
                        SelectionEnd.Index = Caret.Index;
                        SelectionCaret = null;
                    }
                    else SelectionCaret = SelectionEnd;
                }
                if (SelectionCaret != null)
                {
                    SelectionCaret.AtEndOfLine = Caret.AtEndOfLine;
                    SelectionCaret.Index = Caret.Index;
                }
                if (SelectionEnd.Index == SelectionStart.Index) CancelSelection();
            }
            ResetIdle();
            if (Shift) RedrawSelectionBoxes();
            UpdateCaretPosition(true);
        }
    }

    private void MoveHome(bool Shift)
    {
        if (!Interactable) return;
        if (!Shift && HasSelection) CancelSelection();
        int indexadd = 0;
        if (Caret.Line.LineIndex == 0 || Lines[Caret.Line.LineIndex - 1].EndsInNewline)
        {
            foreach (char c in Caret.Line.Text)
            {
                if (c == '\n' || c == ' ') indexadd++;
                else break;
            }
            if (indexadd == Caret.Line.Text.Length) indexadd = 0;
            if (Caret.Index == Caret.Line.StartIndex + indexadd)
            {
                indexadd = 0;
                HomeSnappedToFirstPrior = true;
            }
            else if (Caret.Index == Caret.Line.StartIndex && HomeSnappedToFirstPrior)
            {
                HomeSnappedToFirstPrior = false;
            }
        }
        if (Caret.IndexInLine > 0 || indexadd > 0)
        {
            if (Shift && !HasSelection) StartSelection();
            int OldIndex = Caret.Index;
            Caret.Index = Caret.Line.StartIndex + indexadd;
            Caret.AtEndOfLine = false;
            if (Shift)
            {
                if (OldIndex > SelectionLeft.Index)
                {
                    if (Caret.Index < SelectionLeft.Index)
                    {
                        SelectionEnd.AtEndOfLine = SelectionLeft.AtEndOfLine;
                        SelectionEnd.Index = SelectionLeft.Index;
                        SelectionStart.AtEndOfLine = Caret.AtEndOfLine;
                        SelectionStart.Index = Caret.Index;
                    }
                    else
                    {
                        SelectionRight.AtEndOfLine = Caret.AtEndOfLine;
                        SelectionRight.Index = Caret.Index;
                    }
                }
                else
                {
                    SelectionLeft.AtEndOfLine = Caret.AtEndOfLine;
                    SelectionLeft.Index = Caret.Index;
                }
                if (SelectionEnd.Index == SelectionStart.Index) CancelSelection();
            }
            ResetIdle();
            if (Shift) RedrawSelectionBoxes();
            UpdateCaretPosition(true);
        }
        MaxCaretPositionInLine = Caret.Line.WidthUpTo(Caret.IndexInLine);
    }

    private void MoveEnd(bool Shift)
    {
        if (!Interactable) return;
        if (!Shift && HasSelection) CancelSelection();
        if (Caret.IndexInLine < Caret.Line.EndIndex + 1 && Caret.Index < Text.Length)
        {
            if (Shift && !HasSelection) StartSelection();
            int OldIndex = Caret.Index;
            bool nl = Caret.Line.EndsInNewline;
            Caret.Index = Caret.Line.EndIndex + (nl ? 0 : 1);
            Caret.AtEndOfLine = !nl;
            if (Shift)
            {
                if (OldIndex < SelectionRight.Index)
                {
                    if (Caret.Index > SelectionRight.Index)
                    {
                        SelectionStart.AtEndOfLine = SelectionRight.AtEndOfLine;
                        SelectionStart.Index = SelectionRight.Index;
                        SelectionEnd.AtEndOfLine = Caret.AtEndOfLine;
                        SelectionEnd.Index = Caret.Index;
                    }
                    else
                    {
                        SelectionLeft.AtEndOfLine = Caret.AtEndOfLine;
                        SelectionLeft.Index = Caret.Index;
                    }
                }
                else
                {
                    SelectionRight.AtEndOfLine = Caret.AtEndOfLine;
                    SelectionRight.Index = Caret.Index;
                }
                if (SelectionStart.Index == SelectionEnd.Index) CancelSelection();
            }
            ResetIdle();
            if (Shift) RedrawSelectionBoxes();
            UpdateCaretPosition(true);
            MaxCaretPositionInLine = Caret.Line.WidthUpTo(Caret.IndexInLine);
        }
    }

    protected void SetPreviousViewState()
    {
        UndoableStates.Last().ParentScrolledX = Parent.ScrolledX;
        UndoableStates.Last().ParentScrolledY = Parent.ScrolledY;
        UndoableStates.Last().Caret = (CaretIndex)Caret.Clone();
    }

    protected virtual void AddUndoState()
    {
        UndoableStates.Add(new TextAreaState(this));
        RedoableStates.Clear();
    }

    protected virtual void Undo()
    {
        if (!Interactable || ReadOnly) return;
        if (UndoableStates.Count < 2) return;
        TextAreaState PreviousState = UndoableStates[UndoableStates.Count - 2];
        PreviousState.Apply(true);
        RedoableStates.Add(UndoableStates[UndoableStates.Count - 1]);
        UndoableStates.RemoveAt(UndoableStates.Count - 1);
    }

    protected virtual void Redo()
    {
        if (!Interactable || ReadOnly) return;
        if (RedoableStates.Count < 1) return;
        TextAreaState PreviousState = RedoableStates[RedoableStates.Count - 1];
        PreviousState.Apply(true);
        UndoableStates.Add(PreviousState);
        RedoableStates.RemoveAt(RedoableStates.Count - 1);
    }

    protected virtual void UpdateUpDownAnchor(bool Now = false)
    {
        if (!Now)
        {
            RequireUpdateUpDownAnchor = true;
            return;
        }
        MaxCaretPositionInLine = Caret.Line.WidthUpTo(Caret.IndexInLine);
        RequireUpdateUpDownAnchor = false;
    }

    protected virtual void InsertText(int Index, string Text)
    {
        if (!Interactable || ReadOnly) return;
        Text = Text.Replace("\r", "");
        if (Text.Length == 0) return;
        SetPreviousViewState();
        if (HasSelection)
        {
            int count = SelectionRight.Index - SelectionLeft.Index;
            if (Index > SelectionLeft.Index) Index -= count;
            DeleteSelection();
        }
        this.Text = this.Text.Insert(Index, Text);
        if (Index <= Caret.Index) Caret.Index += Text.Length;
        if (Text == "\n") Caret.AtEndOfLine = false;
        AddUndoState();
        ResetIdle();
        RecalculateLines();
        UpdateUpDownAnchor();
    }

    protected virtual void RemoveText(int Index, int Count)
    {
        if (!Interactable || ReadOnly) return;
        if (Index < 0) return;
        Count = Math.Min(Text.Length - Index, Count);
        if (Count < 1) return;
        SetPreviousViewState();
        Text = Text.Remove(Index, Count);
        if (Index <= Caret.Index) Caret.Index = Math.Max(Index, Caret.Index - Count);
        Caret.AtEndOfLine = !Caret.Line.EndsInNewline && Caret.Index == Caret.Line.EndIndex + 1;
        AddUndoState();
        ResetIdle();
        RecalculateLines();
        UpdateUpDownAnchor();
    }

    private void CutSelection()
    {
        if (!Interactable || ReadOnly) return;
        if (!HasSelection) return;
        CopySelection();
        DeleteSelection();
    }

    private void CopySelection()
    {
        if (!Interactable || ReadOnly) return;
        if (!HasSelection) return;
        BoolEventArgs e = new BoolEventArgs(true);
        OnCopy?.Invoke(e);
        if (e.Value)
        {
            string text = GetSelectionText();
            Input.SetClipboard(text);
        }
    }

    private void Paste()
    {
        if (!Interactable || ReadOnly) return;
        BoolEventArgs e = new BoolEventArgs(true);
        OnPaste?.Invoke(e);
        if (e.Value)
        {
            string text = Input.GetClipboard();
            InsertText(Caret.Index, text);
        }
    }

    public override void TextInput(TextEventArgs e)
    {
        if (!Interactable || ReadOnly) return;
        base.TextInput(e);
        string text = Text;
        if (!string.IsNullOrEmpty(e.Text))
        {
            if (e.Text == "\n" && Input.Press(Keycode.CTRL)) return;
            if (e.Text != "\n" && InsertMode)
            {
                int DistanceToNewline = -1;
                for (int i = Caret.Index; i < Text.Length; i++)
                {
                    if (Text[i] == '\n')
                    {
                        DistanceToNewline = i - Caret.Index;
                        break;
                    }
                }
                if (DistanceToNewline > 0)
                {
                    RemoveText(Caret.Index, Math.Min(DistanceToNewline, e.Text.Length));
                }
                else if (DistanceToNewline == -1)
                {
                    RemoveText(Caret.Index, Math.Min(Text.Length - Caret.Index, e.Text.Length));
                }
            }
            InsertText(Caret.Index, e.Text);
        }
        else if (e.Backspace || e.Delete)
        {
            if (HasSelection) DeleteSelection();
            else
            {
                if (e.Delete)
                {
                    if (Caret.Index < Text.Length)
                    {
                        int Count = 1;
                        if (Input.Press(Keycode.CTRL)) Count = TextArea.FindNextCtrlIndex(Text, Caret.Index, false) - Caret.Index;
                        RemoveText(Caret.Index, Count);
                    }
                }
                else
                {
                    int Count = 1;
                    if (Input.Press(Keycode.CTRL)) Count = Caret.Index - TextArea.FindNextCtrlIndex(Text, Caret.Index, true);
                    RemoveText(Caret.Index - Count, Count);
                }
            }
        }
        else if (e.Tab) TabInput();
        if (Text != text) OnTextChanged?.Invoke(new BaseEventArgs());
    }

    protected void TabInput()
    {
        if (!Interactable || ReadOnly) return;
        if (Input.Press(Keycode.CTRL)) return;
        if (HasSelection && SelectionStart.Line.LineIndex != SelectionEnd.Line.LineIndex)
        {
            int StartIdx = SelectionLeft.Line.LineIndex;
            int EndIdx = SelectionRight.Line.LineIndex;
            for (int i = StartIdx; i <= EndIdx; i++)
            {
                TabInputForLine(i, false);
            }
        }
        else
        {
            TabInputForLine(Caret.Line.LineIndex, true);
        }
        UpdateCaretPosition(true);
        RedrawSelectionBoxes();
    }

    protected void TabInputForLine(int LineIndex, bool TestIfCaretPrecedes)
    {
        bool CaretPrecedesLineContent = true;
        int LineStartIndex = -1;
        for (int i = 0; i < Lines[LineIndex].Length; i++)
        {
            if (Lines[LineIndex].Text[i] != ' ' && Lines[LineIndex].Text[i] != '\n')
            {
                if (LineStartIndex == -1) LineStartIndex = i;
                if (i >= Caret.IndexInLine) continue;
                CaretPrecedesLineContent = false;
                break;
            }
        }
        if (LineStartIndex == -1) LineStartIndex = Lines[LineIndex].Text.Length;
        if (HasSelection && (!TestIfCaretPrecedes || CaretPrecedesLineContent))
        {
            if (Input.Press(Keycode.SHIFT))
            {
                int Count = 0;
                for (int i = 0; i < LineStartIndex; i++)
                {
                    if (Lines[LineIndex].Text[i] == ' ') Count++;
                }
                if (Count == 0) return;
                Count = Math.Min(2, Count);
                CaretIndex StartIndex = (CaretIndex)SelectionLeft.Clone();
                CaretIndex EndIndex = (CaretIndex)SelectionRight.Clone();
                bool Before = Lines[LineIndex].StartIndex < StartIndex.Index;
                CancelSelection();
                CaretIndex OldCaret = (CaretIndex)Caret.Clone();
                Caret.Index = Lines[LineIndex].StartIndex;
                RemoveText(Lines[LineIndex].StartIndex, Count);
                Caret = OldCaret;
                SelectionStart = StartIndex;
                SelectionEnd = EndIndex;
                if (Before) SelectionLeft.Index -= Count;
                SelectionRight.Index -= Count;
                int IndexRedux = Count;
                if (Caret.Line.LineIndex == LineIndex) IndexRedux = Math.Min(IndexRedux, Caret.IndexInLine);
                if (Caret.Index >= Lines[LineIndex].StartIndex) Caret.Index -= IndexRedux;
            }
            else
            {
                CaretIndex StartIndex = (CaretIndex)SelectionLeft.Clone();
                CaretIndex EndIndex = (CaretIndex)SelectionRight.Clone();
                bool Before = Lines[LineIndex].StartIndex < StartIndex.Index;
                CancelSelection();
                CaretIndex OldCaret = (CaretIndex)Caret.Clone();
                Caret.Index = Lines[LineIndex].StartIndex;
                InsertText(Lines[LineIndex].StartIndex, "  ");
                Caret = OldCaret;
                SelectionStart = StartIndex;
                SelectionEnd = EndIndex;
                if (Before) SelectionLeft.Index += 2;
                SelectionRight.Index += 2;
                if (Caret.Index >= Lines[LineIndex].StartIndex) Caret.Index += 2;
            }
        }
        else if (!HasSelection)
        {
            InsertText(Caret.Index, "  ");
        }
    }

    protected virtual void ToggleInsertMode()
    {
        if (!Interactable) return;
        InsertMode = !InsertMode;
        if (InsertMode)
        {
            CaretWidth = Font.TextSize('a').Width;
            ((SolidBitmap)Sprites["caret"].Bitmap).SetColor(new Color(255, 255, 255, 128));
        }
        else
        {
            CaretWidth = 1;
            ((SolidBitmap)Sprites["caret"].Bitmap).SetColor(Color.WHITE);
        }
        ((SolidBitmap)Sprites["caret"].Bitmap).SetSize(CaretWidth, LineHeight);
    }

    protected virtual CaretIndex GetHoveredIndex(MouseEventArgs e)
    {
        int rx = e.X - Viewport.X + LeftCutOff;
        int ry = e.Y - Viewport.Y + TopCutOff;
        int LineIndex = (int)Math.Round((double)(ry - Font.Size / 2) / (LineHeight + LineMargins));
        if (LineIndex < 0) LineIndex = 0;
        if (LineIndex >= Lines.Count) LineIndex = Lines.Count - 1;
        Line Line = Lines[LineIndex];
        if (Line.Length == 0) return new CaretIndex(this) { Index = Line.StartIndex };
        if (rx >= Line.LineWidth)
        {
            return new CaretIndex(this) { Index = Line.EndIndex + (Line.EndsInNewline ? 0 : 1), AtEndOfLine = !Line.EndsInNewline };
        }
        else if (rx < 0)
        {
            return new CaretIndex(this) { Index = Line.StartIndex, AtEndOfLine = false };
        }
        int idx = Line.StartIndex + Line.GetIndexAroundWidth(rx);
        return new CaretIndex(this) { Index = idx, AtEndOfLine = !Line.EndsInNewline && idx == Line.EndIndex + 1 };
    }

    protected virtual bool MouseInsideTextArea(MouseEventArgs e)
    {
        return Parent.Mouse.Inside;
    }

    public override void LeftMouseDown(MouseEventArgs e)
    {
        base.LeftMouseDown(e);
        if (!Interactable) return;
        if (MouseInsideTextArea(e))
        {
            if (TimerExists("triple"))
            {
                if (!TimerPassed("triple"))
                {
                    TripleLeftMouseDownInside(e);
                    DestroyTimer("triple");
                    CancelDoubleClick();
                    return;
                }
                else DestroyTimer("triple");
            }
            CaretIndex Index = GetHoveredIndex(e);
            if (Input.Press(Keycode.SHIFT))
            {
                if (Caret.Index != Index.Index) MouseMoving(e);
            }
            else
            {
                if (HasSelection) CancelSelection();
                Caret = Index;
                UpdateCaretPosition(true);
                ResetIdle();
            }
        }
        else if (Mouse.Inside) CancelDoubleClick();
    }

    public override void DoubleLeftMouseDownInside(MouseEventArgs e)
    {
        base.DoubleLeftMouseDownInside(e);
        if (!Interactable) return;
        if (TimerExists("triple")) DestroyTimer("triple");
        SetTimer("triple", 300);
        SnapToWords = true;
        CaretIndex Index = GetHoveredIndex(e);
        MinSelIndex = TextArea.FindNextCtrlIndex(Text, Index.Index, true);
        MaxSelIndex = TextArea.FindNextCtrlIndex(Text, Index.Index, false);
        if (!HasSelection) StartSelection();
        SelectionStart.AtEndOfLine = false;
        SelectionStart.Index = (int)MinSelIndex;
        SelectionEnd.Index = (int)MaxSelIndex;
        SelectionEnd.AtEndOfLine = !SelectionEnd.Line.EndsInNewline && SelectionEnd.Index == SelectionEnd.Line.EndIndex + 1;
        Caret.AtEndOfLine = SelectionEnd.AtEndOfLine;
        Caret.Index = SelectionEnd.Index;
        ResetIdle();
        RedrawSelectionBoxes();
        UpdateCaretPosition(true);
    }

    private void TripleLeftMouseDownInside(MouseEventArgs e)
    {
        if (!Interactable) return;
        if (!HasSelection) StartSelection();
        CaretIndex Index = GetHoveredIndex(e);
        Line Line = Index.Line;
        SelectionStart.AtEndOfLine = false;
        SelectionStart.Index = Line.StartIndex;
        SelectionEnd.AtEndOfLine = !Line.EndsInNewline;
        SelectionEnd.Index = Line.EndIndex + (Line.EndsInNewline ? 0 : 1);
        Caret.AtEndOfLine = SelectionEnd.AtEndOfLine;
        Caret.Index = SelectionEnd.Index;
        ResetIdle();
        RedrawSelectionBoxes();
        UpdateCaretPosition(true);
        SnapToLines = true;
        LineSnapIndex = Line.LineIndex;
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (!Interactable) return;
        if (Mouse.LeftMousePressed && StartedInParent)
        {
            CaretIndex Index = GetHoveredIndex(e);
            if (SnapToWords)
            {
                int Left = TextArea.FindNextCtrlIndex(Text, Index.Index, true);
                int Right = TextArea.FindNextCtrlIndex(Text, Index.Index, false);
                if (Left > MinSelIndex) Left = (int)MinSelIndex;
                if (Right < MaxSelIndex) Right = (int)MaxSelIndex;
                SelectionStart.AtEndOfLine = false;
                SelectionStart.Index = Left;
                SelectionEnd.Index = Right;
                SelectionEnd.AtEndOfLine = !SelectionEnd.Line.EndsInNewline && SelectionEnd.Index == SelectionEnd.Line.EndIndex + 1;
                if (Left < MinSelIndex)
                {
                    Caret.AtEndOfLine = SelectionStart.AtEndOfLine;
                    Caret.Index = SelectionStart.Index;
                }
                else
                {
                    Caret.AtEndOfLine = SelectionEnd.AtEndOfLine;
                    Caret.Index = SelectionEnd.Index;
                }
            }
            else if (SnapToLines)
            {
                Line LineStart = Lines[Index.Line.LineIndex <= (int)LineSnapIndex ? Index.Line.LineIndex : (int)LineSnapIndex];
                Line LineEnd = Lines[Index.Line.LineIndex <= (int)LineSnapIndex ? (int)LineSnapIndex : Index.Line.LineIndex];
                SelectionStart.AtEndOfLine = false;
                SelectionStart.Index = LineStart.StartIndex;
                SelectionEnd.AtEndOfLine = !LineEnd.EndsInNewline;
                SelectionEnd.Index = LineEnd.EndIndex + (LineEnd.EndsInNewline ? 0 : 1);
                Caret.AtEndOfLine = SelectionEnd.AtEndOfLine;
                Caret.Index = SelectionEnd.Index;
            }
            else
            {
                if (Caret.Index != Index.Index)
                {
                    if (!HasSelection) StartSelection();
                    SelectionEnd.AtEndOfLine = Index.AtEndOfLine;
                    SelectionEnd.Index = Index.Index;
                    Caret.AtEndOfLine = Index.AtEndOfLine;
                    Caret.Index = Index.Index;
                }
            }
            if (HasSelection && SelectionStart.Index == SelectionEnd.Index) CancelSelection();
            ResetIdle();
            RedrawSelectionBoxes();
            UpdateCaretPosition(true);
            MaxCaretPositionInLine = Caret.Line.WidthUpTo(Caret.IndexInLine);
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (!Interactable) return;
        if (Mouse.LeftMouseReleased)
        {
            SnapToWords = false;
            SnapToLines = false;
            MinSelIndex = null;
            MaxSelIndex = null;
            LineSnapIndex = null;
        }
    }

    public class Line : ICloneable
    {
        public Font Font;
        public int LineIndex;
        public int StartIndex;
        public int EndIndex => StartIndex + Math.Max(0, Length - 1);
        public int Length = 0;
        public bool EndsInNewline = true;
        public string Text = "";
        public List<int> CharacterWidths = new List<int>();
        public int LineWidth => Math.Max(1, CharacterWidths.Sum());

        public Line(Font Font)
        {
            this.Font = Font;
        }

        public void AddCharacter(char c, int w)
        {
            EndsInNewline = c == '\n';
            Text += c;
            CharacterWidths.Add(w);
            Length++;
        }

        public void AddText(string Text)
        {
            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '\r') continue;
                AddCharacter(Text[i], GetTextWidth(Font, Text[i].ToString()));
            }
        }

        public void SetText(string Text)
        {
            EndsInNewline = true;
            this.Text = "";
            Length = 0;
            CharacterWidths.Clear();
            AddText(Text);
        }

        public int WidthUpTo(int Index)
        {
            string txt = Text.Substring(0, Index);
            int w = Font.TextSize(txt).Width;
            return w;
        }

        public int GetIndexAroundWidth(int Width)
        {
            if (Width == 0) return 0;
            for (int i = 0; i < Text.Length - (EndsInNewline ? 1 : 0); i++)
            {
                int w = GetTextWidth(Font, Text.Substring(0, i));
                if (w + GetTextWidth(Font, Text[i].ToString()) / 2 >= Width) return i;
            }
            return Text.Length - (EndsInNewline ? 1 : 0);
        }

        public object Clone()
        {
            Line line = new Line(Font);
            line.EndsInNewline = EndsInNewline;
            line.StartIndex = StartIndex;
            line.LineIndex = LineIndex;
            line.Length = Length;
            line.Text = Text;
            line.CharacterWidths = new List<int>(CharacterWidths);
            return line;
        }
    }

    public class CaretIndex : ICloneable
    {
        private MultilineTextArea TextArea;

        public Line Line => TextArea.Lines.Find(l => Index - (AtEndOfLine ? 1 : 0) >= l.StartIndex && Index - (AtEndOfLine ? 1 : 0) <= l.EndIndex) ?? TextArea.Lines.Last();
        public int Index;
        public int IndexInLine => Index - Line.StartIndex;
        public bool AtEndOfLine = false;

        public CaretIndex(MultilineTextArea TextArea)
        {
            this.TextArea = TextArea;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj is CaretIndex)
            {
                CaretIndex c = (CaretIndex)obj;
                return Index == c.Index && AtEndOfLine == c.AtEndOfLine;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public object Clone()
        {
            return new CaretIndex(TextArea) { AtEndOfLine = AtEndOfLine, Index = Index };
        }
    }

    public class TextAreaState
    {
        // If this field differs
        public string Text;

        // Then use these properties to revert state
        protected MultilineTextArea TextArea;
        public CaretIndex Caret;
        public int ParentScrolledX;
        public int ParentScrolledY;
        public int MaxChildWidth;
        public int MaxChildHeight;

        public TextAreaState(MultilineTextArea TextArea)
        {
            this.TextArea = TextArea;
            Text = TextArea.Text;
            Caret = (CaretIndex)TextArea.Caret?.Clone();
            ParentScrolledX = TextArea.Parent.ScrolledX;
            ParentScrolledY = TextArea.Parent.ScrolledY;
            MaxChildWidth = ((Widget)TextArea.Parent).MaxChildWidth;
            MaxChildHeight = ((Widget)TextArea.Parent).MaxChildHeight;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return false;
            if (obj is TextAreaState)
            {
                TextAreaState s = (TextAreaState)obj;
                return Caret.Equals(s.Caret) &&
                       ParentScrolledX == s.ParentScrolledX &&
                       ParentScrolledY == s.ParentScrolledY &&
                       MaxChildWidth == s.MaxChildWidth &&
                       MaxChildHeight == s.MaxChildHeight;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual void Apply(bool _ = false)
        {
            TextArea.SetText(Text, false, false);
            TextArea.Caret = (CaretIndex)Caret.Clone();
            TextArea.Parent.ScrolledX = ParentScrolledX;
            TextArea.Parent.ScrolledY = ParentScrolledY;
            ((Widget)TextArea.Parent).MaxChildWidth = MaxChildWidth;
            ((Widget)TextArea.Parent).MaxChildHeight = MaxChildHeight;
            TextArea.UpdateHeight();
            ((Widget)TextArea.Parent).UpdateAutoScroll();
            TextArea.Window.UI.SetSelectedWidget(TextArea);
        }
    }
}
