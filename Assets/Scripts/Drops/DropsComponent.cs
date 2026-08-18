using FishNet;
using UnityEngine;

public class DropsComponent : MonoBehaviour
{

    public void DropFromChest()
    {
        Drop(Vector3.up * 1.5f + transform.forward * 0.5f, Vector3.zero);
    }

    public void DropFromEnemy()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector3 force = new Vector3(dir.x, 1f, dir.y) * Random.Range(1f, 3f);
        Vector3 torque = Random.insideUnitSphere * 10f;

        Drop(force, torque);
    }

    private void Drop(Vector3 force, Vector3 torque)
    {
        if (ItemDatabase.Instance == null) return;

        var rarity = GetRarity();
        if (rarity?.AllowedDrops == null || rarity.AllowedDrops.Count == 0) return;

        LootDefinition definition = rarity.AllowedDrops[Random.Range(0, rarity.AllowedDrops.Count)];

        var obj = Instantiate(definition.Prefab, transform.position, Quaternion.identity);

        WorldDrop drop = obj.GetComponent<WorldDrop>();
        
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(force, ForceMode.Impulse);
            if (torque != Vector3.zero) rb.AddTorque(torque, ForceMode.Impulse);
        }

        InstanceFinder.ServerManager.Spawn(obj);
        definition.Initialize(drop, rarity);

    }

    private static Rarity GetRarity()
    {
        return ItemDatabase.Instance.RollRandomRarity(new System.Random());
    }


}