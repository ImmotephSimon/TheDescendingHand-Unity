using UnityEngine;

public class BossLocation : MonoBehaviour
{
    [SerializeField] private FloorOpening floorOpening;
    [SerializeField] private EnemyDefinition bossDefinition;
    [SerializeField] private Transform bossTransform;
    
    private Enemy _boss;
    private EnemySpawner spawner;

    private void Start()
    {
        spawner = GetComponent<EnemySpawner>();
        if (spawner == null)
        {
            Debug.LogError($"{name}: BossLocation requires EnemySpawner.", this);
            return;
        }

        spawner.enemyDefinition = bossDefinition;
        spawner.fixedSpawnPoint = bossTransform;
        spawner.OnSpawned += HandleEnemySpawned;
    }

    private void HandleEnemySpawned(Enemy enemy)
    {
        spawner.OnSpawned -= HandleEnemySpawned;
        if (_boss != null) return;

        _boss = enemy;
        enemy.Died += OnEnemyDied;
    }

    private void OnValidate()
    {
        if (floorOpening == null)
            Debug.LogError($"{name}: FloorOpening is not assigned on {GetType().Name}!", this);

        if (bossDefinition == null)
            Debug.LogError($"{name}: Boss prefab is not assigned on {GetType().Name}!", this);

        if (bossTransform == null)
            Debug.LogError($"{name}: Boss transform is not assigned on {GetType().Name}!", this);
    }


    private void OnDisable()
    {
        if (_boss != null)
            _boss.Died -= OnEnemyDied;
    }


    private void OnEnemyDied(IEntity enemy)
    {
        enemy.Died -= OnEnemyDied;
        floorOpening.OpenFloor();
        DungeonManager.Instance.OnDungeonCompleted(this);
    }
}