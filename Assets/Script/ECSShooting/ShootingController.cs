using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class ShootingController : MonoBehaviour
{
    [SerializeField]
    private ShootingView shootingView;

    private int enemyDeth = 0;

    private void Awake()
    {
        EnemyScoreEvent.OnEnemyDamaged += OnEnemyDamaged;
        EnemyScoreEvent.OnEnemyDefeated += OnEnemyDefeated;
        PlayerHealthEvent.OnPlayerDead += OnPlayerDead;
        PlayerHealthEvent.OnHealthChanged += OnHealthChanged;
        GoalEvent.OnGoalEvent += OnGoalEvent;
    }

    private void OnDestroy()
    {
        EnemyScoreEvent.OnEnemyDamaged -= OnEnemyDamaged;
        EnemyScoreEvent.OnEnemyDefeated -= OnEnemyDefeated;
        PlayerHealthEvent.OnPlayerDead -= OnPlayerDead;
        PlayerHealthEvent.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int maxHp, int currentHp)
    {
        shootingView.SetHp(maxHp, currentHp);
        Debug.Log($"HP:{currentHp}");
    }

    private void OnPlayerDead()
    {
        Debug.Log("Player HP:0");
    }

    public void OnEnemyDamaged(int damage)
    {
        Debug.Log($"Enemy damage:{damage}");
    }

    public void OnEnemyDefeated(int num)
    {
        enemyDeth += num;
        shootingView.SetEnemyText(enemyDeth);
        Debug.Log($"Enemy {num}");
    }

    private void OnGoalEvent()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(
            ComponentType.ReadOnly<PlayerData>(),
            ComponentType.ReadOnly<LocalTransform>());
        var entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            var data = manager.GetComponentData<PlayerData>(entity);
            data.AutoMoveForward = false;
            break;
        }
    }

}

