using System;
using System.Collections.Generic;

[Serializable]
public struct CardVisuals
{
    public string Name;
    public string Description;
    public SpriteString Icon;
    public AnimationString AnimationOverride;
    public List<VfxType> Vfx;

    public bool IsEmpty => string.IsNullOrEmpty(Name);
}