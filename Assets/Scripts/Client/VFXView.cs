using System;
using System.Collections.Generic;
using UnityEngine;

public class VFXView : MonoBehaviour
{
    [SerializeField] private List<AbilityVisualEntry> projectileAttachments;
    [SerializeField] private List<CardImpactEntry> impacts;
    
    [Serializable]
    public class AbilityVisualEntry
    {
        public AbilityVisual Visual;
        public GameObject Prefab;
    }

    private IAnimationHandler animationHandler;

    private void Awake()
    {
        animationHandler = GetComponentInParent<IAnimationHandler>();
    }


    private readonly Dictionary<Transform, GameObject> _spawnedVisuals = new();

    public void AttachAbilityVisual(AbilityVisual visual, Transform target)
    {
        if (!TryGetPrefab(visual, out var prefab))
        {
            Debug.LogWarning($"No VFX assigned for {visual}");
            return;
        }

        var instance = Instantiate(prefab);
        if (instance.TryGetComponent<IVfx>(out var vfx))
        {
            vfx.Initialize(target.position,target);

        }
    }

    public void DetachAbilityVisual(AbilityVisual visual, Transform target)
    {
    }

    private bool TryGetPrefab(AbilityVisual visual, out GameObject prefab)
    {
        foreach (var entry in projectileAttachments)
        {
            if (entry.Visual == visual)
            {
                prefab = entry.Prefab;
                return true;
            }
        }

        prefab = null;
        return false;
    }


    internal void PlayCardCastAnimation(CardCastAnimation animation)
    {
        
        animationHandler.PlayAnimation(animation);
    }

    public void PlayCardImpact(CardImpactVisual visual)
    {
        if (!TryGetImpact(visual, out var prefab))
        {
            return;
        }

        Instantiate(prefab, transform);
    }


    private bool TryGetImpact(CardImpactVisual visual, out GameObject prefab)
    {
        foreach (var entry in impacts)
        {
            if (entry.Visual == visual)
            {
                prefab = entry.Prefab;
                return true;
            }
        }

        prefab = null;
        return false;
    }

    internal void SetAnimationHandler(IAnimationHandler animationHandler)
    {
        this.animationHandler = animationHandler;
    }

    [Serializable]
    private class CardCastEntry
    {
        public CardCastAnimation Animation;
        public GameObject Prefab;
    }

    [Serializable]
    private class CardImpactEntry
    {
        public CardImpactVisual Visual;
        public GameObject Prefab;
    }
}

