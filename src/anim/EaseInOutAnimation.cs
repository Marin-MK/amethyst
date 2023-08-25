using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amethyst.Animations;

public class EaseInOutAnimation : IAnimation
{
    public EaseInOutAnimation(string ID, long Milliseconds, Action<double> Callback, bool CompleteIfStopped = true) : base(ID, Milliseconds, Callback, CompleteIfStopped) { }

    public override void Execute(double CompletionFactor)
    {
        base.Execute(CompletionFactor);
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;

        double result = CompletionFactor < 0.5
          ? Math.Pow(2 * CompletionFactor, 2) * ((c2 + 1) * 2 * CompletionFactor - c2) / 2
          : (Math.Pow(2 * CompletionFactor - 2, 2) * ((c2 + 1) * (CompletionFactor * 2 - 2) + c2) + 2) / 2;
        // Make sure we eventually pass a 1 for anything that depends on 100% completion
        if (CompletionFactor == 1) Callback(1);
        else Callback(result);
    }
}
