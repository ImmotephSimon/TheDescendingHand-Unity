using System;
using System.Collections.Generic;

[Serializable]
public struct CardDefinitionDto
{
    public Guid Id;
    public string Name;
    public string Description;
    public SpriteString Icon;
    public List<string> VfxIds;
}