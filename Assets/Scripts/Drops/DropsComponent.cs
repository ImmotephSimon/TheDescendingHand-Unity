using FishNet;
using UnityEngine;

public class DropsComponent : MonoBehaviour
{

    public void DropAtLocation()
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError($"No ItemDatabase found.");
            return;
        }

        var rarity = GetRarity();
        if (rarity == null || rarity.AllowedDrops == null || rarity.AllowedDrops.Count == 0)
            return;

        LootDefinition definition = rarity.AllowedDrops[Random.Range(0, rarity.AllowedDrops.Count)];

        var obj = Instantiate(
            definition.Prefab,
            transform.position,
            Quaternion.identity);

        ApplyRarityVisuals(obj, rarity);
        WorldDrop drop = obj.GetComponent<WorldDrop>();


        definition.Initialize(drop, rarity);

        InstanceFinder.ServerManager.Spawn(obj);
        Debug.Log($"Spawned {obj.name}, active={obj.activeSelf}");
    }

    private static Rarity GetRarity()
    {
        return ItemDatabase.Instance.RollRandomRarity(new System.Random());
    }

    private void ApplyRarityVisuals(GameObject obj, Rarity rarity)
    {
        if (obj.TryGetComponent<Light>(out var light))
        {
            light.color = rarity.DisplayColor;
            light.intensity = rarity.LightIntensity;
            light.range = rarity.LightRange;
        }
    }
}