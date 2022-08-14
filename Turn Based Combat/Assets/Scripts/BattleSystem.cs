using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] PlayerUnit[] players;
    [SerializeField] List<EnemyUnit> enemies;

    List<Unit> unitTurns;
    int currentUnitTurn;
    Unit currentUnit;
    int currentEnemySelected;

    public UnityEvent onBattleStart;
    public UnityEvent onPlayerTurnStart;
    public UnityEvent onPlayerAction;
    public UnityEvent onEnemyTurnStart;
    public UnityEvent onBattleEnd;

    bool isPlayerTurn;
    bool canSelectEnemy;
    bool battleOver;

    BattleUltimate battleUltimate;
    SFXManager sfxManager;
    BattleSystemUI battleSystemUI;
    public static BattleSystem Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        battleUltimate = GetComponent<BattleUltimate>();
        sfxManager = SFXManager.Instance;
        battleSystemUI = GetComponent<BattleSystemUI>();

        unitTurns = new List<Unit>();

        DecideUnitTurns();
        currentUnitTurn = -1;

        StartCoroutine(Intro());

        currentEnemySelected = 0;

        Random.InitState(System.DateTime.Now.Millisecond);
    }

    IEnumerator Intro()
    {
        onBattleStart.Invoke();

        yield return new WaitForSeconds(2.5f);

        for (int i = 0; i < players.Length; i++)
            players[i].ShowUI();

        NextTurn();
    }

    bool AllPlayersDead()
    {
        for(int i = 0; i < players.Length; i++)
        {
            if (!players[i].IsDead)
                return false; 
        }

        return true;
    }

    void NextTurn()
    {
        if (enemies.Count == 0)
        {
            PlayerVictory();
            return;
        }
        else if (AllPlayersDead())
        {
            PlayerLose();
            return;
        }

        if (currentUnit is PlayerUnit)
            currentUnit.DeselectUnit();

        currentUnitTurn = (currentUnitTurn + 1) % unitTurns.Count;
        currentUnit = unitTurns[currentUnitTurn];

        if (currentUnit is PlayerUnit)
            StartPlayerTurn();
        else
            StartEnemyTurn();
    }

    void StartPlayerTurn()
    {
        if (currentUnit.IsDead)
        {
            NextTurn();
            return;
        } 

        canSelectEnemy = true;
        isPlayerTurn = true;

        onPlayerTurnStart.Invoke();

        currentUnit.SelectUnit();
        enemies[currentEnemySelected].SelectUnit();
    }

    void StartEnemyTurn()
    {
        isPlayerTurn = false;

        enemies[currentEnemySelected].DeselectUnit();

        onEnemyTurnStart.Invoke();

        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        yield return ((EnemyUnit)currentUnit).ChooseAttack(players);
        NextTurn();
    }

    public void PlayerAttack()
    {
        canSelectEnemy = false;
        StartCoroutine(PlayerAttackCoroutine());
    }
   
    IEnumerator PlayerAttackCoroutine()
    {
        onPlayerAction.Invoke();

        yield return currentUnit.Attack(enemies[currentEnemySelected]);

        NextTurn();
    }

    public void PlayerAbility()
    {
        canSelectEnemy = false;

        StartCoroutine(PlayerAbilityCoroutine());
    }

    IEnumerator PlayerAbilityCoroutine()
    {
        if (currentUnit.CanUseAbility())
        {
            sfxManager.PlayMenuSFX(0);
            onPlayerAction.Invoke();
            yield return currentUnit.Ability(enemies[currentEnemySelected]);
            NextTurn();
        }
        else
        {
            sfxManager.PlayMenuSFX(1);
            battleSystemUI.ShowDialogue("Not enough MP!");
            yield return new WaitForSeconds(2f);
            battleSystemUI.ShowDialogue("");
        }
    }

    public void PlayerUseItem(Item item, PlayerUnit targetUnit)
    {
        canSelectEnemy = false;

        StartCoroutine(PlayerUseItemCoroutine(item, targetUnit));
    }

    IEnumerator PlayerUseItemCoroutine(Item item, PlayerUnit target)
    {
        onPlayerAction.Invoke();

        yield return currentUnit.UseItem(item, target);

        NextTurn();
    }

    public List<PlayerUnit> EligiblePlayersForItem(Item item)
    {
        List<PlayerUnit> eligiblePlayers = new List<PlayerUnit>();

        for (int i = 0; i < players.Length; i++)
        {
            if(players[i].IsEligibleForItem(item))
                eligiblePlayers.Add(players[i]);
        }

        return eligiblePlayers;
    }

    void DecideUnitTurns()
    {
        unitTurns.AddRange(players);
        unitTurns.AddRange(enemies);
        // Orders in ascending order
        unitTurns.Sort((x, y) => x.Luck.CompareTo(y.Luck));
        // Flip to get highest luck first
        unitTurns.Reverse();
    }

    void SelectEnemyToAttack()
    {
        if (!canSelectEnemy)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            enemies[currentEnemySelected].DeselectUnit();
            if (currentEnemySelected == 0)
                currentEnemySelected = enemies.Count - 1;
            else
                currentEnemySelected--;
            enemies[currentEnemySelected].SelectUnit();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            enemies[currentEnemySelected].DeselectUnit();
            currentEnemySelected = (currentEnemySelected + 1) % enemies.Count;
            enemies[currentEnemySelected].SelectUnit();
        }
    }

    public void RemoveDefeatedEnemy(Unit defeatedEnemy)
    {
        int enemyUnitIndex = unitTurns.FindIndex(x => x == defeatedEnemy);

        unitTurns.Remove(defeatedEnemy);
        enemies.Remove((EnemyUnit)defeatedEnemy);

        if (enemyUnitIndex < currentUnitTurn)
            currentUnitTurn = (currentUnitTurn - 1) % unitTurns.Count;

        currentEnemySelected = 0;
    }

    public Ability PlayerAbilities()
    {
        return currentUnit.Abilities;
    }

    public string PlayerAttackName()
    {
        return ((PlayerUnit)currentUnit).AttackName;
    }

    void PlayerVictory()
    {
        battleOver = true;

        battleSystemUI.ShowDialogue("Victory!");

        if(currentUnit is PlayerUnit)
            currentUnit.DeselectUnit();

        for (int i = 0; i < players.Length; i++)
            players[i].Victory();

        onBattleEnd.Invoke();
    }

    void PlayerLose()
    {
        battleOver = true;

        battleSystemUI.ShowDialogue("The party has been wiped out!");

        if (currentUnit is PlayerUnit)
            currentUnit.DeselectUnit();

        onBattleEnd.Invoke();
    }

    public void AddUltimatePoints(int points)
    {
        battleUltimate.AddUltimatePoints(points);
    }

    public void ActivateUltimate()
    {
        StartCoroutine(UltimateCoroutine());
    }

    IEnumerator UltimateCoroutine()
    {
        if (battleUltimate.CanUltimate)
        {
            sfxManager.PlayMenuSFX(0);
            onPlayerAction.Invoke();
            battleUltimate.ClearPoints();
            canSelectEnemy = false;
            currentUnit.DeselectUnit();
            int totalUltimateDamage = 0;

            List<PlayerUnit> alivePlayers = players.Where(x => !x.IsDead).ToList();

            for (int i = 0; i < alivePlayers.Count; i++)
            {
                totalUltimateDamage += alivePlayers[i].UltimateDamage();
                StartCoroutine(alivePlayers[i].UltimateCast());
            }

            yield return new WaitForSeconds(2f);

            yield return alivePlayers[0].UltimateAttack(enemies[currentEnemySelected], totalUltimateDamage);

            for (int i = 0; i < alivePlayers.Count; i++)
                alivePlayers[i].UltimateEnd();

            NextTurn();
        }
        else
        {
            sfxManager.PlayMenuSFX(1);
            battleSystemUI.ShowDialogue("Not enough ultimate points!");
            yield return new WaitForSeconds(2f);
            battleSystemUI.ShowDialogue("");
        }
    }

    private void Update()
    {
        SelectEnemyToAttack();

        if (battleOver && Input.GetKeyDown(KeyCode.Return))
            SceneManager.LoadScene(0);
    }
}
