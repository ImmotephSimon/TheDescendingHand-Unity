using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TagReactions/Tag Reactions")]
public class TagReactions : ScriptableObject
{
    public List<TagReactionEntry> Reactions;
}