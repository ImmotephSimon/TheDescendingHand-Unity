using FishNet.Object;
using System;
using System.Collections.Generic;

public class GameWorld : NetworkBehaviour
{
    public static GameWorld Instance { get; private set; }

    private readonly List<IEntity> entities = new();

    public event Action<IEntity> EntityRevived;
    public event Action<IEntity, IEntity> EntityDied;
    // victim, killer

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        Instance = this;
    }

    public override void OnStopServer()
    {
        if (Instance == this)
            Instance = null;

        base.OnStopServer();
    }

    public void RegisterEntity(IEntity entity)
    {
        if (!IsServerStarted)
            return;

        if (!entities.Contains(entity))
            entities.Add(entity);
    }

    public void UnregisterEntity(IEntity entity)
    {
        if (!IsServerStarted)
            return;

        entities.Remove(entity);
    }

    public void NotifyDeath(IEntity victim, IEntity killer)
    {
        if (!IsServerStarted)
            return;

        EntityDied?.Invoke(victim, killer);
    }

    public void NotifyRevive(IEntity entity)
    {
        if (!IsServerStarted)
            return;

        EntityRevived?.Invoke(entity);
    }


    public IReadOnlyList<IEntity> GetEntities()
    {
        return entities;
    }
}