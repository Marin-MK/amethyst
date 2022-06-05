using odl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace amethyst;

public class NewMultilineTextArea : Widget
{
    public string Text { get; protected set; } = "";
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.WHITE;
    public Color TextColorSelected { get; protected set; } = Color.BLACK;
    public Color SelectionBackgroundColor { get; protected set; } = new Color(128, 128, 255);
    private int _FontLineHeight = -1;
    private int _UserSetLineHeight = -1;
    public int LineHeight => _UserSetLineHeight == -1 ? _FontLineHeight : _UserSetLineHeight;
    public DrawOptions DrawOptions { get; protected set; } = DrawOptions.None;
    public bool OverlaySelectedText { get; protected set; } = true;

    public BaseEvent OnTextChanged;
    public BoolEvent OnCopy;
    public BoolEvent OnPaste;

    List<Line> Lines;
    List<Sprite> LineSprites = new List<Sprite>();
    List<Sprite> SelBoxSprites = new List<Sprite>();
    CaretIndex Caret;
    CaretIndex? SelectionStart;
    CaretIndex? SelectionEnd;
    CaretIndex? SelectionLeft => SelectionStart.Index > SelectionEnd.Index ? SelectionEnd : SelectionStart;
    CaretIndex? SelectionRight => SelectionStart.Index > SelectionEnd.Index ? SelectionStart : SelectionEnd;
    int LineWidthLimit;
    bool RequireRecalculation = false;
    bool RequireRedrawText = false;
    bool RequireCaretRepositioning = false;
    bool EnteringText = false;
    bool HasSelection => SelectionStart != null;
    int OldTopLineIndex = 0;
    int OldBottomLineIndex = 0;
    int TopLineIndex => Parent.ScrolledY / LineHeight;
    int BottomLineIndex => TopLineIndex + Parent.Size.Height / LineHeight;

