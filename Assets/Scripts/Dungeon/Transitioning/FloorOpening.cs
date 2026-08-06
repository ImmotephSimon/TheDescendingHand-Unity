using System.Collections;
using UnityEngine;

public class FloorOpening : MonoBehaviour
{
    [SerializeField] private Transform[] rightTiles;
    [SerializeField] private Transform[] leftTiles;
    [SerializeField] private float dropDepth = 0.3f;
    [SerializeField] private float slideDistance = 3f; // Set to roomUnit*openWidth (3f*1)
    private float moveSpeed = 1f;

    public void OpenFloor()
    {
        StartCoroutine(AnimateOpening());
    }

    private IEnumerator AnimateOpening()
    {
        Vector3[] leftStarts = GetStartPositions(rightTiles);
        Vector3[] rightStarts = GetStartPositions(leftTiles);

        // Phase 1: Move both sides down
        Coroutine dropLeft = StartCoroutine(MoveGroup(rightTiles, leftStarts, Vector3.down * dropDepth));
        Coroutine dropRight = StartCoroutine(MoveGroup(leftTiles, rightStarts, Vector3.down * dropDepth));
        yield return dropLeft;
        yield return dropRight;

        // Phase 2: Slide left side (-right) and right side (+right)
        Vector3 leftTargetOffset = (Vector3.down * dropDepth) - (transform.right * slideDistance);
        Vector3 rightTargetOffset = (Vector3.down * dropDepth) + (transform.right * slideDistance);

        Coroutine moveLeft = StartCoroutine(MoveGroup(rightTiles, leftStarts, leftTargetOffset));
        Coroutine moveRight = StartCoroutine(MoveGroup(leftTiles, rightStarts, rightTargetOffset));
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