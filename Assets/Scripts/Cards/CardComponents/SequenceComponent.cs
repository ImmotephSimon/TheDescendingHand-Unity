using System;
using System.Collections;
using UnityEngine;
public class SequenceComponent : CardComponent
{
    private int _steps;
    private float _delay;

    public Action OnSequence;

    public void Configure(int steps, float delay)
    {
        _steps = steps;
        _delay = delay;
    }

    public override void Initialize(CardRuntime card, IEntity owner)
    {
        base.Initialize(card, owner);
        OnBeginRoutine();
    }


    protected IEnumerator OnBeginRoutine()
    {
        for (int i = 0; i < _steps; i++)
        {
            OnSequence.Invoke();
            if (i < _steps - 1) yield return new WaitForSeconds(_delay);
        }
    }
}