    public NewMultilineTextArea(IContainer Parent) : base(Parent)
    {
        Lines = new List<Line>() { new Line(null) { LineIndex = 0, StartIndex = 0 } };
        Caret = new CaretIndex(this);
        Caret.Index = 0;
        Sprites["caret"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, Color.WHITE));
        Sprites["caret"].Z = 2;
        Sprites["caret"].Visible = false;
        OnSizeChanged += _ =>
        {
            if (LineWidthLimit != Size.Width)
            {
                LineWidthLimit = Size.Width;
                RecalculateLines();
            }
        };
        OnWidgetSelected += WidgetSelected;
        OnDisposed += _ =>
        {
            this.Window.UI.SetSelectedWidget(null);
            Input.SetCursor(odl.SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        };
        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.RIGHT), _ => MoveRight(Input.Press(Keycode.SHIFT), Input.Press(Keycode.CTRL))),
            new Shortcut(this, new Key(Keycode.LEFT), _ => MoveLeft(Input.Press(Keycode.SHIFT), Input.Press(Keycode.CTRL))),
            new Shortcut(this, new Key(Keycode.UP), _ => MoveUp(Input.Press(Keycode.SHIFT))),
            new Shortcut(this, new Key(Keycode.DOWN), _ => MoveDown(Input.Press(Keycode.SHIFT))),
            new Shortcut(this, new Key(Keycode.HOME), _ => MoveHome(Input.Press(Keycode.SHIFT))),
            new Shortcut(this, new Key(Keycode.END), _ => MoveEnd(Input.Press(Keycode.SHIFT))),
            new Shortcut(this, new Key(Keycode.PAGEUP), _ => MovePageUp(Input.Press(Keycode.SHIFT))),
            new Shortcut(this, new Key(Keycode.PAGEDOWN), _ => MovePageDown(Input.Press(Keycode.SHIFT))),
            new Shortcut(this, new Key(Keycode.A, Keycode.CTRL), _ => SelectAll()),
            new Shortcut(this, new Key(Keycode.X, Keycode.CTRL), _ => CutSelection()),
            new Shortcut(this, new Key(Keycode.C, Keycode.CTRL), _ => CopySelection()),
            new Shortcut(this, new Key(Keycode.V, Keycode.CTRL), _ => Paste()),
            new Shortcut(this, new Key(Keycode.ESCAPE), _ => CancelSelection())
        });
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            Text = Text.Replace("\r", "");
            this.Text = Text;
            RecalculateLines();
        }
    }

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            _FontLineHeight = Font.Size + 5;
            ((SolidBitmap) Sprites["caret"].Bitmap).SetSize(1, this.LineHeight);
            RecalculateLines();
        }
    }

    public void SetTextColor(Color TextColor)
    {
        if (this.TextColor != TextColor)
        {
            this.TextColor = TextColor;
            RedrawText();
        }
    }

    public void SetLineHeight(int LineHeight)
    {
        if (this.LineHeight != LineHeight || _UserSetLineHeight == -1)
        {
            int OldHeight = this.LineHeight;
            _UserSetLineHeight = LineHeight;
            ((SolidBitmap)Sprites["caret"].Bitmap).SetSize(1, this.LineHeight);
            if (OldHeight != this.LineHeight) RedrawText();
        }
    }

    public void SetSelectionBackgroundColor(Color SelectionBackgroundColor)
    {
        if (this.SelectionBackgroundColor != SelectionBackgroundColor)
        {
            this.SelectionBackgroundColor = SelectionBackgroundColor;
            if (HasSelection) this.RedrawText();
        }
    }

    public void SetOverlaySelectedText(bool OverlaySelectedText)
    {
        if (this.OverlaySelectedText != OverlaySelectedText)
        {
            this.OverlaySelectedText = OverlaySelectedText;
            if (HasSelection) this.RedrawText();
        }
    }

    public void SetTextColorSelected(Color TextColorSelected)
    {
        if (this.TextColorSelected != TextColorSelected)
        {
            this.TextColorSelected = TextColorSelected;
            if (HasSelection) this.RedrawText();
        }
    }

    public override void Update()
    {
        base.Update();
        if (!SelectedWidget)
        {
            if (EnteringText) WidgetDeselected(new BaseEventArgs());
            if (Sprites["caret"].Visible) Sprites["caret"].Visible = false;
        }
        if (Parent.ScrolledY % LineHeight != 0)
        {
            Parent.ScrolledY -= Parent.ScrolledY % LineHeight;
            ((Widget) Parent).UpdateAutoScroll();
        }
        if (OldTopLineIndex != TopLineIndex || OldBottomLineIndex != BottomLineIndex)
        {
            RequireRedrawText = true;
        }
        if (RequireRecalculation) RecalculateLines(true);
        if (RequireRedrawText) RedrawText(true);
        OldTopLineIndex = TopLineIndex;
        OldBottomLineIndex = BottomLineIndex;
        if (RequireCaretRepositioning)
        {
            if (Caret.Line.LineIndex < TopLineIndex) ScrollUp(TopLineIndex - Caret.Line.LineIndex);
            if (Caret.Line.LineIndex >= BottomLineIndex) ScrollDown(Caret.Line.LineIndex - BottomLineIndex + 1);
            RequireCaretRepositioning = false;
        }
        if (TimerPassed("idle"))
        {
            Sprites["caret"].Visible = !Sprites["caret"].Visible;
            ResetTimer("idle");
        }
    }

    private void ScrollUp(int Count)
    {
        int px = Count * LineHeight;
        Parent.ScrolledY = Math.Max(Parent.ScrolledY - px, 0);
        ((Widget) Parent).UpdateAutoScroll();
    }

    private void ScrollDown(int Count)
    {
        int px = Count * LineHeight;
        Parent.ScrolledY += px;
        ((Widget) Parent).UpdateAutoScroll();
    }

    public override void WidgetSelected(BaseEventArgs e)
    {
        base.WidgetSelected(e);
        EnteringText = true;
        Input.StartTextInput();
        SetTimer("idle", 400);
    }

    public override void WidgetDeselected(BaseEventArgs e)
    {
        base.WidgetDeselected(e);
        EnteringText = false;
        Input.StopTextInput();
    }

    private void RecalculateLines(bool Now = false)
    {
        if (!Now)
        {
            RequireRecalculation = true;
            RequireRedrawText = true;
            RequireCaretRepositioning = true;
            return;
        }
        if (this.Font == null) return;
        int startidx = 0;
        int lastsplittableindex = -1;
        this.Lines.Clear();
        for (int i = 0; i < this.Text.Length; i++)
        {
            char c = this.Text[i];
            string txt = this.Text.Substring(startidx, i - startidx + 1);
            Size s = this.Font.TextSize(txt);
            if (c == '\n')
            {
                Line l = new Line(this.Font);
                l.LineIndex = Lines.Count;
                l.StartIndex = startidx;
                l.AddText(this.Text.Substring(startidx, i - startidx + 1));
                this.Lines.Add(l);
                startidx = i + 1;
                if (i == Text.Length - 1)
                {
                    this.Lines.Add(new Line(this.Font)
                    {
                        LineIndex = Lines.Count,
                        StartIndex = startidx
                    });
                }
            }
            else if (s.Width >= LineWidthLimit)
            {
                int endidx = lastsplittableindex == -1 ? i : lastsplittableindex + 1;
                Line l = new Line(this.Font);
                l.LineIndex = Lines.Count;
                l.StartIndex = startidx;
                l.AddText(this.Text.Substring(startidx, endidx - startidx - 1));
                this.Lines.Add(l);
                startidx = endidx - 1;
                lastsplittableindex = -1;
            }
            else if (c == ' ' || c == '-')
            {
                lastsplittableindex = i + 1;
            }
        }
        if (startidx != this.Text.Length)
        {
            Line l = new Line(this.Font);
            l.LineIndex = Lines.Count;
            l.StartIndex = startidx;
            l.AddText(this.Text.Substring(startidx));
            this.Lines.Add(l);
        }
        else if (Lines.Count == 0)
        {
            this.Lines.Add(new Line(this.Font)
            {
                LineIndex = 0,
                StartIndex = 0
            });
        }
        RequireRecalculation = false;
    }

    private void RedrawText(bool Now = false)
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
        int h = Lines.Count * LineHeight + 2;
        if (h >= Parent.Size.Height) h += ((h - Parent.Size.Height) % LineHeight);
        SetHeight(h);
        Lines.ForEach(line =>
        {
            if (line.LineIndex < TopLineIndex || line.LineIndex > BottomLineIndex) return;
            Sprite sprite = new Sprite(this.Viewport);
            sprite.Y = line.LineIndex * LineHeight;
            sprite.Bitmap = new Bitmap(line.LineWidth, LineHeight + 2);
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
                    sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), this.TextColor, this.DrawOptions);
                    if (OverlaySelectedText)
                    {
                        sprite.Bitmap.FillRect(x, 0, w, sprite.Bitmap.Height, SelectionBackgroundColor);
                        Bitmap bmp = new Bitmap(sprite.Bitmap.Width, sprite.Bitmap.Height);
                        bmp.Font = this.Font;
                        bmp.Unlock();
                        bmp.DrawText(line.Text.Replace('\n', ' '), this.TextColorSelected, this.DrawOptions);
                        bmp.Lock();
                        sprite.Bitmap.Build(x, 0, bmp, new Rect(x, 0, w, bmp.Height));
                        bmp.Dispose();
                    }
                    else
                    {
                        Sprite s = new Sprite(this.Viewport);
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
                    sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), this.TextColor, this.DrawOptions);
                    if (w > sprite.Bitmap.Width) w = sprite.Bitmap.Width;
                    if (OverlaySelectedText)
                    {
                        sprite.Bitmap.FillRect(0, 0, w, sprite.Bitmap.Height, SelectionBackgroundColor);
                        sprite.Bitmap.DrawText(line.Text.Replace('\n', ' ').Substring(0, SelectionRight.IndexInLine), this.TextColorSelected, this.DrawOptions);
                    }
                    else
                    {
                        Sprite s = new Sprite(this.Viewport);
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
                    sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), this.TextColor, this.DrawOptions);
                    if (x + w > sprite.Bitmap.Width) w = sprite.Bitmap.Width - x;
                    if (w > 0)
                    {
                        if (OverlaySelectedText)
                        {
                            sprite.Bitmap.FillRect(x, 0, w, sprite.Bitmap.Height, SelectionBackgroundColor);
                            Bitmap bmp = new Bitmap(sprite.Bitmap.Width, sprite.Bitmap.Height);
                            bmp.Font = this.Font;
                            bmp.Unlock();
                            bmp.DrawText(line.Text.Replace('\n', ' '), this.TextColorSelected, this.DrawOptions);
                            bmp.Lock();
                            sprite.Bitmap.Build(x, 0, bmp, new Rect(x, 0, w, bmp.Height));
                            bmp.Dispose();
                        }
                        else
                        {
                            Sprite s = new Sprite(this.Viewport);
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
                        sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), this.TextColorSelected, this.DrawOptions);
                    }
                    else
                    {
                        Sprite s = new Sprite(this.Viewport);
                        s.Y = sprite.Y;
                        s.Z = 1;
                        s.Bitmap = new SolidBitmap(w, sprite.Bitmap.Height, SelectionBackgroundColor);
                        SelBoxSprites.Add(s);
                        sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), this.TextColor, this.DrawOptions);
                        Sprites[$"box{line.LineIndex}"] = s;
                    }
                }
                else
                {
                    // This line is not included in any part of the selection
                    sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), this.TextColor, this.DrawOptions);
                }
            }
            else sprite.Bitmap.DrawText(line.Text.Replace('\n', ' '), this.TextColor, this.DrawOptions);
            sprite.Bitmap.Lock();
            Sprites[$"line{line.LineIndex}"] = sprite;
            LineSprites.Add(sprite);
        });
        RequireRedrawText = false;
        UpdateCaretPosition(false);
        UpdateBounds();
    }

    private void UpdateCaretPosition(bool ResetScroll)
    {
        Sprites["caret"].X = Caret.Line.WidthUpTo(Caret.IndexInLine);
        Sprites["caret"].Y = Caret.Line.LineIndex * LineHeight;
        if (ResetScroll) RequireCaretRepositioning = true;
    }

    private void ResetIdle()
    {
        Sprites["caret"].Visible = true;
        if (TimerExists("idle")) ResetTimer("idle");
    }

    private void StartSelection(int? Index = null)
    {
        SelectionStart = new CaretIndex(this);
        SelectionStart.Index = Index ?? Caret.Index;
        SelectionEnd = new CaretIndex(this);
        SelectionEnd.Index = Index ?? Caret.Index;
    }

    private void CancelSelection()
    {
        if (!HasSelection) return;
        SelectionStart = null;
        SelectionEnd = null;
        RedrawText(true);
        UpdateCaretPosition(true);
    }

    private void DeleteSelection()
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
        return this.Text.Substring(SelectionLeft.Index, SelectionRight.Index - SelectionLeft.Index);
    }

    private void SelectAll()
    {
        if (!HasSelection) StartSelection();
        SelectionStart.Index = 0;
        SelectionStart.AtEndOfLine = false;
        SelectionEnd.Index = Text.Length;
        SelectionEnd.AtEndOfLine = false;
        Caret.Index = Text.Length;
        Caret.AtEndOfLine = false;
        ResetIdle();
        RedrawText();
        UpdateCaretPosition(true);
    }

    private void MoveRight(bool Shift, bool Control)
    {
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
                Caret.Index = TextArea.FindNextCtrlIndex(this.Text, Caret.Index, false);
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
            if (Shift) RedrawText(true);
            UpdateCaretPosition(true);
        }
    }

    private void MoveLeft(bool Shift, bool Control)
    {
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
                Caret.Index = TextArea.FindNextCtrlIndex(this.Text, Caret.Index, true);
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
            if (Shift) RedrawText();
            UpdateCaretPosition(true);
        }
    }

    private void MoveUp(bool Shift)
    {
        if (!Shift && HasSelection) CancelSelection();
        int OldIndex = Caret.Index;
        if (Caret.Line.LineIndex > 0)
        {
            int CurWidth = Caret.Line.WidthUpTo(Caret.IndexInLine);
            Line l = Lines[Caret.Line.LineIndex - 1];
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
                CaretIndex? SelectionCaret = SelectionEnd;
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
            if (Shift) RedrawText();
            UpdateCaretPosition(true);
        }
    }

    private void MoveDown(bool Shift)
    {
        if (!Shift && HasSelection) CancelSelection();
        int OldIndex = Caret.Index;
        if (Caret.Line.LineIndex < Lines.Count - 1)
        {
            int CurWidth = Caret.Line.WidthUpTo(Caret.IndexInLine);
            Line l = Lines[Caret.Line.LineIndex + 1];
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
                CaretIndex? SelectionCaret = SelectionStart;
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
            if (Shift) RedrawText();
            UpdateCaretPosition(true);
        }
    }

    private void MovePageUp(bool Shift)
    {
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
        Caret.AtEndOfLine = false;
        if (OldIndex != Caret.Index)
        {
            if (Shift)
            {
                if (!HasSelection) StartSelection(OldIndex);
                CaretIndex? SelectionCaret = SelectionEnd;
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
            if (Shift) RedrawText();
            UpdateCaretPosition(true);
        }
    }

    private void MovePageDown(bool Shift)
    {
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
        if (OldIndex != Caret.Index)
        {
            if (Shift)
            {
                if (!HasSelection) StartSelection(OldIndex);
                CaretIndex? SelectionCaret = SelectionStart;
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
            if (Shift) RedrawText();
            UpdateCaretPosition(true);
        }
    }

    private void MoveHome(bool Shift)
    {
        if (!Shift && HasSelection) CancelSelection();
        if (Caret.IndexInLine > 0)
        {
            if (Shift && !HasSelection) StartSelection();
            int OldIndex = Caret.Index;
            Caret.Index = Caret.Line.StartIndex;
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
            if (Shift) RedrawText();
            UpdateCaretPosition(true);
        }
    }

    private void MoveEnd(bool Shift)
    {
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
            if (Shift) RedrawText();
            UpdateCaretPosition(true);
        }
    }

    private void InsertText(int Index, string Text)
    {
        Text = Text.Replace("\r", "");
        if (Text.Length == 0) return;
        if (HasSelection)
        {
            int count = SelectionRight.Index - SelectionLeft.Index;
            if (Index > SelectionLeft.Index) Index -= count;
            DeleteSelection();
        }
        this.Text = this.Text.Insert(Index, Text);
        if (Index <= Caret.Index) Caret.Index += Text.Length;
        if (Text == "\n") Caret.AtEndOfLine = false;
        ResetIdle();
        RecalculateLines();
    }

    private void RemoveText(int Index, int Count)
    {
        if (Index < 0) return;
        Count = Math.Min(Text.Length - Index, Count);
        if (Count < 1) return;
        Text = Text.Remove(Index, Count);
        if (Index <= Caret.Index) Caret.Index = Math.Max(Index, Caret.Index - Count);
        Caret.AtEndOfLine = !Caret.Line.EndsInNewline && Caret.Index == Caret.Line.EndIndex + 1;
        ResetIdle();
        RecalculateLines();
    }

    private void CutSelection()
    {
        if (!HasSelection) return;
        CopySelection();
        DeleteSelection();
    }

    private void CopySelection()
    {
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
        base.TextInput(e);
        string text = this.Text;
        if (!string.IsNullOrEmpty(e.Text))
        {
            InsertText(Caret.Index, e.Text);
        }
        else if (e.Backspace || e.Delete)
        {
            if (HasSelection) DeleteSelection();
            else
            {
                if (e.Delete)
                {
                    if (Caret.Index < this.Text.Length)
                    {
                        int Count = 1;
                        if (Input.Press(Keycode.CTRL)) Count = TextArea.FindNextCtrlIndex(this.Text, this.Caret.Index, false) - Caret.Index;
                        RemoveText(this.Caret.Index, Count);
                    }
                }
                else
                {
                    int Count = 1;
                    if (Input.Press(Keycode.CTRL)) Count = Caret.Index - TextArea.FindNextCtrlIndex(this.Text, this.Caret.Index, true);
                    RemoveText(this.Caret.Index - Count, Count);
                }
            }
        }
        if (this.Text != text) OnTextChanged?.Invoke(new BaseEventArgs());
        RecalculateLines();
    }

    private class Line
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

        public void AddCharacter(char c, Size s)
        {
            EndsInNewline = c == '\n';
            Text += c;
            CharacterWidths.Add(s.Width);
            Length++;
        }

        public void AddText(string Text)
        {
            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '\r') continue;
                char c = Text[i];
                Size s = Font.TextSize(c);
                AddCharacter(Text[i], s);
            }
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
                int w = Font.TextSize(Text.Substring(0, i)).Width;
                if (w >= Width) return i;
            }
            return Text.Length - (EndsInNewline ? 1 : 0);
        }
    }

    private class CaretIndex
    {
        private NewMultilineTextArea TextArea;

        public Line Line => TextArea.Lines.Find(l => Index - (AtEndOfLine ? 1 : 0) >= l.StartIndex && Index - (AtEndOfLine ? 1 : 0) <= l.EndIndex) ?? TextArea.Lines.Last();
        public int Index;
        public int IndexInLine => Index - Line.StartIndex;
        public bool AtEndOfLine = false;

        public CaretIndex(NewMultilineTextArea TextArea) 
        {
            this.TextArea = TextArea;
        }
    }
}
