using UnityEngine;

public class Stairs : MonoBehaviour
{
    [SerializeField] private Transform dungeonAnchor;
    public void Transition(bool goingDown)
    {

        if (goingDown)
        {
            DungeonManager.Instance.EnterDungeon(dungeonAnchor);
        }
        else
        {
            DungeonManager.Instance.LeaveDungeon(dungeonAnchor);
        }
    }
}