using UnityEngine;

public class ClientBridge : MonoBehaviour
{
    public static ClientBridge Instance { get; private set; }
    public VFXView VFXView { get; private set; }
    public IAbilitySystem AbilitySystem { get; private set; }
    public IPlayerMovement Movement { get; private set; }

    private void Awake()
    {
        Instance = this;
        VFXView = GetComponentInChildren<VFXView>();
    }

    public void RegisterPlayer(Player player)
    {
        AbilitySystem = player.GetComponentInChildren<IAbilitySystem>();
        Movement = player.GetComponent<IPlayerMovement>();
    }
}