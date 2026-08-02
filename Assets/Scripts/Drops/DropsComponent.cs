using FishNet;
using System.Linq;
using UnityEngine;

public class DropsComponent : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;

    public void DropAtLocation()
    {

        if (ItemDatabase.Instance == null || ItemDatabase.Instance.Items == null || ItemDatabase.Instance.Items.Count == 0)
        {
            Debug.LogError("ItemDatabase instance or Items list is missing!");
            return;
        }
        var drops = ItemDatabase.Instance.Items;

        var definition = drops[Random.Range(0, drops.Count)];

        var obj = Instantiate(itemPrefab, transform.position, Quaternion.identity);

        // Set SyncVar before Spawn()
        obj.GetComponent<Item>().Initialize(definition);

        InstanceFinder.ServerManager.Spawn(obj);
        Debug.Log($"Spawned {obj.name}, active={obj.activeSelf}");
    }
}