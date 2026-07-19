using Cards.CardComponents;
using System;
using UnityEngine;

public class ElectrocuteCard : Card
{
    // The parameters here match the factory signature exactly
    public ElectrocuteCard(Guid id, float castTime, Transform owner, GameObject projectilePrefab, Action<GameObject> onNetworkSpawn)
        : base(id, castTime, owner)
    {
        AddComponent(new DirectDamageComponent(damageRange: new Vector2(50, 100), effectiveness: 1.5f));

        // Hand the network delegate over to the projectile component
        AddComponent(new ProjectileComponent(new ProjectileInfo(owner, new Vector3(), projectilePrefab), onNetworkSpawn));
    }
    //AddComponent(new ChainComponent(5));
    //AddComponent(new RefreshChainComponent());
}