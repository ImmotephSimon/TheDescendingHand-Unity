using FishNet.Connection;
using FishNet.Object;
using System;

public interface IPlayerNetworkActionsServerHandler
{
    void RequestInteract();
}

public class PlayerNetworkActions : NetworkBehaviour
{
    private IPlayerNetworkActionsServerHandler _serverHandler;

    public static event Action<string, float> SpeechBubbleReceived;

    public event Action<NetworkObject, bool> BossBarChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();

        _serverHandler = GetComponent<IPlayerNetworkActionsServerHandler>();

        if (_serverHandler == null)
            throw new InvalidOperationException(
                $"{nameof(IPlayerNetworkActionsServerHandler)} is missing on {gameObject.name}.");
    }

    [ServerRpc]
    public void RequestInteract()
    {
        _serverHandler.RequestInteract();
    }

    public void ShowSpeechBubble(string text, float duration = -1f)
    {
        ShowSpeechBubbleTargetRpc(Owner, text, duration);
    }

    [TargetRpc]
    private void ShowSpeechBubbleTargetRpc(
        NetworkConnection connection, string text, float duration)
    {
        SpeechBubbleReceived?.Invoke(text, duration);
    }


    public void ShowBossBar(NetworkObject boss) => BossBarRpc(Owner, boss, true);
    public void HideBossBar() => BossBarRpc(Owner, null, false);

    [TargetRpc]
    private void BossBarRpc(NetworkConnection c, NetworkObject boss, bool show) => BossBarChanged?.Invoke(boss, show);
}