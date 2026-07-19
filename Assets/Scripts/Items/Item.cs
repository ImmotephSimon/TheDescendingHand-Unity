using UnityEngine;

public class Item : MonoBehaviour
{
    private ItemComponent[] components;

    void Awake()
    {
        components = GetComponents<ItemComponent>();
    }

    public void Initialize()
    {
        foreach (var component in components)
            component.Initialize(this);
    }

    public void Shutdown()
    {
        foreach (var component in components)
            component.Shutdown();
    }
}