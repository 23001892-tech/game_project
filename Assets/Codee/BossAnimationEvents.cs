using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    private Boss boss;

    private void Awake()
    {
        boss = GetComponentInParent<Boss>();
    }

    private Boss GetBoss()
    {
        if (boss == null)
        {
            boss = GetComponentInParent<Boss>();
        }

        return boss;
    }

    public void Animation_BossImpactShockwave()
    {
        Boss bossScript = GetBoss();

        if (bossScript != null)
        {
            bossScript.Animation_BossImpactShockwave();
        }
    }

    public void Animation_BossDamageTarget()
    {
        Boss bossScript = GetBoss();

        if (bossScript != null)
        {
            bossScript.Animation_BossDamageTarget();
        }
    }

    public void Animation_BossFinishAttack()
    {
        Boss bossScript = GetBoss();

        if (bossScript != null)
        {
            bossScript.Animation_BossFinishAttack();
        }
    }

    public void Animation_FinishAttack()
    {
        Boss bossScript = GetBoss();

        if (bossScript != null)
        {
            bossScript.Animation_FinishAttack();
        }
    }
}