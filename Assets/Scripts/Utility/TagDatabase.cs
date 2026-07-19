using System.Collections.Generic;
using UnityEngine;

public class TagDatabase : MonoBehaviour
{
    public TextAsset TagFile;

    private HashSet<string> tags = new();

    private void Awake()
    {
        LoadTags();
    }

    private void LoadTags()
    {
        foreach (string line in TagFile.text.Split('\n'))
        {
            string tag = line.Trim();

            if (!string.IsNullOrEmpty(tag))
                tags.Add(tag);
        }
    }

    public bool Exists(string tag)
    {
        return tags.Contains(tag);
    }
}