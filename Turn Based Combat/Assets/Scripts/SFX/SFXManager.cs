using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField] AudioSource[] sources;
    AudioSource[] playerAttackSounds;
    AudioSource[] enemyAttackSounds;
    AudioSource[] abilitySounds;
    AudioSource[] itemSounds;
    AudioSource[] menuSounds;
    AudioSource[] impactSounds;
    AudioSource[] ultimateSounds;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerAttackSounds = transform.GetChild(0).GetComponentsInChildren<AudioSource>();
        abilitySounds = transform.GetChild(1).GetComponentsInChildren<AudioSource>();
        enemyAttackSounds = transform.GetChild(2).GetComponentsInChildren<AudioSource>();
        itemSounds = transform.GetChild(3).GetComponentsInChildren<AudioSource>();
        menuSounds = transform.GetChild(4).GetComponentsInChildren<AudioSource>();
        impactSounds = transform.GetChild(5).GetComponentsInChildren<AudioSource>();
        ultimateSounds = transform.GetChild(6).GetComponentsInChildren<AudioSource>();
    }

    public void PlayMenuSFX(int index)
    {
        menuSounds[index].Play();
    }

    public void PlayImpactSound()
    {
        impactSounds[0].Play();
    }

    public void PlayerAttackSound(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.SWORD:
                playerAttackSounds[0].Play();
                break;
            case WeaponType.FIST:
                playerAttackSounds[1].Play();
                break;
            case WeaponType.DAGGERS:
                playerAttackSounds[2].Play();
                break;
            case WeaponType.TOME:
                playerAttackSounds[3].Play();
                break;
        }
    }

    public void EnemyAttackSound(int index)
    {
        enemyAttackSounds[index].Play();
    }

    public void PlayAbilitySound(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.TOME:
                abilitySounds[0].Play();
                break;
            case WeaponType.DAGGERS:
                abilitySounds[1].Play();
                break;
            case WeaponType.SWORD:
                abilitySounds[2].Play();
                break;
            case WeaponType.FIST:
                abilitySounds[3].Play();
                break;
        }
    }

    public void PlayItemSound(Item item)
    {
        switch (item.ItemType)
        {
            case ItemType.HEAL:
                itemSounds[0].Play();
                break;
            case ItemType.REVIVE:
                itemSounds[1].Play();
                break;
        }
    }

    public void PlayCastFX()
    {
        abilitySounds[4].Play();
    }

    public void PlayUltimateCastFX()
    {
        ultimateSounds[0].Play();
    }
}
