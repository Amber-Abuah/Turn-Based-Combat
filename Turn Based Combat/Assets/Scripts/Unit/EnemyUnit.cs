using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyUnit : Unit
{
    [SerializeField] Transform uiPos;
    [SerializeField] int enemyDeathValue;
    [SerializeField] int enemyAttackSFX;
    [SerializeField] bool abilityRandomTarget;

    private void Start()
    {
        unitUI = UnitSpawner.Instance.CreateEnemyUI(uiPos).GetComponent<UnitUI>();
        base.Start();
        unitUI.HideUI();
    }

    public override void SelectUnit()
    {
        base.SelectUnit();
        unitUI.ShowUI();
    }

    public override void DeselectUnit()
    {
        base.DeselectUnit();
        unitUI.HideUI();
    }

    public Unit SelectRandomUnit(PlayerUnit[] playerUnits)
    {
        int random = Random.Range(0, playerUnits.Length);

        while (playerUnits[random].IsDead)
            random = Random.Range(0, playerUnits.Length);

        return playerUnits[random];
    }

    public Unit SelectWeakUnit(PlayerUnit[] playerUnits)
    {
        List<PlayerUnit> sortedPlayers = new List<PlayerUnit>();
        sortedPlayers.AddRange(playerUnits);

        sortedPlayers = sortedPlayers.OrderByDescending(e => OpposingElement(e.Element)).ToList();

        int index = 0;

        while (sortedPlayers[index].IsDead)
            index++;

        return sortedPlayers[index];
    }

    protected override void Die()
    {
        base.Die();
        battleSystem.RemoveDefeatedEnemy(this);
        battleSystem.AddUltimatePoints(enemyDeathValue);
        StartCoroutine(DieCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        yield return new WaitForSeconds(2f);

        gameObject.SetActive(false);
        unitUI.gameObject.SetActive(false);
    }

    public IEnumerator ChooseAttack(PlayerUnit[] players)
    {
        int random = Random.Range(0, 2);

        if (random == 1 && CanUseAbility())
        {
            if (abilityRandomTarget)
                yield return Ability(SelectRandomUnit(players));
            else
                yield return Ability(SelectWeakUnit(players));
        }
        else
            yield return Attack(SelectRandomUnit(players));
    }

    protected override void PlayAttackSFX()
    {
        sfxManager.EnemyAttackSound(enemyAttackSFX);
    }
}