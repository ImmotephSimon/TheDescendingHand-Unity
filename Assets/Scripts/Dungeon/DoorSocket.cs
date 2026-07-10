using UnityEngine;

public class DoorSocket : MonoBehaviour
{
    public enum DoorType
    {
        Normal,
        Entrance
    }
    public enum DoorSide
    {
        North,
        South,
        East,
        West
    }

    [SerializeField] private DoorSide side;

    [Tooltip("X = position along wall edge. Y = vertical position within room bounds.")]
    [SerializeField] private Vector2 offset;

    [SerializeField] private DoorType type;
    public DoorSide Side => side;
    public Vector2 Offset => offset;
    public DoorType Type => type;
    private DoorSocket connectedDoor;
    public DoorSocket ConnectedDoor => connectedDoor;
    public bool IsConnected => connectedDoor != null;

    public void UpdatePosition(BoxCollider bounds, float roomUnit)
    {
        Vector3 position = bounds.center + GetSideOffset(side, bounds.size);

        position.y += offset.y * roomUnit;

        if (side == DoorSide.North || side == DoorSide.South)
            position.x += offset.x * roomUnit;
        else
            position.z += offset.x * roomUnit;

        transform.localPosition = position;
        transform.localRotation = Quaternion.LookRotation(GetForward(side));
    }

    private Vector3 GetSideOffset(DoorSide side, Vector3 size)
    {
        return side switch
        {
            DoorSide.North => new Vector3(0, 0, size.z / 2),
            DoorSide.South => new Vector3(0, 0, -size.z / 2),
            DoorSide.East => new Vector3(size.x / 2, 0, 0),
            DoorSide.West => new Vector3(-size.x / 2, 0, 0),
            _ => Vector3.zero
        };
    }

    private Vector3 GetForward(DoorSide side)
    {
        return side switch
        {
            DoorSide.North => Vector3.forward,
            DoorSide.South => Vector3.back,
            DoorSide.East => Vector3.right,
            DoorSide.West => Vector3.left,
            _ => Vector3.zero
        };
    }

    private void OnValidate()
    {
        GetComponentInParent<Room>()?.UpdateDoors();
    }

    

    public void Connect(DoorSocket other)
    {
        connectedDoor = other;
    }

    public void Disconnect()
    {
        connectedDoor = null;
    }

}