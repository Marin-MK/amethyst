﻿using System;
using System.Collections.Generic;
using odl;


namespace amethyst;

public class TextArea : Widget
{
    public string Text { get; protected set; } = "";
    public int TextX { get; protected set; } = 0;
    public int TextY { get; protected set; } = 0;
    public int CaretY { get; protected set; } = 2;
    public int CaretHeight { get; protected set; } = 13;
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.WHITE;
    public Color DisabledTextColor { get; protected set; } = new Color(141, 151, 163);
    public Color CaretColor { get; protected set; } = Color.WHITE;
    public Color FillerColor { get; protected set; } = new Color(0, 120, 215);
    public bool ReadOnly { get; protected set; } = false;
    public bool Enabled { get; protected set; } = true;
    public bool NumericOnly { get; protected set; } = false;
    public bool AllowFloats { get; protected set; } = false;
    public float DefaultNumericValue { get; protected set; } = 0;
    public bool AllowMinusSigns { get; protected set; } = true;
    public bool ShowDisabledText { get; protected set; } = false;
    public bool DeselectOnEnterPressed { get; protected set; } = true;

    public bool EnteringText = false;

    public int X;
    public int RX;
    public int Width;

    public int CaretIndex = 0;

    public int SelectionStartIndex = -1;
    public int SelectionEndIndex = -1;

    public int SelectionStartX = -1;

    public TextEvent OnTextChanged;
    public BaseEvent OnPressingUp;
    public BaseEvent OnPressingDown;
    public BaseEvent OnEnterPressed;

    List<TextAreaState> UndoList = new List<TextAreaState>();
    List<TextAreaState> RedoList = new List<TextAreaState>();
    bool UndoingOrRedoing = false;

    public TextArea(IContainer Parent) : base(Parent)
    {
        Sprites["text"] = new Sprite(Viewport);
        Sprites["text"].Z = 2;
        Sprites["filler"] = new Sprite(Viewport, new SolidBitmap(1, 16, FillerColor));
        Sprites["filler"].Visible = false;
        Sprites["filler"].Y = 2;
        Sprites["caret"] = new Sprite(Viewport, new SolidBitmap(1, 16, CaretColor));
        Sprites["caret"].Y = 2;
        Sprites["caret"].Z = 1;
        Sprites["caret"].Visible = false;
        OnWidgetSelected += WidgetSelected;
        OnDisposed += delegate (BaseEventArgs e)
        {
            if (Mouse.InsideButNotNecessarilyAccessible)
            {
                Window.UI.SetSelectedWidget(null);
                Input.SetCursor(CursorType.Arrow);
            }
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
            DrawText();
        }
    }

