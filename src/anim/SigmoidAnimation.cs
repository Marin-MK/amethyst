using System;

namespace amethyst.Animations;

public class SigmoidAnimation : IAnimation
{
    public SigmoidAnimation(string ID, long Milliseconds, Action<double> Callback, bool CompleteIfStopped = true) : base(ID, Milliseconds, Callback, CompleteIfStopped) { }

    public override void Execute(double CompletionFactor)
    {
        base.Execute(CompletionFactor);
        double result = 1 / (1 + Math.Exp(-2 * 10 * CompletionFactor + 10));
        // Make sure we eventually pass a 1 for anything that depends on 100% completion
        if (CompletionFactor == 1) Callback(1);
        else Callback(result);
    }
}