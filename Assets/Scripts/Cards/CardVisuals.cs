using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardVisuals
{
    public string Name;
    public string Description;
    public Texture2D Art;
    public AnimationClip AnimationOverride;
    public List<GameObject> Vfx;
}