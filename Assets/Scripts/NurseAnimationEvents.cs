using UnityEngine;

public class NurseAnimationEvents : MonoBehaviour
{
    private NurseEnemy nurse;

    private void Awake()
    {
        nurse = GetComponentInParent<NurseEnemy>();
    }

    public void NurseNormalAttackDamage()
    {
        if (nurse != null)
            nurse.NurseNormalAttackDamage();
    }

    public void EndNurseAttack()
    {
        if (nurse != null)
            nurse.EndNurseAttack();
    }

    public void NurseSkillDash()
    {
        if (nurse != null)
            nurse.NurseSkillDash();
    }

    public void NurseSkillDamage()
    {
        if (nurse != null)
            nurse.NurseSkillDamage();
    }

    public void EndNurseSkill()
    {
        if (nurse != null)
            nurse.EndNurseSkill();
    }
}