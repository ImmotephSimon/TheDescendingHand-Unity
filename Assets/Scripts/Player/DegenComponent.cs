using System;
using UnityEngine;

public class DegenComponent : MonoBehaviour
{
    private IHealth _healthHandler;

    public DegenComponent()
    {
        _healthHandler = GetComponent<IHealth>();
    }

    public void Apply(DegenInfo degenInfo)
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        
    }
}