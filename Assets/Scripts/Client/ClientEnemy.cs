using UnityEngine;

public class ClientEnemy : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;

    private void Start()
    {
        Instantiate(healthBarPrefab, transform);
    }
}