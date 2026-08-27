using UnityEngine;

public abstract class ReactionType : ScriptableObject
{
    public abstract GameTag ListeningTag { get; }

    public virtual void StartListening(IEntity owner)
    {
        if (ListeningTag == null) return;

        ModifierHandle handle = null;

        owner.Stats.Listen(ListeningTag, newValue =>
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
        });
    }

    protected abstract ModifierHandle Apply(IEntity owner, float newValue);
}