using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFXManager : MonoBehaviour
{
    [SerializeField] ParticleSystem potionHealPS;
    [SerializeField] ParticleSystem revivePS;
    [SerializeField] ParticleSystem magicAttackPS;
    [SerializeField] ParticleSystem ultimatePS;

    [SerializeField] Ability[] abilities;
    Dictionary<Ability, ParticleSystem> abilityToPFX;

    public static ParticleFXManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        abilityToPFX = new Dictionary<Ability, ParticleSystem>();

        for (int i = 0; i < abilities.Length; i++)
        {
            GameObject pfx = Instantiate(abilities[i].ParticleEffect) as GameObject;
            pfx.transform.SetParent(transform.GetChild(0));

            abilityToPFX.Add(abilities[i], pfx.GetComponent<ParticleSystem>());
        }
    }

    public void Ability(Ability ability, Vector3 position)
    {
        StartCoroutine(ParticleFX(abilityToPFX[ability], position));
    }

    public void Ultimate(Vector3 position)
    {
        StartCoroutine(ParticleFX(ultimatePS, position));
    }

    public void PotionHealFX(Vector3 position)
    {
        StartCoroutine(ParticleFX(potionHealPS, position));
    }

    public void ReviveFX(Vector3 position)
    {
        StartCoroutine(ParticleFX(revivePS, position));
    }

    public void MagicAttackFX(Vector3 position)
    {
        StartCoroutine(ParticleFX(magicAttackPS, position, true));
    }

    IEnumerator ParticleFX(ParticleSystem ps, Vector3 position, bool instantEnd = false)
    {
        ps.transform.position = position;
        ps.Play();

        yield return new WaitForSeconds(2f);

        if (instantEnd)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        else
            ps.Stop();
    }
}
