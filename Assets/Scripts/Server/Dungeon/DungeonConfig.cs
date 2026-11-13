using FishNet.Managing.Object;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonConfig", menuName = "Dungeon/Dungeon Config")]
public class DungeonConfig : ScriptableObject
{
    public DualPrefabObjects[] startRooms;
    public DualPrefabObjects[] standardRooms;
    public DualPrefabObjects[] bossRooms;
    public DualPrefabObjects deadEndRoom;
}