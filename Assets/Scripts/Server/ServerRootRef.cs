using UnityEngine;

public class ServerRootRef : MonoBehaviour
{
    [SerializeField] private GameObject serverRootPrefab;
    public GameObject Prefab => serverRootPrefab;
}