using System;

[Serializable]
public class CardVisuals
{
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