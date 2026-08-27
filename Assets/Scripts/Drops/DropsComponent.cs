using FishNet;
using UnityEngine;

public class DropsComponent : MonoBehaviour
{
    public void DropFromChest(Vector3 position, Vector3 forward)
    {
        Rarity rarity = ItemDatabase.Instance.RollRandomRarity(
            new System.Random(), minimumTier: 1);

        LootDefinition definition =
            rarity.AllowedDrops[Random.Range(0, rarity.AllowedDrops.Count)];

        Vector3 spawnPosition =
            GetFloorPosition(position) +
            Vector3.up * definition.DropHeight;

        Vector3 force =
            Vector3.up * definition.UpForce +
            forward * definition.ForwardForce;

        Drop(definition, rarity, spawnPosition, force);
    }

    public void DropFromEnemy(Vector3 position)
    {
        Rarity rarity = ItemDatabase.Instance.RollRandomRarity(
            new System.Random());

        LootDefinition definition =
            rarity.AllowedDrops[Random.Range(0, rarity.AllowedDrops.Count)];

        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector3 force =
            new Vector3(dir.x, 1f, dir.y) *
            (definition.UpForce + Random.Range(-1f, 1f));

        Vector3 spawnPosition = GetFloorPosition(position);

        Drop(definition, rarity, spawnPosition, force);
    }

    private Vector3 GetFloorPosition(Vector3 position)
    {
        int floorLayer = LayerMask.NameToLayer("Floor");

        if (Physics.Raycast(
                position,
                Vector3.down,
                out RaycastHit hit,
                Mathf.Infinity,
                1 << floorLayer))
        {
            Debug.Log(
                $"Drop trace: source={position}, floor={hit.point}, distance={hit.distance}");

            return hit.point;
        }

        Debug.LogError($"Could not find floor below drop position {position}.");
        return position;
    }

    private void Drop(
        LootDefinition definition,
        Rarity rarity,
        Vector3 position,
        Vector3 force)
    {
        var obj = Instantiate(
            definition.Prefab,
            position,
            Quaternion.identity);

        WorldDrop drop = obj.GetComponent<WorldDrop>();

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(force, ForceMode.Impulse);

            if (definition.Torque != 0f)
            {
                rb.AddTorque(
                    Random.insideUnitSphere * definition.Torque,
                    ForceMode.Impulse);
            }
        }

        InstanceFinder.ServerManager.Spawn(obj);
        definition.Initialize(drop, rarity);

        Debug.Log(
            $"Dropping {definition.Prefab.name} ({rarity.name})");
    }
}