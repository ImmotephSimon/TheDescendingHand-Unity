using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class CardVisuals
{
    public string Name;
    public string Description;
    [FormerlySerializedAs("Art")] public Sprite Icon;
    public AnimationClip AnimationOverride;
    public List<GameObject> Vfx;
}