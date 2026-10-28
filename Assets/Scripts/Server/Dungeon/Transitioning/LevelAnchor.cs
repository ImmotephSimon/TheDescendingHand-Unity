using UnityEngine;

public class LevelAnchor : MonoBehaviour
{
    [SerializeField] private Transform anchor;

    public Transform Anchor => anchor;

    public int LevelIndex;
}