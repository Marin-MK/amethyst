﻿using System;

namespace amethyst.Animations;

public class LinearAnimation : IAnimation
{
    public LinearAnimation(string ID, long Milliseconds, Action<double> Callback, bool CompleteIfStopped = true) : base(ID, Milliseconds, Callback, CompleteIfStopped) { }

    public override void Execute(double CompletionFactor)
    {
        base.Execute(CompletionFactor);
        Callback(CompletionFactor);
    }
}
