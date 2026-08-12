using System;
using UnityEngine;

[Serializable]
public class CardVisuals
{
    public string Name;
    public string Description;
    public Texture2D Art;
    public CardCastAnimation CastAnimation;
    public GameObject Impact;
}

public enum CardCastAnimation
{
    Default,
    Special
}
