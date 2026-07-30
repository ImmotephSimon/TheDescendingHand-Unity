using System;
using UnityEngine;

[Serializable]
public class CardVisuals
{
    public string Name;
    public string Description;
    public Texture2D Art;
    public CardCastAnimation CastAnimation;
    public CardImpactVisual Impact;
}

public enum CardCastAnimation
{
    Default,
    Special
}

public enum CardImpactVisual
{
    None,
    Explosion
}