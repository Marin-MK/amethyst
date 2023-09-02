using System;

namespace amethyst.Animations;

public class EaseOutCircularAnimation : IAnimation
{
    public EaseOutCircularAnimation(string ID, long Milliseconds, Action<double> Callback, bool CompleteIfStopped = true) : base(ID, Milliseconds, Callback, CompleteIfStopped) { }

    public override void Execute(double CompletionFactor)
    {
        base.Execute(CompletionFactor);
        double result = Math.Sqrt(1 - Math.Pow(CompletionFactor - 1, 2));
        // Make sure we eventually pass a 1 for anything that depends on 100% completion
        if (CompletionFactor == 1) Callback(1);
        else Callback(result);
    }
}
