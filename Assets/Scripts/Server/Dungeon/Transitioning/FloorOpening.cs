using System.Collections;
using UnityEngine;

public class FloorOpening : MonoBehaviour
{
    [SerializeField] private Transform[] rightTiles;
    [SerializeField] private Transform[] leftTiles;
    [SerializeField] private float dropDepth = 0.3f;
    [SerializeField] private float slideDistance = 3f;
    private float moveSpeed = 1f;

    public void OpenFloor()
    {
        StartCoroutine(AnimateOpening());
    }

    private IEnumerator AnimateOpening()
    {
        // Fixed: Mapped left to left, right to right
        Vector3[] leftStarts = GetStartPositions(leftTiles);
        Vector3[] rightStarts = GetStartPositions(rightTiles);

        // Phase 1: Move both sides down
        Coroutine dropLeft = StartCoroutine(MoveGroup(leftTiles, leftStarts, Vector3.down * dropDepth));
        Coroutine dropRight = StartCoroutine(MoveGroup(rightTiles, rightStarts, Vector3.down * dropDepth));
        yield return dropLeft;
        yield return dropRight;

        // Phase 2: Slide apart using local space vectors
        Vector3 leftTargetOffset = (Vector3.down * dropDepth) + (Vector3.left * slideDistance);
        Vector3 rightTargetOffset = (Vector3.down * dropDepth) + (Vector3.right * slideDistance);

        Coroutine moveLeft = StartCoroutine(MoveGroup(leftTiles, leftStarts, leftTargetOffset));
        Coroutine moveRight = StartCoroutine(MoveGroup(rightTiles, rightStarts, rightTargetOffset));
        yield return moveLeft;
        yield return moveRight;
    }

    private Vector3[] GetStartPositions(Transform[] tiles)
    {
        Vector3[] starts = new Vector3[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
            starts[i] = tiles[i].localPosition;
        return starts;
    }

    private IEnumerator MoveGroup(Transform[] tiles, Vector3[] startPositions, Vector3 offset)
    {
        bool moving = true;
        while (moving)
        {
            moving = false;
            for (int i = 0; i < tiles.Length; i++)
            {
                Vector3 target = startPositions[i] + offset;
                tiles[i].localPosition = Vector3.MoveTowards(
                    tiles[i].localPosition,
                    target,
                    moveSpeed * Time.deltaTime
                );

                if (Vector3.Distance(tiles[i].localPosition, target) > 0.001f)
                    moving = true;
            }
            yield return null;
        }
    }
}