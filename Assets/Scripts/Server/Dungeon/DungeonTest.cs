using UnityEngine;

public class TestDungeon : MonoBehaviour
{
    [SerializeField] private Room roomStart;
    [SerializeField] private Room roomEnd;

    void Start()
    {
        Debug.Log("Dungeon test started");

    }

    void SnapRoom(
        DoorSocket parentDoor,
        Transform childRoom,
        DoorSocket childDoor)
    {
        Quaternion rotation = Quaternion.FromToRotation(
            childDoor.transform.forward,
            -parentDoor.transform.forward
        );

        childRoom.rotation = rotation * childRoom.rotation;

        Vector3 offset = childDoor.transform.position - parentDoor.transform.position;
        childRoom.position -= offset;
    }
}