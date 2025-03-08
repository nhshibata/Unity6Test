using UnityEngine;

public class ShootingController : MonoBehaviour
{

    private void Awake()
    {
        EnemyScoreEvent.OnEnemyDamaged += OnEnemyDamaged;
        EnemyScoreEvent.OnEnemyDefeated += OnEnemyDefeated;
        PlayerHealthEvent.OnPlayerDead += OnPlayerDead;
        PlayerHealthEvent.OnHealthChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        EnemyScoreEvent.OnEnemyDamaged -= OnEnemyDamaged;
        EnemyScoreEvent.OnEnemyDefeated -= OnEnemyDefeated;
        PlayerHealthEvent.OnPlayerDead -= OnPlayerDead;
        PlayerHealthEvent.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int currentHp)
    {
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
        Debug.Log($"Enemy {num}");
    }

}
