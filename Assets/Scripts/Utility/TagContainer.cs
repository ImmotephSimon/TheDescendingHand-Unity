using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TagContainer
{
    public List<string> Tags = new();

    public bool HasTag(string tag)
    {
        foreach (var existing in Tags)
        {
            if (existing == tag)
                return true;

            // parent matching
            if (existing.StartsWith(tag + "."))
                return true;
        }

        return false;
    }
}