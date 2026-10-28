using UnityEngine;

public class BossLocation : MonoBehaviour
{
    [SerializeField] private FloorOpening floorOpening;
    [SerializeField] private Transform bossTransform;
    [SerializeField] private Transform stairsAnchor;
    private EnemySpawner spawner;

    private void Start()
    {
        spawner = GetComponent<EnemySpawner>();
        if (spawner == null)
        {
            Debug.LogError($"{name}: BossLocation requires EnemySpawner.", this);
            return;
        }

        spawner.OnSpawned += HandleEnemySpawned;
    }

    private void HandleEnemySpawned(Enemy enemy)
    {
        spawner.OnSpawned -= HandleEnemySpawned;

        enemy.Died += OnEnemyDied;
    }

    private void OnValidate()
    {
        if (floorOpening == null)
            Debug.LogError($"{name}: FloorOpening is not assigned on {GetType().Name}!", this);

        if (bossTransform == null)
            Debug.LogError($"{name}: Boss transform is not assigned on {GetType().Name}!", this);
    }



    private void OnEnemyDied(IEntity enemy)
    {
        enemy.Died -= OnEnemyDied;
        floorOpening.OpenFloor();
        DungeonManager.Instance.OnDungeonCompleted(stairsAnchor);
    }
}