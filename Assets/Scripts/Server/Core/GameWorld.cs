using System;
using System.Collections.Generic;
using UnityEngine;

public class GameWorld : MonoBehaviour
{
    public static GameWorld Instance { get; private set; }

    private readonly List<IEntity> entities = new();

    public event Action<IEntity> EntityRevived;
    public event Action<IEntity, IEntity> EntityDied;

    public ServerScheduler Scheduler { get; private set; }

    private void Awake()
    {
        Instance = this;
        Scheduler = gameObject.AddComponent<ServerScheduler>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegisterEntity(IEntity entity)
    {
        if (!entities.Contains(entity))
            entities.Add(entity);
    }

    public void UnregisterEntity(IEntity entity)
    {
        entities.Remove(entity);
    }

    public void NotifyDeath(IEntity victim, IEntity killer)
    {
        EntityDied?.Invoke(victim, killer);
    }

    public void NotifyRevive(IEntity entity)
    {
        EntityRevived?.Invoke(entity);
    }

    public IReadOnlyList<IEntity> GetEntities() => entities;
}