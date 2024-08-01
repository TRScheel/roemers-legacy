using Godot;
using System;

public partial class GameTimeManager : Node
{
    public static GameTimeManager Instance { get; private set; } = null!;

    private DateTimeOffset startTime;
    private DateTimeOffset currentTime;
    private bool isPaused;

    public override void _Ready()
    {
        if (Instance is null)
        {
            Instance = this;
            startTime = DateTimeOffset.UtcNow;
            currentTime = startTime;
            isPaused = false;
            SetProcess(false); // Disable processing by default
        }
        else
        {
            QueueFree();
        }
    }

    public void AdvanceTime(TimeSpan delta)
    {
        if (!isPaused)
        {
            currentTime = currentTime.Add(delta);
        }
    }

    public void SetTime(DateTimeOffset time)
    {
        currentTime = time;
    }

    public DateTimeOffset GetCurrentTime()
    {
        return currentTime;
    }

    public long GetElapsedTime()
    {
        return currentTime.UtcTicks - startTime.UtcTicks;
    }

    public void PauseTime()
    {
        isPaused = true;
        SetProcess(false);
    }

    public void ResumeTime()
    {
        isPaused = false;
        SetProcess(true);
    }
}
