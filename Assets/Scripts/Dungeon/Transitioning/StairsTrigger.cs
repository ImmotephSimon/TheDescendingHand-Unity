using UnityEngine;

public class StairsTrigger : MonoBehaviour
{
    [SerializeField] private bool goingDown;
    [SerializeField] private Stairs stairs;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        stairs.Transition(goingDown);
    }
}