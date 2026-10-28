using System;
using System.Collections.Generic;

[Serializable]
public class CardVisuals
{
    public string Name;
    public string Description;
    public SpriteString Icon;
    public AnimationString AnimationOverride;
    public List<VfxString> Vfx;
}