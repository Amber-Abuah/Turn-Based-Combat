using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "ScriptableObjects/Non attack ability")]
public class NonAttackAbility : ScriptableObject
{
    [SerializeField] string abilityName;
    [SerializeField] int healAmount;
    [SerializeField] int mpAmount;

    public string Name { get { return abilityName; } }
    public int HealAmount { get { return healAmount; } }
    public int MpAmount { get { return mpAmount; } }

    public void UseAbility(Unit unit)
    {
        unit.Heal(healAmount);
    }
}
