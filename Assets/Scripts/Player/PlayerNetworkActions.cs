using FishNet.Connection;
using FishNet.Object;

public class PlayerNetworkActions : NetworkBehaviour
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        GetComponent<PlayerInventory>().OnItemDropped += SpawnWorldDrop;
    }


    [Server]
    public void SpawnWorldDrop(IInventoryItem item)
    {
        if (!ServerPrefabRegistry.Instance.TryInstantiate(item.DropPrefabTag, transform.position, out var obj))
            return;

        ServerManager.Spawn(obj);
        obj.GetComponent<WorldDrop>().Restore(item);
    }

    [ServerRpc]
    public void RequestInteract()
    {
        var player = GetComponent<Player>();
        var interactable = player.FindNearbyInteractable();
        interactable?.Interact(player);
    }

    internal void ShowSpeechBubble(string text, float duration = -1f)
    {
        ShowSpeechBubbleTargetRpc(Owner, text, duration);
    }

    [TargetRpc]
    private void ShowSpeechBubbleTargetRpc(NetworkConnection connection, string text, float duration)
    {
        ClientBridge.Instance.PlayerHUD.HUD.ShowSpeechBubble(text, duration);
    }
}