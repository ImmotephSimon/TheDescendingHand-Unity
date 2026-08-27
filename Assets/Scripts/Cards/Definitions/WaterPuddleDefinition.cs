using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Water Puddle")]
public class WaterPuddleDefinition : CardDefinition
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;

    public override Card Create(CardInitContext context)
    {
        var card = new Card(
            context.InstanceId,
            this,
            context.Owner);

        var overlap = new AreaOverlapComponent(radius);

        var status = new StatusEffectComponent(GameTags.StatusFreeze, 4f);

        var damage = new DirectDamageComponent(
            effectiveness,
            damageConversion,
            triggerOnHit: false);

        var listeners = new Dictionary<IEntity, Action<float>>();
        var electrified = false;

        void SetElectrified(bool IsElectrified)
        {
            if (electrified == IsElectrified)
                return;

            electrified = IsElectrified;

            // Change the puddle's behavior/state here.
        }


        card.OnActivated += () =>
        {
            overlap.ToggleTick(card.TargetLocation);
            context.ClientSpawn(this, new VfxSpawnParams(card.TargetLocation) { Scale = Vector3.one * radius });
        };



        overlap.OnEntityEntered += entity =>
        {
            Action<float> listener = newValue =>
            {
                if (newValue <= 0f)
                    return;

                SetElectrified(true);


            };

            listeners[entity] = listener;
            entity.Stats.Listen(GameTags.StatusElectrified, listener);

            // Also check immediately
            listener?.Invoke(entity.Stats.GetStat(GameTags.StatusElectrified));
        };

        overlap.OnEntityExited += entity =>
        {
            if (listeners.TryGetValue(entity, out var listener))
            {
                entity.Stats.StopListening(GameTags.StatusElectrified, listener);
                listeners.Remove(entity);
            }

        };

        card.AddComponent(overlap);
        card.AddComponent(damage);
        card.AddComponent(status);

        return card;
    }


}