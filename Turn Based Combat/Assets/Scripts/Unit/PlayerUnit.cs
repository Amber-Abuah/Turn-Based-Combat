using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    [SerializeField] string attackName;
    public string AttackName { get { return attackName; } }

    public override void SelectUnit()
    {
        base.SelectUnit();
        unitUI.Enlarge();
    }

    public override void DeselectUnit()
    {
        base.DeselectUnit();
        unitUI.ReturnToOriginalSize();
    }

    public override IEnumerator Attack(Unit unitToAttack)
    {
        yield return AttackCoroutine(unitToAttack, true);
    }

    public override IEnumerator Ability(Unit unitToAttack)
    {
        yield return AbilityCoroutine(unitToAttack, true);
    }

    public void Victory()
    {
        animator.SetTrigger("Victory");
        HideWeapons();
        unitUI.HideUI();
    }

    protected override void Die()
    {
        base.Die();
    }

    public bool IsEligibleForItem(Item item)
    {
        switch (item.ItemType)
        {
            case ItemType.REVIVE:
                return IsDead;
            case ItemType.CUREAILMENT:
                return false;
            case ItemType.HEAL:
                return !IsDead;
        }

        return true;
    }

    protected override void PlayAttackSFX()
    {
        SFXManager.Instance.PlayerAttackSound(weaponType);
    }

    public void ShowUI()
    {
        unitUI.ShowUI();
    }
}
