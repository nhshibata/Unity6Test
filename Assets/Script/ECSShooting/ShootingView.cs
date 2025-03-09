using TMPro;
using UnityEngine;

public class ShootingView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private HeartbeatEffect heartbeatEffect;

    public void SetEnemyText(int num)
    {
        text.text = num.ToString();
    }

    public void SetHp(int maxHp, int hp)
    {
        heartbeatEffect.SetHealthPercentage((float)hp / maxHp);
    }
}