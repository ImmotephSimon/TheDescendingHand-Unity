using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerScheduler : MonoBehaviour
{
    private class ScheduledAction
    {
        public float ExecuteAt;
        public Action Callback;

        public ScheduledAction(float executeAt, Action callback)
        {
            ExecuteAt = executeAt;
            Callback = callback;
        }
    }

    private readonly List<ScheduledAction> _actions = new();
    private Coroutine _routine;

    public void Schedule(float delay, Action callback)
    {
        var action = new ScheduledAction(
            Time.time + delay,
            callback);

        bool wasEarliest = _actions.Count == 0 ||
                           action.ExecuteAt < _actions[0].ExecuteAt;

        _actions.Add(action);
        _actions.Sort((a, b) => a.ExecuteAt.CompareTo(b.ExecuteAt));

        if (_routine == null)
        {
            _routine = StartCoroutine(Process());
        }
        else if (wasEarliest)
        {
            StopCoroutine(_routine);
            _routine = StartCoroutine(Process());
        }
    }

    private IEnumerator Process()
    {
        while (_actions.Count > 0)
        {
            var next = _actions[0];
            float delay = next.ExecuteAt - Time.time;

            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            _actions.RemoveAt(0);
            next.Callback();
        }

        _routine = null;
    }
}