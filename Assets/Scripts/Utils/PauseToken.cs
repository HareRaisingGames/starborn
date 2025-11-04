using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

public class PauseTokenSource
{
    private TaskCompletionSource<bool> _tcs;
    public PauseToken Token { get; }

    public PauseTokenSource()
    {
        _tcs = new TaskCompletionSource<bool>();
        Token = new PauseToken(_tcs.Task);
    }

    public void Pause()
    {
        _tcs = new TaskCompletionSource<bool>(); // Create a new TCS to pause
        Token.Task = _tcs.Task; // Update the token's task
    }

    public void Resume()
    {
        _tcs.SetResult(true); // Complete the current TCS to resume
    }
}

public class PauseToken
{
    internal Task Task { get; set; }

    internal PauseToken(Task task)
    {
        Task = task;
    }

    public async Task WaitWhilePaused()
    {
        await Task;
    }
}
