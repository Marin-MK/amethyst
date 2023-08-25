using System;
using System.Diagnostics;

namespace amethyst.Animations;

public abstract class IAnimation
{
    public string ID { get; protected set; }
    public long StartTicks { get; protected set; }
    public long EndTicks { get; protected set; }
    public long Length { get; protected set; }
    public Action<double> Callback { get; protected set; }
    public bool CompleteIfStopped { get; protected set; } = true;
    public bool Paused { get; protected set; }

    public Action OnStarted;
    public Action OnStopped;
    public Action OnPaused;
    public Action OnResumed;
    public Action OnFinished;

    private double CompletionFactor;

    public IAnimation(string ID, long Milliseconds, Action<double> Callback, bool CompleteIfStopped = true)
    {
        this.ID = ID;
        Length = Milliseconds * 10000;
        this.Callback = Callback;
        this.CompleteIfStopped = true;
    }

    public virtual void Start()
    {
        if (Paused) throw new Exception("Animation is paused.");
        else if (StartTicks != 0) throw new Exception("Animation has already started.");
        StartTicks = Stopwatch.GetTimestamp();
        EndTicks = StartTicks + Length;
        OnStarted?.Invoke();
    }

    public virtual void Pause()
    {
        if (Paused) throw new Exception("Animation is already paused.");
        Paused = true;
        OnPaused?.Invoke();
    }

    public virtual void Resume()
    {
        if (!Paused) throw new Exception("Animation was not paused.");
        Paused = false;
        long Current = Stopwatch.GetTimestamp();
        StartTicks = Current - (long)Math.Round(Length * CompletionFactor);
        EndTicks = Current + (long)Math.Round(Length * (1 - CompletionFactor));
        OnResumed?.Invoke();
    }

    public virtual void Stop()
    {
        if (StartTicks == 0) throw new Exception("Animation was never started.");
        if (CompleteIfStopped) Execute(1d);
        OnStopped?.Invoke();
    }

    public virtual void Execute(double CompletionFactor)
    {
        this.CompletionFactor = CompletionFactor;
    }
}
