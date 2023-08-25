using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amethyst.src.Animations;

public class EaseOutExponentialAnimation : IAnimation
{
    public EaseOutExponentialAnimation(string ID, long Milliseconds, Action<double> Callback, bool CompleteIfStopped = true) : base(ID, Milliseconds, Callback, CompleteIfStopped) { }

    public override void Execute(double CompletionFactor)
    {
        base.Execute(CompletionFactor);
        double result = 1 - Math.Pow(2, -10 * CompletionFactor);
        // Make sure we eventually pass a 1 for anything that depends on 100% completion
        if (CompletionFactor == 1) Callback(1);
        else Callback(result);
    }
}
