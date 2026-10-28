using UnityEngine;

public class GoldDrop : WorldDrop
{
    private int _amount;

    public void Initialize(int amount)
    {
        _amount = amount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerStarted)
            return;

        if (!other.TryGetComponent<IPlayerCollection>(out var collection))
        {
            Debug.LogError($"No gold storage found.");
            return;
        }
            

        collection.AddGold(_amount);
        Despawn();
    }
}