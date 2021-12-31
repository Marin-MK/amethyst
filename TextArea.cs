using System;
using System.Collections.Generic;
using odl;
using static odl.SDL2.SDL;

namespace amethyst;

public class TextArea : Widget
{
    public string Text { get; protected set; } = "";
    public int TextY { get; protected set; } = 0;
    public int CaretY { get; protected set; } = 2;
    public int CaretHeight { get; protected set; } = 13;
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.WHITE;
    public Color DisabledTextColor { get; protected set; } = new Color(120, 120, 120);
    public Color CaretColor { get; protected set; } = Color.BLACK;
    public Color FillerColor { get; protected set; } = new Color(0, 120, 215);
    public bool ReadOnly { get; protected set; } = false;
    public bool Enabled { get; protected set; } = true;

    public bool EnteringText = false;

    public int X;
    public int RX;
    public int Width;

    public int CaretIndex = 0;

    public int SelectionStartIndex = -1;
    public int SelectionEndIndex = -1;

    public int SelectionStartX = -1;

    public TextEvent OnTextChanged;

    List<TextAreaState> UndoList = new List<TextAreaState>();
    List<TextAreaState> RedoList = new List<TextAreaState>();
    bool UndoingOrRedoing = false;

    public TextArea(IContainer Parent) : base(Parent)
    {
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].Z = 2;
        Sprites["filler"] = new Sprite(this.Viewport, new SolidBitmap(1, 16, FillerColor));
        Sprites["filler"].Visible = false;
        Sprites["filler"].Y = 2;
        Sprites["caret"] = new Sprite(this.Viewport, new SolidBitmap(1, 16, CaretColor));
        Sprites["caret"].Y = 2;
        Sprites["caret"].Z = 1;
        OnWidgetSelected += WidgetSelected;
        OnDisposed += delegate (BaseEventArgs e)
        {
            this.Window.UI.SetSelectedWidget(null);
            Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        };
        OnTextChanged += delegate (TextEventArgs e)
        {
            if (!UndoingOrRedoing)
            {
                RedoList.Clear();
                UndoList.Add(GetState());
            }
        };
    }

    public void SetCaretColor(Color CaretColor)
    {
        if (this.CaretColor != CaretColor)
        {
            this.CaretColor = CaretColor;
            ((SolidBitmap)Sprites["caret"].Bitmap).SetColor(CaretColor);
        }
    }

    public void SetDisabledTextColor(Color DisabledTextColor)
    {
        if (this.DisabledTextColor != DisabledTextColor)
        {
            this.DisabledTextColor = DisabledTextColor;
            this.DrawText();
        }
    }

    public void SetFillercolor(Color FillerColor)
    {
        if (this.FillerColor != FillerColor)
        {
            this.FillerColor = FillerColor;
            ((SolidBitmap)Sprites["filler"].Bitmap).SetColor(FillerColor);
        }
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            this.DrawText();
            if (this.SelectedWidget) Window.UI.SetSelectedWidget(null);
        }
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            string OldText = this.Text;
            this.Text = Text ?? "";
            X = 0;
            RX = 0;
            CaretIndex = 0;
            DrawText();
            int width = Sprites["text"].Bitmap.Width;
            int inviswidth = width - Size.Width;
            if (inviswidth > 0) X = inviswidth;
            RX = inviswidth > 0 ? Size.Width - 1 : width;
            CaretIndex = this.Text.Length;
            this.OnTextChanged?.Invoke(new TextEventArgs(this.Text, OldText));
        }
    }

    public void SetFont(Font f)
    {
        this.Font = f;
        DrawText();
    }

    public void SetTextY(int TextY)
    {
        this.TextY = TextY;
        Sprites["text"].Y = TextY;
    }

    public void SetCaretY(int CaretY)
    {
        this.CaretY = CaretY;
        Sprites["caret"].Y = Sprites["filler"].Y = CaretY;
    }

    public void SetCaretHeight(int CaretHeight)
    {
        this.CaretHeight = CaretHeight;
        SolidBitmap caret = Sprites["caret"].Bitmap as SolidBitmap;
        caret.SetSize(1, CaretHeight);
        SolidBitmap filler = Sprites["filler"].Bitmap as SolidBitmap;
        filler.SetSize(filler.BitmapWidth, CaretHeight);
    }

    public void SetTextColor(Color TextColor)
    {
        if (this.TextColor != TextColor)
        {
            this.TextColor = TextColor;
            DrawText();
        }
    }

    public void SetReadOnly(bool ReadOnly)
    {
        if (this.ReadOnly != ReadOnly)
        {
            this.ReadOnly = ReadOnly;
            if (this.ReadOnly)
            {
                EnteringText = false;
                Input.StopTextInput();
                CancelSelectionHidden();
            }
            else
            {
                if (this.SelectedWidget)
                {
                    EnteringText = true;
                    Input.StartTextInput();
                    SetTimer("idle", 400);
                }
            }
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        this.Width = Size.Width;
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
        if (SelectionStartIndex != -1) CancelSelectionHidden();
    }

    public override void TextInput(TextEventArgs e)
    {
        base.TextInput(e);
        string text = this.Text;
        if (e.Text == "\n")
        {
            Window.UI.SetSelectedWidget(null);
            return;
        }
        else if (!string.IsNullOrEmpty(e.Text))
        {
            if (SelectionStartIndex != -1 && SelectionStartIndex != SelectionEndIndex) DeleteSelection();
            InsertText(CaretIndex, e.Text);
        }
        else if (e.Backspace || e.Delete)
        {
            if (SelectionStartIndex != -1 && SelectionStartIndex != SelectionEndIndex)
            {
                DeleteSelection();
            }
            else
            {
                if (SelectionStartIndex == SelectionEndIndex) CancelSelectionHidden();
                if (e.Delete)
                {
                    if (CaretIndex < this.Text.Length)
                    {
                        int Count = 1;
                        if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
                            Count = FindNextCtrlIndex(false) - CaretIndex;
                        MoveCaretRight(Count);
                        RemoveText(this.CaretIndex - Count, Count);
                    }
                }
                else
                {
                    int Count = 1;
                    if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
                        Count = CaretIndex - FindNextCtrlIndex(true);
                    RemoveText(this.CaretIndex - Count, Count);
                }
            }
        }
        if (this.Text != text)
        {
            this.OnTextChanged?.Invoke(new TextEventArgs(this.Text, text));
        }
        DrawText();
    }

    /// <summary>
    /// Inserts text to the left of the caret.
    /// </summary>
    /// <param name="InsertionIndex">The index at which to insert text.</param>
    /// <param name="Text">The text to insert.</param>
    public void InsertText(int InsertionIndex, string Text)
    {
        if (this.ReadOnly) return;
        while (Text.Contains("\n")) Text = Text.Replace("\n", "");
        if (Text.Length == 0) return;
        int charw = Font.TextSize(this.Text.Substring(0, InsertionIndex) + Text).Width - Font.TextSize(this.Text.Substring(0, InsertionIndex)).Width;

        if (RX + charw >= Width)
        {
            int left = Width - RX;
            RX += left;
            X += charw - left;
        }
        else
        {
            RX += charw;
        }
        this.Text = this.Text.Insert(InsertionIndex, Text);
        this.CaretIndex += Text.Length;
        ResetIdle();
    }

    /// <summary>
    /// Deletes text to the left of the caret.
    /// </summary>
    /// <param name="StartIndex">Starting index of the range to delete.</param>
    /// <param name="Count">Number of characters to delete.</param>
    public void RemoveText(int StartIndex, int Count = 1)
    {
        if (this.ReadOnly) return;
        if (this.Text.Length == 0 || StartIndex < 0 || StartIndex >= this.Text.Length) return;
        string TextIncluding = this.Text.Substring(0, StartIndex + Count);
        int charw = Font.TextSize(TextIncluding).Width - Font.TextSize(this.Text.Substring(0, StartIndex)).Width;
        int dRX = Math.Min(RX, charw);
        if (RX - charw < 0)
        {
            X -= charw - RX;
            RX = 0;
        }
        else
        {
            RX -= charw;
        }
        int exwidth = Sprites["text"].Bitmap.Width - Width - X - charw;
        if (exwidth < 0)
        {
            RX += Math.Min(X, Math.Abs(exwidth));
            X -= Math.Min(X, Math.Abs(exwidth));
        }
        CaretIndex -= Count;
        this.Text = this.Text.Remove(StartIndex, Count);
        ResetIdle();
    }

    /// <summary>
    /// Deletes the content inside the selection.
    /// </summary>
    public void DeleteSelection()
    {
        if (this.ReadOnly) return;
        int startidx = SelectionStartIndex > SelectionEndIndex ? SelectionEndIndex : SelectionStartIndex;
        int endidx = SelectionStartIndex > SelectionEndIndex ? SelectionStartIndex : SelectionEndIndex;
        CancelSelectionRight();
        RemoveText(startidx, endidx - startidx);
        ResetIdle();
    }

    /// <summary>
    /// Handles input for various keys.
    /// </summary>
    public override void Update()
    {
        base.Update();

        if (SelectedWidget && !this.Enabled)
        {
            Window.UI.SetSelectedWidget(null);
        }

        if (!SelectedWidget)
        {
            if (TimerExists("double")) DestroyTimer("double");
            if (TimerExists("left")) DestroyTimer("left");
            if (TimerExists("left_initial")) DestroyTimer("left_initial");
            if (TimerExists("right")) DestroyTimer("right");
            if (TimerExists("right_initial")) DestroyTimer("right_initial");
            if (TimerExists("paste")) DestroyTimer("paste");
            if (TimerExists("paste_initial")) DestroyTimer("paste_initial");
            if (TimerExists("undo")) DestroyTimer("undo");
            if (TimerExists("undo_initial")) DestroyTimer("undo_initial");
            if (TimerExists("redo")) DestroyTimer("redo");
            if (TimerExists("redo_initial")) DestroyTimer("redo_initial");
            if (EnteringText) WidgetDeselected(new BaseEventArgs());
            if (Sprites["caret"].Visible) Sprites["caret"].Visible = false;
            return;
        }

        if (Input.Trigger(SDL_Keycode.SDLK_LEFT) || TimerPassed("left"))
        {
            if (TimerPassed("left")) ResetTimer("left");
            if (CaretIndex > 0)
            {
                int Count = 1;
                if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
                {
                    Count = CaretIndex - FindNextCtrlIndex(true);
                }

                if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                {
                    if (SelectionStartIndex == -1)
                    {
                        SelectionStartX = X + RX;
                        SelectionStartIndex = CaretIndex;
                    }
                    MoveCaretLeft(Count);
                    SelectionEndIndex = CaretIndex;
                    RepositionSprites();
                }
                else
                {
                    if (SelectionStartIndex != -1)
                    {
                        CancelSelectionLeft();
                    }
                    else
                    {
                        MoveCaretLeft(Count);
                        RepositionSprites();
                    }
                }
            }
            else if (SelectionStartIndex != -1 && !(Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT)))
            {
                CancelSelectionLeft();
            }
        }
        if (Input.Trigger(SDL_Keycode.SDLK_RIGHT) || TimerPassed("right"))
        {
            if (TimerPassed("right")) ResetTimer("right");
            if (CaretIndex < this.Text.Length)
            {
                int Count = 1;
                if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
                {
                    Count = FindNextCtrlIndex(false) - CaretIndex;
                }

                if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
                {
                    if (SelectionStartIndex == -1)
                    {
                        SelectionStartX = X + RX;
                        SelectionStartIndex = CaretIndex;
                    }
                    MoveCaretRight(Count);
                    SelectionEndIndex = CaretIndex;
                    RepositionSprites();
                }
                else
                {
                    if (SelectionStartIndex != -1)
                    {
                        CancelSelectionRight();
                    }
                    else
                    {
                        MoveCaretRight(Count);
                        RepositionSprites();
                    }
                }
            }
            else if (SelectionStartIndex != -1 && !(Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT)))
            {
                CancelSelectionRight();
            }
        }
        if (Input.Trigger(SDL_Keycode.SDLK_HOME))
        {
            if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
            {
                if (SelectionStartIndex != -1) SelectionEndIndex = 0;
                else
                {
                    SelectionStartIndex = CaretIndex;
                    SelectionStartX = X + RX;
                    SelectionEndIndex = 0;
                }
            }
            else CancelSelectionLeft();
            MoveCaretLeft(CaretIndex);
            RepositionSprites();
        }
        if (Input.Trigger(SDL_Keycode.SDLK_END))
        {
            if (Input.Press(SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL_Keycode.SDLK_RSHIFT))
            {
                if (SelectionStartIndex != -1) SelectionEndIndex = this.Text.Length;
                else
                {
                    SelectionStartIndex = CaretIndex;
                    SelectionStartX = X + RX;
                    SelectionEndIndex = this.Text.Length;
                }
            }
            else CancelSelectionRight();
            MoveCaretRight(this.Text.Length - CaretIndex);
            RepositionSprites();
        }
        if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
        {
            if (Input.Trigger(SDL_Keycode.SDLK_a))
            {
                SelectAll();
            }
            if (Input.Trigger(SDL_Keycode.SDLK_x))
            {
                CutSelection();
            }
            if (Input.Trigger(SDL_Keycode.SDLK_c))
            {
                CopySelection();
            }
            if (Input.Trigger(SDL_Keycode.SDLK_v) || TimerPassed("paste"))
            {
                PasteText();
            }
            if (Input.Trigger(SDL_Keycode.SDLK_z) || TimerPassed("undo"))
            {
                UndoText();
            }
            if (!Input.Press(SDL_Keycode.SDLK_z) && (Input.Trigger(SDL_Keycode.SDLK_y) || TimerPassed("redo")))
            {
                RedoText();
            }
        }

        // Timers for repeated input
        if (Input.Press(SDL_Keycode.SDLK_LEFT))
        {
            if (!TimerExists("left_initial") && !TimerExists("left"))
            {
                SetTimer("left_initial", 300);
            }
            else if (TimerPassed("left_initial"))
            {
                DestroyTimer("left_initial");
                SetTimer("left", 50);
            }
        }
        else
        {
            if (TimerExists("left")) DestroyTimer("left");
            if (TimerExists("left_initial")) DestroyTimer("left_initial");
        }
        if (Input.Press(SDL_Keycode.SDLK_RIGHT))
        {
            if (!TimerExists("right_initial") && !TimerExists("right"))
            {
                SetTimer("right_initial", 300);
            }
            else if (TimerPassed("right_initial"))
            {
                DestroyTimer("right_initial");
                SetTimer("right", 50);
            }
        }
        else
        {
            if (TimerExists("right")) DestroyTimer("right");
            if (TimerExists("right_initial")) DestroyTimer("right_initial");
        }
        if (Input.Press(SDL_Keycode.SDLK_LCTRL) || Input.Press(SDL_Keycode.SDLK_RCTRL))
        {
            if (Input.Press(SDL_Keycode.SDLK_v))
            {
                if (!TimerExists("paste_initial") && !TimerExists("paste"))
                {
                    SetTimer("paste_initial", 300);
                }
                else if (TimerPassed("paste_initial"))
                {
                    DestroyTimer("paste_initial");
                    SetTimer("paste", 50);
                }
            }
            else
            {
                if (TimerExists("paste")) DestroyTimer("paste");
                if (TimerExists("paste_initial")) DestroyTimer("paste_initial");
            }
            if (Input.Press(SDL_Keycode.SDLK_z))
            {
                if (!TimerExists("undo_initial") && !TimerExists("undo"))
                {
                    SetTimer("undo_initial", 300);
                }
                else if (TimerPassed("undo_initial"))
                {
                    DestroyTimer("undo_initial");
                    SetTimer("undo", 50);
                }
            }
            else
            {
                if (TimerExists("undo")) DestroyTimer("undo");
                if (TimerExists("undo_initial")) DestroyTimer("undo_initial");
            }
            if (Input.Press(SDL_Keycode.SDLK_y) && !Input.Press(SDL_Keycode.SDLK_z))
            {
                if (!TimerExists("redo_initial") && !TimerExists("redo"))
                {
                    SetTimer("redo_initial", 300);
                }
                else if (TimerPassed("redo_initial"))
                {
                    DestroyTimer("redo_initial");
                    SetTimer("redo", 50);
                }
            }
            else
            {
                if (TimerExists("redo")) DestroyTimer("redo");
                if (TimerExists("redo_initial")) DestroyTimer("redo_initial");
            }
        }
        else
        {
            if (TimerExists("undo")) DestroyTimer("undo");
            if (TimerExists("undo_initial")) DestroyTimer("undo_initial");
            if (TimerExists("redo")) DestroyTimer("redo");
            if (TimerExists("redo_initial")) DestroyTimer("redo_initial");
        }

        if (TimerPassed("double")) DestroyTimer("double");

        if (TimerPassed("idle"))
        {
            Sprites["caret"].Visible = !Sprites["caret"].Visible;
            ResetTimer("idle");
        }

        if (this.ReadOnly) Sprites["caret"].Visible = false;
    }

    /// <summary>
    /// Resets the idle timer, which pauses the caret blinking.
    /// </summary>
    public void ResetIdle()
    {
        Sprites["caret"].Visible = this.Enabled;
        if (TimerExists("idle")) ResetTimer("idle");
    }

    /// <summary>
    /// Finds the next word that could be skipped to with control.
    /// </summary>
    /// <param name="Left">Whether to search to the left or right of the caret.</param>
    /// <returns>The next index to jump to when holding control.</returns>
    public int FindNextCtrlIndex(bool Left) // or false for Right
    {
        int idx = 0;
        string splitters = " `~!@#$%^&*()-=+[]{}\\|;:'\",.<>/?\n";
        bool found = false;
        if (Left)
        {
            for (int i = CaretIndex - 1; i >= 0; i--)
            {
                if (splitters.Contains(this.Text[i]) && i != CaretIndex - 1)
                {
                    idx = i + 1;
                    found = true;
                    break;
                }
            }
            if (!found) idx = 0;
        }
        else
        {
            for (int i = CaretIndex + 1; i < this.Text.Length; i++)
            {
                if (splitters.Contains(this.Text[i]))
                {
                    idx = i;
                    found = true;
                    break;
                }
            }
            if (!found) idx = this.Text.Length;
        }
        return idx;
    }

    /// <summary>
    /// Cancels the selection and puts the caret on the left.
    /// </summary>
    public void CancelSelectionLeft()
    {
        if (SelectionEndIndex > SelectionStartIndex)
        {
            if (SelectionStartX < X)
            {
                X = SelectionStartX;
                RX = 0;
            }
            else
            {
                RX = SelectionStartX - X;
            }
            CaretIndex -= SelectionEndIndex - SelectionStartIndex;
            RepositionSprites();
        }
        SelectionStartIndex = -1;
        SelectionEndIndex = -1;
        SelectionStartX = -1;
        Sprites["filler"].Visible = false;
    }

    /// <summary>
    /// Cancels the selection and puts the caret on the right.
    /// </summary>
    public void CancelSelectionRight()
    {
        if (SelectionStartIndex > SelectionEndIndex)
        {
            if (SelectionStartX > X + Width)
            {
                X += SelectionStartX - X - Width;
                RX = Width;
            }
            else
            {
                RX = SelectionStartX - X;
            }
            CaretIndex += SelectionStartIndex - SelectionEndIndex;
            RepositionSprites();
        }
        SelectionStartIndex = -1;
        SelectionEndIndex = -1;
        SelectionStartX = -1;
        Sprites["filler"].Visible = false;
    }

    /// <summary>
    /// Cancels the selection without updating the caret.
    /// </summary>
    public void CancelSelectionHidden()
    {
        SelectionStartIndex = -1;
        SelectionEndIndex = -1;
        SelectionStartX = -1;
        Sprites["filler"].Visible = false;
    }

    /// <summary>
    /// Moves the caret to the left.
    /// </summary>
    /// <param name="Count">The number of characters to skip.</param>
    public void MoveCaretLeft(int Count = 1)
    {
        if (this.ReadOnly) return;
        if (CaretIndex - Count < 0) return;
        string TextToCaret = this.Text.Substring(0, CaretIndex);
        int charw = Font.TextSize(TextToCaret).Width - Font.TextSize(TextToCaret.Substring(0, TextToCaret.Length - Count)).Width;
        if (RX - charw < 0)
        {
            X -= charw - RX;
            RX = 0;
        }
        else
        {
            RX -= charw;
        }
        CaretIndex -= Count;
        ResetIdle();
    }

    /// <summary>
    /// Moves the caret to the right.
    /// </summary>
    /// <param name="Count">The number of characters to skip.</param>
    public void MoveCaretRight(int Count = 1)
    {
        if (this.ReadOnly) return;
        if (CaretIndex + Count > this.Text.Length) return;
        string TextToCaret = this.Text.Substring(0, CaretIndex);
        string TextToCaretPlusOne = this.Text.Substring(0, CaretIndex + Count);
        int charw = Font.TextSize(TextToCaretPlusOne).Width - Font.TextSize(TextToCaret).Width;
        if (RX + charw >= Width)
        {
            int left = Width - RX;
            RX += left;
            X += charw - left;
        }
        else
        {
            RX += charw;
        }
        CaretIndex += Count;
        ResetIdle();
    }

    /// <summary>
    /// Determines key values based on the given mouse position.
    /// </summary>
    /// <returns>List<int>() { RX, X, found }</int></returns>
    public List<int> GetMousePosition(MouseEventArgs e)
    {
        int RetRX = RX;
        int RetCaretIndex = CaretIndex;
        int Found = 0;
        int rmx = e.X - Viewport.X;
        if (rmx < 0 || rmx >= Width) return null;
        for (int i = 0; i < this.Text.Length; i++)
        {
            int fullwidth = Font.TextSize(this.Text.Substring(0, i)).Width;
            int charw = Font.TextSize(this.Text.Substring(0, i + 1)).Width - Font.TextSize(this.Text.Substring(0, i)).Width;
            int rx = fullwidth - X;
            if (rx >= 0 && rx < Width)
            {
                if (rmx >= rx && rmx < rx + charw)
                {
                    int diff = rx + charw - rmx;
                    if (diff > charw / 2)
                    {
                        RetRX = rx;
                        RetCaretIndex = i;
                    }
                    else
                    {
                        RetRX = rx + charw;
                        RetCaretIndex = i + 1;
                    }
                    Found = 1;
                    break;
                }
            }
        }
        return new List<int>() { RetRX, RetCaretIndex, Found };
    }

    /// <summary>
    /// Redraws the text bitmap.
    /// </summary>
    public void DrawText()
    {
        RepositionSprites();
        Sprites["text"].Bitmap?.Dispose();
        if (string.IsNullOrEmpty(this.Text)) return;
        Size s = Font.TextSize(this.Text);
        if (s.Width < 1 || s.Height < 1) return;
        Sprites["text"].Bitmap = new Bitmap(s);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = this.Font;
        if (this.Enabled) Sprites["text"].Bitmap.DrawText(this.Text, this.Enabled ? this.TextColor : DisabledTextColor);
        Sprites["text"].Bitmap.Lock();
    }

    /// <summary>
    /// Repositions the text, caret and selection sprites.
    /// </summary>
    public void RepositionSprites()
    {
        Sprites["text"].X = -X;
        int add = 1;
        if (this.Text.Length > 0 && CaretIndex > 0 && this.Text[CaretIndex - 1] == ' ' && RX != Width) add = 0;
        Sprites["caret"].X = Math.Max(0, RX - add);

        // Selections
        if (SelectionStartIndex > SelectionEndIndex)
        {
            if (X + Width < SelectionStartX)
            {
                Sprites["filler"].X = RX;
                (Sprites["filler"].Bitmap as SolidBitmap).SetSize(Width - RX, CaretHeight);
                Sprites["filler"].Visible = true;
            }
            else
            {
                Sprites["filler"].X = RX;
                (Sprites["filler"].Bitmap as SolidBitmap).SetSize(SelectionStartX - X - RX, CaretHeight);
                Sprites["filler"].Visible = true;
            }
        }
        else if (SelectionStartIndex < SelectionEndIndex)
        {
            if (SelectionStartX < X)
            {
                Sprites["filler"].X = 0;
                (Sprites["filler"].Bitmap as SolidBitmap).SetSize(RX, CaretHeight);
                Sprites["filler"].Visible = true;
            }
            else
            {
                Sprites["filler"].X = SelectionStartX - X;
                (Sprites["filler"].Bitmap as SolidBitmap).SetSize(X + RX - SelectionStartX, CaretHeight);
                Sprites["filler"].Visible = true;
            }
        }
        else
        {
            if (SelectionStartIndex != -1)
            {
                Sprites["filler"].Visible = false;
            }
        }
    }

    /// <summary>
    /// Selects all text.
    /// </summary>
    public void SelectAll()
    {
        if (this.ReadOnly) return;
        MoveCaretRight(this.Text.Length - CaretIndex);
        SelectionStartIndex = 0;
        SelectionEndIndex = this.Text.Length;
        SelectionStartX = 0;
        RepositionSprites();
    }

    /// <summary>
    /// Copies the selected text to the clipboard and deletes the selection.
    /// </summary>
    public void CutSelection()
    {
        if (this.ReadOnly) return;
        if (SelectionStartIndex != -1)
        {
            int startidx = SelectionStartIndex > SelectionEndIndex ? SelectionEndIndex : SelectionStartIndex;
            int endidx = SelectionStartIndex > SelectionEndIndex ? SelectionStartIndex : SelectionEndIndex;
            string text = this.Text.Substring(startidx, endidx - startidx);
            Input.SetClipboard(text);
            DeleteSelection();
            DrawText();
        }
    }

    /// <summary>
    /// Copies the selected text to the clipboard.
    /// </summary>
    public void CopySelection()
    {
        if (SelectionStartIndex != -1)
        {
            int startidx = SelectionStartIndex > SelectionEndIndex ? SelectionEndIndex : SelectionStartIndex;
            int endidx = SelectionStartIndex > SelectionEndIndex ? SelectionStartIndex : SelectionEndIndex;
            string text = this.Text.Substring(startidx, endidx - startidx);
            Input.SetClipboard(text);
        }
    }

    /// <summary>
    /// Pastes text from the clipboard to the text field.
    /// </summary>
    public void PasteText()
    {
        if (this.ReadOnly) return;
        if (TimerPassed("paste")) ResetTimer("paste");
        string text = Input.GetClipboard();
        if (SelectionStartIndex != -1 && SelectionStartIndex != SelectionEndIndex) DeleteSelection();
        InsertText(CaretIndex, text);
        DrawText();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!WidgetIM.Hovering || this.Text.Length == 0 || this.ReadOnly || !this.Enabled) return;
        if (SelectionStartIndex != -1 && SelectionStartIndex != SelectionEndIndex) CancelSelectionHidden();
        int OldRX = RX;
        int OldCaretIndex = CaretIndex;
        List<int> newvals = GetMousePosition(e);
        RX = newvals[0];
        CaretIndex = newvals[1];
        bool found = newvals[2] == 1;
        if (!found)
        {
            if (Sprites["text"].Bitmap.Width - X < Width) // No extra space to the right that could be scrolled to
            {
                RX = Sprites["text"].Bitmap.Width;
                CaretIndex = this.Text.Length;
            }
        }
        RepositionSprites();
        if (!TimerExists("double"))
        {
            SetTimer("double", 300);
        }
        else if (!TimerPassed("double"))
        {
            // Double clicked
            DoubleClick();
            DestroyTimer("double");
        }
    }

    public void DoubleClick()
    {
        int startindex = FindNextCtrlIndex(true);
        int endindex = FindNextCtrlIndex(false);
        if (endindex - startindex > 0)
        {
            SelectionStartIndex = startindex;
            SelectionEndIndex = endindex;
            MoveCaretLeft(CaretIndex - startindex);
            SelectionStartX = RX + X;
            MoveCaretRight(endindex - CaretIndex);
            RepositionSprites();
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (!e.LeftButton || WidgetIM.ClickedLeftInArea != true || this.ReadOnly || !this.Enabled) return;
        int OldRX = RX;
        int OldCaretIndex = CaretIndex;
        int rmx = e.X - Viewport.X;
        if (rmx >= Width && Sprites["text"].Bitmap.Width - X >= Width)
        {
            MoveCaretRight();
            SelectionEndIndex = CaretIndex;
            RepositionSprites();
            return;
        }
        else if (rmx < 0 && X > 0)
        {
            MoveCaretLeft();
            SelectionEndIndex = CaretIndex;
            RepositionSprites();
            return;
        }
        List<int> newvals = GetMousePosition(e);
        if (newvals == null)
        {
            if (rmx < 0)
            {
                MoveCaretLeft();
                SelectionEndIndex = CaretIndex;
                RepositionSprites();
            }
            return;
        }
        RX = newvals[0];
        CaretIndex = newvals[1];
        bool found = newvals[2] == 1;
        if (found && CaretIndex != OldCaretIndex)
        {
            if (SelectionStartIndex == -1)
            {
                SelectionStartIndex = OldCaretIndex;
                SelectionStartX = OldRX + X;
            }
            SelectionEndIndex = CaretIndex;
            RepositionSprites();
        }
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        if (WidgetIM.Hovering && Enabled && !ReadOnly)
        {
            Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM);
        }
        else
        {
            Input.SetCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        }
    }

    void UndoText()
    {
        if (this.ReadOnly) return;
        if (TimerPassed("undo")) ResetTimer("undo");
        if (UndoList.Count > 1)
        {
            string OldText = this.Text;
            TextAreaState NewState = UndoList[UndoList.Count - 2];
            SetState(NewState);
            TextAreaState OldState = UndoList[UndoList.Count - 1];
            UndoList.RemoveAt(UndoList.Count - 1);
            RedoList.Add(OldState);
            UndoingOrRedoing = true;
            OnTextChanged?.Invoke(new TextEventArgs(this.Text, OldText));
            UndoingOrRedoing = false;
        }
    }

    void RedoText()
    {
        if (this.ReadOnly) return;
        if (TimerPassed("redo")) ResetTimer("redo");
        if (RedoList.Count > 0)
        {
            string OldText = this.Text;
            TextAreaState NewState = RedoList[RedoList.Count - 1];
            SetState(NewState);
            RedoList.RemoveAt(RedoList.Count - 1);
            UndoList.Add(NewState);
            UndoingOrRedoing = true;
            OnTextChanged?.Invoke(new TextEventArgs(this.Text, OldText));
            UndoingOrRedoing = false;
        }
    }

    public TextAreaState GetState()
    {
        return new TextAreaState(Text, X, RX, Width, CaretIndex,
            SelectionStartIndex, SelectionEndIndex, SelectionStartX);
    }

    public void SetState(TextAreaState State)
    {
        this.Text = State.Text;
        this.X = State.X;
        this.RX = State.RX;
        this.Width = State.Width;
        this.CaretIndex = State.CaretIndex;
        this.SelectionStartIndex = State.SelectionStartIndex;
        this.SelectionEndIndex = State.SelectionEndIndex;
        this.SelectionStartX = State.SelectionStartX;
        DrawText();
        RepositionSprites();
    }
}

public class TextAreaState
{
    public string Text;
    public int X;
    public int RX;
    public int Width;
    public int CaretIndex;
    public int SelectionStartIndex;
    public int SelectionEndIndex;
    public int SelectionStartX;

    public TextAreaState(string Text, int X, int RX, int Width, int CaretIndex,
        int SelectionStartIndex, int SelectionEndIndex, int SelectionStartX)
    {
        this.Text = Text;
        this.X = X;
        this.RX = RX;
        this.Width = Width;
        this.CaretIndex = CaretIndex;
        this.SelectionStartIndex = SelectionStartIndex;
        this.SelectionEndIndex = SelectionEndIndex;
        this.SelectionStartX = SelectionStartX;
    }

    public override string ToString()
    {
        return this.Text;
    }
}