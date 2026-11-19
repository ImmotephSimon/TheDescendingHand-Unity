using UnityEngine;

public class OverworldAnchor : MonoBehaviour
{
    public static OverworldAnchor Instance { get; private set; }
    [SerializeField] private Transform stairsAnchor;
    public Transform StairsAnchor => stairsAnchor;

    private void Awake() => Instance = this;
}