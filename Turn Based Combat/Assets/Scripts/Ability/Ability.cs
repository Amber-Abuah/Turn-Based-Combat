using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "ScriptableObjects/Ability")]
public class Ability : ScriptableObject
{
    [SerializeField] Element element;
    [SerializeField] string abilityName;
    [SerializeField] float multiplier;
    [SerializeField] int mpAmount;
    [SerializeField] GameObject particleEffect;
    [SerializeField] float timeUntilImpact;

    public Element Element { get { return element; } }
    public string Name { get { return abilityName; } }
    public float Multiplier { get { return multiplier; } }
    public int MpAmount { get { return mpAmount; } }
    public GameObject ParticleEffect { get { return particleEffect; } }
    public float TimeUntilImpact { get { return timeUntilImpact; } }
}
