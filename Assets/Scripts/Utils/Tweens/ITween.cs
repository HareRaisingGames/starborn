using System;

public interface ITween
{
    void Update();
    void OnCompleteKill();
    void FullKill();
    bool IsTargetDestroyed();
    void Pause();
    void Resume();

    object Target { get; }
    bool isComplete { get; }
    bool WasKilled { get; }
    bool isPaused { get; }
    bool IgnoreTimeScale { get; }
    string Identifier { get; }
    float DelayTime { get; }
    Action onComplete { get; set; }
}
