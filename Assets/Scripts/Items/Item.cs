using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

public class Item : NetworkBehaviour, IInteractable
{
    private readonly SyncVar<string> _definitionId = new SyncVar<string>();

    private ItemDefinition _definition;
    private GameObject _visual;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Rarity _rarity;
    private AffixInstance[] _affixes;

    public ItemDefinition Definition => _definition;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _definitionId.OnChange += OnDefinitionIdChanged;
    }

    public void Initialize(ItemDefinition definition)
    {
        _definition = definition;

        foreach (var component in _definition.Components)
            component.Initialize(this);

        _definitionId.Value = definition.ID;
    }

    private void OnDefinitionIdChanged(string prev, string next, bool asServer)
    {
        if (string.IsNullOrEmpty(next)) return;

        if (ItemDatabase.Instance.TryGet(next, out var def))
        {
            _definition = def;
            ApplyVisuals(def);
        }
    }

    private void ApplyVisuals(ItemDefinition def)
    {
        if (!ItemDatabase.Instance.TryGet(def.ID, out var definition)) return;
        if (definition.Appearance?.WorldModel == null) return;

        var sourceFilter = definition.Appearance.WorldModel.GetComponent<MeshFilter>();
        var sourceRenderer = definition.Appearance.WorldModel.GetComponent<MeshRenderer>();

        if (sourceFilter != null) _meshFilter.sharedMesh = sourceFilter.sharedMesh;
        if (sourceRenderer != null) _meshRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
    }

    public void Interact(Player player)
    {
        if (IsServerStarted) TryPickup(player);
        else RequestPickupServerRpc();
    }

    [ServerRpc]
    private void RequestPickupServerRpc(NetworkConnection sender = null)
    {
        Player player = sender.FirstObject.GetComponent<Player>();

        if (Vector3.Distance(player.transform.position, transform.position) > player.InteractRange)
            return;

        TryPickup(player);
    }

    private void TryPickup(Player player)
    {
        var instance = new ItemInstance
        {
            BaseType = _definition,
            Rarity = _rarity,
            Affixes = _affixes
        };

        if (player.GetComponent<IInventory>().TryAdd(instance))
        {
            Despawn(gameObject);
        }
    }

    public void Shutdown()
    {
        foreach (var component in _definition.Components)
            component.Shutdown();
    }

    internal void SetVisible(bool isVisible)
    {
        _visual.SetActive(isVisible);
    }
}