    public void SetFillerColor(Color FillerColor)
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
            DrawText();
            if (SelectedWidget) Window.UI.SetSelectedWidget(null);
        }
    }

    public void SetText(string Text, bool callTextChangedEvent = true)
    {
        if (this.Text != Text)
        {
            string OldText = this.Text;
            this.Text = Text ?? "";
            X = 0;
            RX = 0;
            CaretIndex = 0;
            DrawText();
            // Puts caret at the end of the text
            //int width = Sprites["text"].Bitmap?.Width ?? 0;
            //int inviswidth = width - Size.Width;
            //if (inviswidth > 0) X = inviswidth;
            //RX = inviswidth > 0 ? Size.Width - 1 : width;
            //CaretIndex = this.Text.Length;
            if (callTextChangedEvent) OnTextChanged?.Invoke(new TextEventArgs(this.Text, OldText));
        }
    }

    public void SetFont(Font f)
    {
        Font = f;
        SetCaretHeight(Font.Size + 5);
        DrawText();
    }

    public void SetTextX(int TextX)
    {
        this.TextX = TextX;
        RepositionSprites();
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
                if (SelectedWidget)
                {
                    EnteringText = true;
                    Input.StartTextInput();
                    SetTimer("idle", 400);
                }
            }
        }
    }

    public void SetNumericOnly(bool NumericOnly)
    {
        if (this.NumericOnly != NumericOnly)
        {
            this.NumericOnly = NumericOnly;
        }
    }

    public void SetAllowFloats(bool AllowFloats)
    {
        if (this.AllowFloats != AllowFloats)
        {
            this.AllowFloats = AllowFloats;
        }
    }

    public void SetDefaultNumericValue(float DefaultNumericValue)
    {
        if (this.DefaultNumericValue != DefaultNumericValue)
        {
            this.DefaultNumericValue = DefaultNumericValue;
        }
    }

    public void SetAllowMinusSigns(bool AllowMinusSigns)
    {
        if (this.AllowMinusSigns != AllowMinusSigns)
        {
            this.AllowMinusSigns = AllowMinusSigns;
        }
    }

    public void SetShowDisabledText(bool ShowDisabledText)
    {
        if (this.ShowDisabledText != ShowDisabledText)
        {
            this.ShowDisabledText = ShowDisabledText;
            if (!Enabled) DrawText();
        }
    }

    public void SetDeselectOnEnterPress(bool DeselectOnEnterPressed)
    {
        if (this.DeselectOnEnterPressed != DeselectOnEnterPressed)
        {
            this.DeselectOnEnterPressed = DeselectOnEnterPressed;
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Width = Size.Width;
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
        if (NumericOnly && (Text == "-" || string.IsNullOrEmpty(Text))) SetText(DefaultNumericValue.ToString());
        EnteringText = false;
        Input.StopTextInput();
        if (SelectionStartIndex != -1) CancelSelectionHidden();
    }

    public override void TextInput(TextEventArgs e)
    {
        base.TextInput(e);
        if (ReadOnly) return;
        string text = Text;
        if (e.Text == "\n")
        {
            OnEnterPressed?.Invoke(new BaseEventArgs());
            if (DeselectOnEnterPressed) Window.UI.SetSelectedWidget(null);
            return;
        }
        else if (!string.IsNullOrEmpty(e.Text))
        {
            if (SelectionStartIndex != -1 && SelectionStartIndex != SelectionEndIndex) DeleteSelection();
            if (NumericOnly)
            {
                if (!IsNumeric(e.Text, AllowFloats)) return;
                if (e.Text == "-" && !AllowMinusSigns) return;
                if (Text.Length > 0 && Text[0] == '-')
                {
                    if (e.Text == "-") return;
                    if (CaretIndex == 0)
                    {
                        if (e.Text == "." || e.Text == ",") return;
                        // e.g. [5]- turns into 5, and [2]-11 turns into 211
                        MoveCaretRight(1);
                        RemoveText(0, 1);
                    }
                }
                // Disallow e.g. 2-3 and 23-
                else if (e.Text == "-" && CaretIndex != 0) return;
                if ((e.Text == "." || e.Text == ",") && Text.Contains('.')) return;
                InsertText(CaretIndex, e.Text.Replace(',', '.'));
            }
            else InsertText(CaretIndex, e.Text);
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
                    if (CaretIndex < Text.Length)
                    {
                        int Count = 1;
                        if (Input.Press(Keycode.CTRL))
                            Count = FindNextCtrlIndex(Text, CaretIndex, false) - CaretIndex;
                        // Disallow empty text
                        //if (CaretIndex == 0 && Count >= this.Text.Length && NumericOnly) return;
                        MoveCaretRight(Count);
                        RemoveText(CaretIndex - Count, Count);
                    }
                }
                else
                {
                    int Count = 1;
                    if (Input.Press(Keycode.CTRL))
                        Count = CaretIndex - FindNextCtrlIndex(Text, CaretIndex, true);
                    // Disallow empty text
                    //if (CaretIndex - Count == 0 && Count >= this.Text.Length && NumericOnly) return;
                    RemoveText(CaretIndex - Count, Count);
                }
            }
        }
        if (Text != text)
        {
            OnTextChanged?.Invoke(new TextEventArgs(Text, text));
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
        if (ReadOnly) return;
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
        CaretIndex += Text.Length;
        ResetIdle();
    }

    /// <summary>
    /// Deletes text to the left of the caret.
    /// </summary>
    /// <param name="StartIndex">Starting index of the range to delete.</param>
    /// <param name="Count">Number of characters to delete.</param>
    public void RemoveText(int StartIndex, int Count = 1)
    {
        if (ReadOnly) return;
        if (Text.Length == 0 || StartIndex < 0 || StartIndex >= Text.Length) return;
        string TextIncluding = Text.Substring(0, StartIndex + Count);
        int charw = Font.TextSize(TextIncluding).Width - Font.TextSize(Text.Substring(0, StartIndex)).Width;
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
        Text = Text.Remove(StartIndex, Count);
        ResetIdle();
    }

    /// <summary>
    /// Deletes the content inside the selection.
    /// </summary>
    public void DeleteSelection()
    {
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

        if (SelectedWidget && !Enabled)
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
            if (TimerExists("up")) DestroyTimer("up");
            if (TimerExists("up_initial")) DestroyTimer("up_initial");
            if (TimerExists("down")) DestroyTimer("down");
            if (TimerExists("down_initial")) DestroyTimer("down_initial");
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

        if (Input.Trigger(Keycode.LEFT) || TimerPassed("left"))
        {
            if (TimerPassed("left")) ResetTimer("left");
            if (CaretIndex > 0)
            {
                int Count = 1;
                if (Input.Press(Keycode.CTRL))
                {
                    Count = CaretIndex - FindNextCtrlIndex(Text, CaretIndex, true);
                }

                if (Input.Press(Keycode.SHIFT))
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
            else if (SelectionStartIndex != -1 && !Input.Press(Keycode.SHIFT))
            {
                CancelSelectionLeft();
            }
        }
        if (Input.Trigger(Keycode.RIGHT) || TimerPassed("right"))
        {
            if (TimerPassed("right")) ResetTimer("right");
            if (CaretIndex < Text.Length)
            {
                int Count = 1;
                if (Input.Press(Keycode.CTRL))
                {
                    Count = FindNextCtrlIndex(Text, CaretIndex, false) - CaretIndex;
                }

                if (Input.Press(Keycode.SHIFT))
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
            else if (SelectionStartIndex != -1 && !Input.Press(Keycode.SHIFT))
            {
                CancelSelectionRight();
            }
        }
        if (Input.Trigger(Keycode.HOME))
        {
            if (Input.Press(Keycode.SHIFT))
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
        if (Input.Trigger(Keycode.END))
        {
            if (Input.Press(Keycode.SHIFT))
            {
                if (SelectionStartIndex != -1) SelectionEndIndex = Text.Length;
                else
                {
                    SelectionStartIndex = CaretIndex;
                    SelectionStartX = X + RX;
                    SelectionEndIndex = Text.Length;
                }
            }
            else CancelSelectionRight();
            MoveCaretRight(Text.Length - CaretIndex);
            RepositionSprites();
        }
        if (Input.Press(Keycode.CTRL))
        {
            if (Input.Trigger(Keycode.A))
            {
                SelectAll();
            }
            if (Input.Trigger(Keycode.X))
            {
                CutSelection();
            }
            if (Input.Trigger(Keycode.C))
            {
                CopySelection();
            }
            if (Input.Trigger(Keycode.V) || TimerPassed("paste"))
            {
                PasteText();
            }
            if (Input.Trigger(Keycode.Z) || TimerPassed("undo"))
            {
                UndoText();
            }
            if (!Input.Press(Keycode.Z) && (Input.Trigger(Keycode.Y) || TimerPassed("redo")))
            {
                RedoText();
            }
        }
        if (Input.Trigger(Keycode.UP) || TimerPassed("up"))
        {
            if (TimerPassed("up")) ResetTimer("up");
            OnPressingUp?.Invoke(new BaseEventArgs());
        }
        if (Input.Trigger(Keycode.DOWN) || TimerPassed("down"))
        {
            if (TimerPassed("down")) ResetTimer("down");
            OnPressingDown?.Invoke(new BaseEventArgs());
        }

        // Timers for repeated input
        if (Input.Press(Keycode.LEFT))
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
        if (Input.Press(Keycode.RIGHT))
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
        if (Input.Press(Keycode.UP))
        {
            if (!TimerExists("up_initial") && !TimerExists("up"))
            {
                SetTimer("up_initial", 300);
            }
            else if (TimerPassed("up_initial"))
            {
                DestroyTimer("up_initial");
                SetTimer("up", 50);
            }
        }
        else
        {
            if (TimerExists("up")) DestroyTimer("up");
            if (TimerExists("up_initial")) DestroyTimer("up_initial");
        }
        if (Input.Press(Keycode.DOWN))
        {
            if (!TimerExists("down_initial") && !TimerExists("down"))
            {
                SetTimer("down_initial", 300);
            }
            else if (TimerPassed("down_initial"))
            {
                DestroyTimer("down_initial");
                SetTimer("down", 50);
            }
        }
        else
        {
            if (TimerExists("down")) DestroyTimer("down");
            if (TimerExists("down_initial")) DestroyTimer("down_initial");
        }
        if (Input.Press(Keycode.CTRL))
        {
            if (Input.Press(Keycode.V))
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
            if (Input.Press(Keycode.Z))
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
            if (Input.Press(Keycode.Y) && !Input.Press(Keycode.Z))
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
    }

    /// <summary>
    /// Resets the idle timer, which pauses the caret blinking.
    /// </summary>
    public void ResetIdle()
    {
        Sprites["caret"].Visible = Enabled;
        if (TimerExists("idle")) ResetTimer("idle");
    }

    /// <summary>
    /// Finds the next word that could be skipped to with control.
    /// </summary>
    /// <param name="Left">Whether to search to the left or right of the caret.</param>
    /// <returns>The next index to jump to when holding control.</returns>
    public static int FindNextCtrlIndex(string Text, int CaretIndex, bool Left) // or false for Right
    {
        int idx = 0;
        string splitters = " `~!@#$%^&*()-=+[]{}\\|;:'\",.<>/?\n";
        bool found = false;
        if (Left)
        {
            for (int i = CaretIndex - 1; i >= 0; i--)
            {
                if (splitters.Contains(Text[i]) && i != CaretIndex - 1)
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
            for (int i = CaretIndex + 1; i < Text.Length; i++)
            {
                if (splitters.Contains(Text[i]))
                {
                    idx = i;
                    found = true;
                    break;
                }
            }
            if (!found) idx = Text.Length;
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
        if (CaretIndex - Count < 0) Count = CaretIndex;
        if (Count <= 0) return;
        string TextToCaret = Text.Substring(0, CaretIndex);
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
        if (CaretIndex + Count > Text.Length) return;
        string TextToCaret = Text.Substring(0, CaretIndex);
        string TextToCaretPlusOne = Text.Substring(0, CaretIndex + Count);
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
    public (int RX, int CaretIndex, bool Found)? GetMousePosition(MouseEventArgs e)
    {
        int RetRX = RX;
        int RetCaretIndex = CaretIndex;
        bool Found = false;
        int rmx = e.X - Viewport.X - TextX;
        if (rmx < 0) return (0, 0, true);
        if (rmx >= Width) return null;
        for (int i = 0; i < Text.Length; i++)
        {
            int fullwidth = Font.TextSize(Text.Substring(0, i)).Width;
            int charw = Font.TextSize(Text.Substring(0, i + 1)).Width - Font.TextSize(Text.Substring(0, i)).Width;
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
                    Found = true;
                    break;
                }
            }
        }
        return (RetRX, RetCaretIndex, Found);
    }

    /// <summary>
    /// Redraws the text bitmap.
    /// </summary>
    public void DrawText()
    {
        RepositionSprites();
        Sprites["text"].Bitmap?.Dispose();
        Sprites["text"].Bitmap = null;
        if (!Enabled && !ShowDisabledText || string.IsNullOrEmpty(Text)) return;
        Size s = Font.TextSize(Text);
        if (s.Width < 1 || s.Height < 1) return;
        Sprites["text"].Bitmap = new Bitmap(s.Width, s.Height);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = Font;
        Sprites["text"].Bitmap.DrawText(Text, Enabled ? TextColor : DisabledTextColor);
        Sprites["text"].Bitmap.Lock();
    }

    /// <summary>
    /// Repositions the text, caret and selection sprites.
    /// </summary>
    public void RepositionSprites()
    {
        Sprites["text"].X = TextX - X;
        int add = 1;
        if (Text.Length > 0 && CaretIndex > 0 && Text[CaretIndex - 1] == ' ' && RX != Width) add = 0;
        Sprites["caret"].X = TextX + Math.Max(0, RX - add);

        // Selections
        if (SelectionStartIndex > SelectionEndIndex)
        {
            if (X + Width < SelectionStartX)
            {
                Sprites["filler"].X = TextX + RX;
                (Sprites["filler"].Bitmap as SolidBitmap).SetSize(Width - RX, CaretHeight);
                Sprites["filler"].Visible = true;
            }
            else
            {
                Sprites["filler"].X = TextX + RX;
                (Sprites["filler"].Bitmap as SolidBitmap).SetSize(SelectionStartX - X - RX, CaretHeight);
                Sprites["filler"].Visible = true;
            }
        }
        else if (SelectionStartIndex < SelectionEndIndex)
        {
            if (SelectionStartX < X)
            {
                Sprites["filler"].X = TextX;
                (Sprites["filler"].Bitmap as SolidBitmap).SetSize(RX, CaretHeight);
                Sprites["filler"].Visible = true;
            }
            else
            {
                Sprites["filler"].X = TextX + SelectionStartX - X;
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
        MoveCaretRight(Text.Length - CaretIndex);
        SelectionStartIndex = 0;
        SelectionEndIndex = Text.Length;
        SelectionStartX = 0;
        RepositionSprites();
    }

    /// <summary>
    /// Copies the selected text to the clipboard and deletes the selection.
    /// </summary>
    public void CutSelection()
    {
        if (ReadOnly) return;
        if (SelectionStartIndex != -1)
        {
            int startidx = SelectionStartIndex > SelectionEndIndex ? SelectionEndIndex : SelectionStartIndex;
            int endidx = SelectionStartIndex > SelectionEndIndex ? SelectionStartIndex : SelectionEndIndex;
            string OldText = Text;
            string text = Text.Substring(startidx, endidx - startidx);
            Input.SetClipboard(text);
            DeleteSelection();
            if (Text != OldText) OnTextChanged?.Invoke(new TextEventArgs(Text, OldText));
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
            string text = Text.Substring(startidx, endidx - startidx);
            Input.SetClipboard(text);
        }
    }

    /// <summary>
    /// Pastes text from the clipboard to the text field.
    /// </summary>
    public void PasteText()
    {
        // To make things easier, pasting is not allowed in numeric-only text areas. This out of laziness;
        // I would prefer not duplicating the code found in TextInput.
        if (ReadOnly) return;
        if (TimerPassed("paste")) ResetTimer("paste");
        string text = Input.GetClipboard();
        if (string.IsNullOrEmpty(text)) return;
        if (NumericOnly && !IsNumeric(text, AllowFloats))
        {
            if (IsNumeric(text.Trim(), AllowFloats)) text = text.Trim();
            else return;
        }
        string OldText = Text;
        if (SelectionStartIndex != -1 && SelectionStartIndex != SelectionEndIndex) DeleteSelection();
        InsertText(CaretIndex, text);
        if (Text != OldText) OnTextChanged?.Invoke(new TextEventArgs(Text, OldText));
        DrawText();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Mouse.Inside || Text.Length == 0 || !Enabled) return;
        if (SelectionStartIndex != -1 && SelectionStartIndex != SelectionEndIndex) CancelSelectionHidden();
        int OldRX = RX;
        int OldCaretIndex = CaretIndex;
        (int RX, int CaretIndex, bool Found)? ret = GetMousePosition(e);
        if (ret == null) throw new Exception("Invalid return value");
        RX = ret.Value.RX;
        CaretIndex = ret.Value.CaretIndex;
        if (!ret.Value.Found)
        {
            if (Sprites["text"].Bitmap.Width - X < Width) // No extra space to the right that could be scrolled to
            {
                RX = Sprites["text"].Bitmap.Width;
                CaretIndex = Text.Length;
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
        int startindex = FindNextCtrlIndex(Text, CaretIndex, true);
        int endindex = FindNextCtrlIndex(Text, CaretIndex, false);
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
        if (!e.LeftButton || !Mouse.LeftStartedInside || !Enabled) return;
        int OldRX = RX;
        int OldCaretIndex = CaretIndex;
        int rmx = e.X - Viewport.X;
        if (rmx >= Width && Sprites["text"].Bitmap.Width - X >= Width)
        {
            if (CaretIndex == Text.Length) X += Sprites["text"].Bitmap.Width - X - Width;
            else MoveCaretRight();
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
        (int RX, int CaretIndex, bool Found)? ret = GetMousePosition(e);
        if (ret == null)
        {
            if (rmx < 0)
            {
                MoveCaretLeft();
                SelectionEndIndex = CaretIndex;
                RepositionSprites();
            }
            return;
        }
        RX = ret.Value.RX;
        CaretIndex = ret.Value.CaretIndex;
        if (ret.Value.Found && CaretIndex != OldCaretIndex)
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
        if (e.CursorHandled) return;
        if (Mouse.LeftMousePressed || Mouse.RightMousePressed || Mouse.MiddleMousePressed) return;
        if (Mouse.Inside && Enabled)
        {
            Input.SetCursor(CursorType.IBeam);
            e.CursorHandled = true;
        }
        else
        {
            Input.SetCursor(CursorType.Arrow);
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (e.CursorHandled) return;
        if (Mouse.LeftMousePressed || Mouse.RightMousePressed || Mouse.MiddleMousePressed) return;
        if (Mouse.Inside && Enabled)
        {
            Input.SetCursor(CursorType.IBeam);
            e.CursorHandled = true;
        }
        else if (!Mouse.Inside && Input.SystemCursor == CursorType.IBeam)
        {
            Input.SetCursor(CursorType.Arrow);
        }
    }

    void UndoText()
    {
        if (ReadOnly) return;
        if (TimerPassed("undo")) ResetTimer("undo");
        if (UndoList.Count > 1)
        {
            string OldText = Text;
            TextAreaState NewState = UndoList[UndoList.Count - 2];
            SetState(NewState);
            TextAreaState OldState = UndoList[UndoList.Count - 1];
            UndoList.RemoveAt(UndoList.Count - 1);
            RedoList.Add(OldState);
            UndoingOrRedoing = true;
            OnTextChanged?.Invoke(new TextEventArgs(Text, OldText));
            UndoingOrRedoing = false;
        }
    }

    void RedoText()
    {
        if (ReadOnly) return;
        if (TimerPassed("redo")) ResetTimer("redo");
        if (RedoList.Count > 0)
        {
            string OldText = Text;
            TextAreaState NewState = RedoList[RedoList.Count - 1];
            SetState(NewState);
            RedoList.RemoveAt(RedoList.Count - 1);
            UndoList.Add(NewState);
            UndoingOrRedoing = true;
            OnTextChanged?.Invoke(new TextEventArgs(Text, OldText));
            UndoingOrRedoing = false;
        }
    }

    bool IsNumeric(string s, bool allowOneDot = false)
    {
        bool seenDot = false;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '.' || c == ',')
            {
                if (allowOneDot && !seenDot)
                {
                    seenDot = true;
                    continue;
                }
                else return false;
            }
            if (allowOneDot && seenDot && c == '-') return false;
            if (c == '-' && i == 0) continue;
            if (c < '0' || c > '9') return false;
        }
        return true;
    }

    public TextAreaState GetState()
    {
        return new TextAreaState(Text, X, RX, Width, CaretIndex,
            SelectionStartIndex, SelectionEndIndex, SelectionStartX);
    }

    public void SetState(TextAreaState State)
    {
        Text = State.Text;
        X = State.X;
        RX = State.RX;
        Width = State.Width;
        CaretIndex = State.CaretIndex;
        SelectionStartIndex = State.SelectionStartIndex;
        SelectionEndIndex = State.SelectionEndIndex;
        SelectionStartX = State.SelectionStartX;
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
        return Text;
    }
}