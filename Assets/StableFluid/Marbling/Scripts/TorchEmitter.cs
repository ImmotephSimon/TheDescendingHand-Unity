using UnityEngine;

public sealed class TorchEmitter
{
    public Vector2 Position { get; set; }
    public Vector2 Force { get; private set; }
    public Color Color { get; } = new Color(1f, 0.5f, 0.1f);

    public TorchEmitter(Vector2 position)
    {
        Position = position;
    }

    public void Update()
    {
        Force = Vector2.up * 50f;
    }
}