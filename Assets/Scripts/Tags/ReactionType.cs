using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public abstract class ReactionType : ScriptableObject
{
    public abstract GameTag ListeningTag { get; }

    public virtual void StartListening(IEntity owner)
    {
        if (ListeningTag == null) return;

        ModifierHandle handle = null;

        void OnStatChanged(float newValue)
        {
            if (newValue > 0 && handle == null)
            {
                handle = Apply(owner, newValue);
            }
            else if (newValue <= 0 && handle != null)
            {
                owner.Stats.RemoveModifier(handle);
                handle = null;
            }
        }

        owner.Stats.Listen(ListeningTag, OnStatChanged);

        // Evaluate initial value immediately
        OnStatChanged(owner.Stats.GetStat(ListeningTag));
    }

    protected abstract ModifierHandle Apply(IEntity owner, float newValue);
}