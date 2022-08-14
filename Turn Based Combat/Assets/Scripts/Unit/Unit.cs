using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    [SerializeField] int maxHp;
    [SerializeField] int currentHp;

    [SerializeField] int maxMp;
    [SerializeField] int currentMp;

    [SerializeField] protected string unitName;
    [SerializeField] int attackPower;
    [SerializeField] int magicPower;
    [SerializeField] int luck;
    [SerializeField] int critRatePercent;
    [SerializeField] protected UnitUI unitUI;

    [SerializeField] Ability ability;
    [SerializeField] Element element;
    [SerializeField] protected WeaponType weaponType;

    [SerializeField] bool closeUpAttack;
    [SerializeField] bool closeUpAbility;

    [SerializeField] GameObject selectionPointer;
    [SerializeField] GameObject castRing;
    [SerializeField] GameObject ultimateCastRing;
    [SerializeField] ParticleSystem[] weaponTrails;
    [SerializeField] GameObject[] weapons;
    [SerializeField] Transform centrePos;

    protected Animator animator;
    NavController navController;
    Vector3 originalPosition;
    Quaternion originalRotation;

    protected BattleSystem battleSystem;
    protected SFXManager sfxManager;
    BattleSystemUI battleSystemUI;
    ParticleFXManager particleFXManager;
    CameraController cameraController;

    HitEffect hitEffect;
    [SerializeField] Transform hitEffectPosition;

    bool isAttacking;
    bool isCasting;

    public string Name { get { return unitName; } }
    public int Luck { get { return luck; } }
    public bool IsDead { get { return currentHp == 0; } }
    public Element Element { get { return element; } }
    public Ability Abilities { get { return ability; } }

    protected void Start()
    {
        unitUI.HideUI();

        battleSystem = BattleSystem.Instance;
        battleSystemUI = BattleSystem.Instance.GetComponent<BattleSystemUI>();
        sfxManager = SFXManager.Instance;
        particleFXManager = ParticleFXManager.Instance;
        cameraController = CameraController.Instance;

        animator = GetComponent<Animator>();
        navController = GetComponent<NavController>();

        currentHp = maxHp;
        currentMp = maxMp;

        selectionPointer.SetActive(false);

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        UnitSpawner.Instance.CreateHitEffect(this);

        unitUI.SetName(unitName);
        unitUI.SetHpBar(currentHp, maxHp);
        unitUI.SetMpBar(currentMp, maxMp);
    }

    public void SetUI(GameObject hitEffectPrefab, Transform worldCanvas)
    {
        GameObject go = Instantiate(hitEffectPrefab, worldCanvas) as GameObject;

        hitEffect = go.GetComponent<HitEffect>();

        hitEffect.gameObject.name = gameObject.name + " Hit Effect";

        if(hitEffectPosition != null)
            hitEffect.gameObject.transform.position = hitEffectPosition.position;
    }

    public virtual void SelectUnit()
    {
        selectionPointer.SetActive(true);
    }

    public virtual void DeselectUnit()
    {
        selectionPointer.SetActive(false);
    }

    void TakeDamage(int damage, bool isCritical, bool isWeakness, bool isResist)
    {
        currentHp = Mathf.Clamp(currentHp - damage, 0, maxHp);

        unitUI.SetHpBar(currentHp, maxHp);

        hitEffect.Hit(damage, isCritical, isWeakness, isResist);

        if (currentHp == 0)
            Die();
        else
            animator.SetTrigger("TakeDamage");
    }

    public void Heal(int amount)
    {
        currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);
        unitUI.SetHpBar(currentHp, maxHp);
    }

    protected virtual void Die() 
    { 
        animator.SetTrigger("Die");
        HideWeapons();
    }

    protected bool OpposingElement(Element element)
    {
        switch (element)
        {
            case Element.NUETRAL:
                return false;
            case Element.WATER:
                return this.element == Element.FIRE;
            case Element.FIRE:
                return this.element == Element.WATER;
            case Element.EARTH:
                return this.element == Element.WIND;
            case Element.WIND:
                return this.element == Element.EARTH;
        }

        return false;
    }

    IEnumerator MoveToTarget(Vector3 position)
    {
        animator.SetTrigger("Locomotion");

        while (navController.MoveToTarget(position) > 1.2f)
            yield return null;

        animator.SetTrigger("Default");
    }

    int Crit(int baseDamage)
    {
        int random = Random.Range(0, 101);

        if(random <= critRatePercent)
            return (int)(baseDamage * Random.Range(1.1f, 1.3f));
        else
            return (int)(baseDamage * Random.Range(0.85f, 1.01f));
    }

    public virtual IEnumerator Attack(Unit unitToAttack)
    {
        yield return AttackCoroutine(unitToAttack);
    }

    protected IEnumerator AttackCoroutine(Unit unitToAttack, bool cameraFollow = false)
    {
        if (closeUpAttack)
        {
            if(cameraFollow)
                cameraController.FollowPlayer(this);

            yield return MoveToTarget(unitToAttack.transform.position);
        }
            
        animator.SetTrigger("Attack");

        while (!isAttacking) yield return null;

        if (weaponType == WeaponType.TOME)
            particleFXManager.MagicAttackFX(unitToAttack.centrePos.position);

        int damage = Crit(attackPower);
        unitToAttack.TakeDamage(damage, damage > attackPower, false, false);

        battleSystemUI.ShowDialogue(unitName + " attacks " + unitToAttack.unitName + " and deals "
            + damage + " damage!" + (damage > attackPower ? " Critical Hit!" : ""));

        while (isAttacking) yield return null;

        battleSystem.AddUltimatePoints(damage);

        if (unitToAttack.IsDead)
            battleSystemUI.ShowDialogue(unitToAttack.unitName + " has been defeated!");

        if (closeUpAttack)
            yield return MoveToTarget(originalPosition);
        else
            yield return new WaitForSeconds(2f);

        yield return ReturnToOriginalPosRot();

        if(closeUpAttack && cameraFollow)
            cameraController.StopFollowPlayer();
    }

    IEnumerator ReturnToOriginalPosRot()
    {
        navController.DisableNavMesh();

        while ((transform.position - originalPosition).magnitude > .2f)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, 2f * Time.deltaTime);

            Quaternion newRotation = Quaternion.Lerp(transform.rotation, originalRotation, 5f * Time.deltaTime);
            transform.rotation = newRotation;

            yield return null;
        }

        navController.EnableNavMesh();
    }

    public virtual IEnumerator Ability(Unit unitToAttack)
    {
        yield return AbilityCoroutine(unitToAttack);
    }

    protected IEnumerator AbilityCoroutine(Unit unitToAttack, bool cameraShot = false)
    {
        if (closeUpAbility)
            yield return MoveToTarget(unitToAttack.transform.position);

        animator.SetTrigger("Ability");

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(false);

        isCasting = true;

        currentMp = currentMp - ability.MpAmount;
        unitUI.SetMpBar(currentMp, maxMp);

        castRing.GetComponent<Animator>().SetTrigger("Cast");

        sfxManager.PlayCastFX();

        if (cameraShot)
        {
            while (isCasting) yield return null;
            cameraController.CloseUpZoomOut(unitToAttack);
        }
        else
            yield return new WaitForSeconds(1.5f);

        if(ability.Element == Element.EARTH)
            particleFXManager.Ability(ability, unitToAttack.centrePos.position);
        else
            particleFXManager.Ability(ability, unitToAttack.transform.position);

        yield return new WaitForSeconds(ability.TimeUntilImpact);

        bool affinity = unitToAttack.OpposingElement(ability.Element);

        int damage = (int)(affinity ? magicPower * ability.Multiplier * 1.5f * Random.Range(0.85f, 1.01f)
            : magicPower * ability.Multiplier * Random.Range(0.85f, 1.1f));

        bool isResistant = ability.Element == unitToAttack.element;

        if (isResistant)
            damage = (int)(damage * .3f);

        unitToAttack.TakeDamage(damage, false, affinity, isResistant);

        sfxManager.PlayImpactSound();

        battleSystemUI.ShowDialogue(unitName + " uses " + ability.Name +  " on " + unitToAttack.unitName + " and deals "
           + damage + " damage!" + (affinity? " Weakness!" : isResistant? " It's not very effective..." : ""));

        battleSystem.AddUltimatePoints(damage);

        sfxManager.PlayAbilitySound(weaponType);

        if (unitToAttack.IsDead)
            battleSystemUI.ShowDialogue(unitToAttack.unitName + " has been defeated!");

        yield return new WaitForSeconds(2.5f);

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(true);

        if (closeUpAbility)
            yield return MoveToTarget(originalPosition);
        else
            yield return new WaitForSeconds(1f);

        yield return ReturnToOriginalPosRot();

        if(cameraShot)
            cameraController.ResetToBackShot();
    }

    public bool CanUseAbility()
    {
        return currentMp >= ability.MpAmount;
    }

    public IEnumerator UseItem(Item item, PlayerUnit targetUnit)
    {
        yield return UseItemCoroutine(item, targetUnit);
    }

    IEnumerator UseItemCoroutine(Item item, PlayerUnit targetUnit)
    {
        HideWeapons();

        animator.SetTrigger("UseItem");

        targetUnit.currentHp = Mathf.Clamp(targetUnit.currentHp + item.HealAmount, 0, targetUnit.maxHp);
        targetUnit.unitUI.SetHpBar(targetUnit.currentHp, targetUnit.maxHp);

        sfxManager.PlayItemSound(item);

        if (item.ItemType == ItemType.HEAL)
        {
            battleSystemUI.ShowDialogue(unitName + " uses " + item.ItemName +
                (targetUnit == this ? "" : " on " + targetUnit.Name) + " and heals " + item.HealAmount + " points!");

            particleFXManager.PotionHealFX(targetUnit.transform.position);
        }
        else if(item.ItemType == ItemType.REVIVE)
        {
            battleSystemUI.ShowDialogue(targetUnit.unitName + " is revived!");

            targetUnit.animator.SetTrigger("Revive");

            particleFXManager.ReviveFX(targetUnit.transform.position);
        }

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(true);
    }

    public IEnumerator UltimateCast()
    {
        cameraController.Ultimate();

        animator.SetTrigger("Ultimate");

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(false);

        ultimateCastRing.GetComponent<Animator>().SetTrigger("Cast");

        sfxManager.PlayUltimateCastFX();

        yield return new WaitForSeconds(2f);
    }

    public IEnumerator UltimateAttack(Unit unitToAttack, int damage)
    {
        particleFXManager.Ultimate(unitToAttack.centrePos.position);

        yield return new WaitForSeconds(2f);

        unitToAttack.TakeDamage(damage, false, false, false);

        sfxManager.PlayImpactSound();

        battleSystemUI.ShowDialogue("Ultimate deals " + damage + " damage to " + unitToAttack.Name + "!");

        if (unitToAttack.IsDead)
            battleSystemUI.ShowDialogue(unitToAttack.unitName + " has been defeated!");

        yield return new WaitForSeconds(2.5f);

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(true);
    }

    public int UltimateDamage()
    {
        if (attackPower > magicPower * ability.Multiplier)
            return (int)(attackPower * Random.Range(1.1f, 1.5f));
        else
            return (int)(magicPower * ability.Multiplier * Random.Range(1.1f, 1.5f));
    }

    public void UltimateEnd()
    {
        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(true);
    }

    protected virtual void PlayAttackSFX() { }

    protected void HideWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(false);
        }
    }

    // Animation events
    public void AttackStart()
    {
        isAttacking = true;

        foreach (ParticleSystem trail in weaponTrails)
            trail.Play();

        PlayAttackSFX();
    }

    public void AttackEnd()
    {
        isAttacking = false;

        foreach (ParticleSystem trail in weaponTrails)
            trail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void CastStart()
    {
        cameraController.CloseUp(this);
    }

    public void CastEnd()
    {
        isCasting = false;
    }
}
