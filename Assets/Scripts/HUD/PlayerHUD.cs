using FishNet.Object;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class PlayerHUD : NetworkBehaviour
{
    [SerializeField] private GameObject hudPrefab;

    private HUD hud;

    public override void OnStartClient()
    {
        if (!IsOwner)
            return;

        var instance = Instantiate(hudPrefab);
        hud = instance.GetComponent<HUD>();

        var player = GetComponent<Player>();
        hud.Bind(player.GetComponent<LevelComponent>());
        hud.Bind(player.GetComponent<IHealth>());
        hud.Bind(player.GetComponent<IMana>());
    }
}