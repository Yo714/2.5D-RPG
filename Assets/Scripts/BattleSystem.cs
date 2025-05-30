using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TMPro;
using UnityEditor.PackageManager.Requests;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private enum BattleState { Start, Selection, Battle, Won, Lost, Run }

    [Header("Battle State")]
    [SerializeField] private BattleState state;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battlers")]
    [SerializeField] private List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> playerBattlers = new List<BattleEntities>();

    [Header("UI")]
    [SerializeField] private GameObject[] enemySelectionButtons;
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject enemySelectionMenu;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject bottomTextPopUp;
    [SerializeField] private TextMeshProUGUI bottomText;

    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private int currentPlayer;

    private const string ACTION_MESSAGE = "'s Action:";
    private const string WIN_MESSAGE = "Your party won the battle";
    private const string LOST_MESSAGE = "Your party has been defeated";
    private const string SUCCESFULLY_RAN_MESSAGE = "Your party ran away";
    private const string UNSUCCESFULLY_RAN_MESSAGE = "Your party failed to run";
    private const int TURN_DURATION = 1;
    private const int RUN_CHANCE = 50;
    private const string OVERWORLD_SCENE = "OverworldScene";

    void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();
        ShowBattleMenu();
        DetermineBattleOrder();
    }

    private IEnumerator BattleRoutine()
    {
        enemySelectionMenu.SetActive(false);
        state = BattleState.Battle;
        bottomTextPopUp.SetActive(true);

        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (state == BattleState.Battle && allBattlers[i].CurrHealth > 0)
            {
                switch (allBattlers[i].BattleAction)
                {
                    case BattleEntities.Action.Attack:
                        yield return StartCoroutine(AttackRoutine(i));
                        break;
                    case BattleEntities.Action.Run:
                        yield return StartCoroutine(RunRoutine());
                        break;
                    default:
                        Debug.Log("Error - incorrect battle action");
                        break;
                }
            }
        }

        RemoveDeadBattlers();

        if (state == BattleState.Battle)
        {
            bottomTextPopUp.SetActive(false);
            currentPlayer = 0;
            ShowBattleMenu();
        }

        yield return null;
    }

    private IEnumerator AttackRoutine(int i)
    {
        if (allBattlers[i].IsPlayer == true)
        {
            BattleEntities currAttacker = allBattlers[i];
            if (allBattlers[currAttacker.Target].CurrHealth <= 0)
            {
                currAttacker.SetTartget(GetRanomEnemy());
            }
            BattleEntities currTarget = allBattlers[currAttacker.Target];
            AttackAction(currAttacker, currTarget);
            yield return new WaitForSeconds(TURN_DURATION);

            if (currTarget.CurrHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION);
                enemyBattlers.Remove(currTarget);

                if (enemyBattlers.Count <= 0)
                {
                    state = BattleState.Won;
                    bottomText.text = WIN_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION);
                    SceneManager.LoadScene(OVERWORLD_SCENE);
                }
            }
        }

        if (i < allBattlers.Count && allBattlers[i].IsPlayer == false)
        {
            BattleEntities currAttacker = allBattlers[i];
            currAttacker.SetTartget(GetRandomPartyMember());
            BattleEntities currTarget = allBattlers[currAttacker.Target];

            AttackAction(currAttacker, currTarget);
            yield return new WaitForSeconds(TURN_DURATION);

            if (currTarget.CurrHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION);
                playerBattlers.Remove(currTarget);

                if (playerBattlers.Count <= 0)
                {
                    state = BattleState.Lost;
                    bottomText.text = LOST_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION);
                    Debug.Log("Game Over");
                }
            }
        }
    }

    private void RemoveDeadBattlers()
    {
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if(allBattlers[i].CurrHealth <= 0)
            {
                allBattlers.RemoveAt(i);
            }
        }
    }

    private void CreatePartyEntities()
    {
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetAliveParty();

        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentParty[i].MemberName, currentParty[i].CurrHealth, currentParty[i].MaxHealth,
            currentParty[i].Initiative, currentParty[i].Strength, currentParty[i].Level, true);

            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].MemberBattleVisualPrefab,
            partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentParty[i].CurrHealth, currentParty[i].MaxHealth, currentParty[i].Level);
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);
        }
    }

    private IEnumerator RunRoutine()
    {
        if(state == BattleState.Battle)
        {
            if(Random.Range(1,101) >= RUN_CHANCE)
            {
                bottomText.text = SUCCESFULLY_RAN_MESSAGE;
                state = BattleState.Run;
                allBattlers.Clear();
                yield return new WaitForSeconds(TURN_DURATION);
                SceneManager.LoadScene(OVERWORLD_SCENE);
                yield break;
            }
            else
            {
                bottomText.text = UNSUCCESFULLY_RAN_MESSAGE;
                yield return new WaitForSeconds(TURN_DURATION);
            }
        }
    }

    private void CreateEnemyEntities()
    {
        List<Enemy> currentEnemies = new List<Enemy>();
        currentEnemies = enemyManager.GetCurrentEnemies();

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentEnemies[i].EnemyName, currentEnemies[i].CurrHealth, currentEnemies[i].MaxHealth,
            currentEnemies[i].Initiative, currentEnemies[i].Strength, currentEnemies[i].Level, false);

            BattleVisuals tempBattleVisuals = Instantiate(currentEnemies[i].EnemyVisualPrefab,
            enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentEnemies[i].MaxHealth, currentEnemies[i].MaxHealth, currentEnemies[i].Level);
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);
        }
    }

    public void ShowBattleMenu()
    {
        actionText.text = playerBattlers[currentPlayer].Name + ACTION_MESSAGE;
        battleMenu.SetActive(true);
    }

    public void ShowEnemySelectionMenu()
    {
        battleMenu.SetActive(false);
        SetEnemySelectionButtons();
        enemySelectionMenu.SetActive(true);
    }

    public void SetEnemySelectionButtons()
    {
        for (int i = 0; i < enemySelectionButtons.Length; i++)
        {
            enemySelectionButtons[i].SetActive(false);
        }

        for (int i = 0; i < enemyBattlers.Count; i++)
        {
            enemySelectionButtons[i].SetActive(true);
            enemySelectionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = enemyBattlers[i].Name;
        }
    }

    public void SelectEnemy(int currentEnemy)
    {
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];
        currentPlayerEntity.SetTartget(allBattlers.IndexOf(enemyBattlers[currentEnemy]));

        currentPlayerEntity.BattleAction = BattleEntities.Action.Attack;
        currentPlayer++;

        if (currentPlayer >= playerBattlers.Count)
        {
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();
        }
    }

    private void AttackAction(BattleEntities currAttacker, BattleEntities currTarget)
    {
        int damage = currAttacker.Strength;
        currAttacker.BattleVisuals.PlayAttackAnimation();
        currTarget.CurrHealth -= damage;
        currTarget.BattleVisuals.PlayHitAnimation();
        currTarget.UpdateUI();
        bottomText.text = string.Format("{0} attacks {1} for {2} damage", currAttacker.Name, currTarget.Name, damage);
        SaveHealth();
    }

    private int GetRandomPartyMember()
    {
        List<int> partyMembers = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == true && allBattlers[i].CurrHealth > 0)
                partyMembers.Add(i);
        }

        return partyMembers[Random.Range(0, partyMembers.Count)];
    }

    private int GetRanomEnemy()
    {
        List<int> enemies = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == false && allBattlers[i].CurrHealth > 0)
                enemies.Add(i);
        }

        return enemies[Random.Range(0, enemies.Count)];
    }

    private void SaveHealth()
    {
        for (int i = 0; i < playerBattlers.Count; i++)
        {
            partyManager.SaveHealth(i, playerBattlers[i].CurrHealth);
        }
    }

    private void DetermineBattleOrder()
    {
        allBattlers.Sort((bi1, bi2) => -bi1.Initiative.CompareTo(bi2.Initiative));
    }

    public void SelectRunAction()
    {
        state = BattleState.Selection;
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];

        currentPlayerEntity.BattleAction = BattleEntities.Action.Run;

        battleMenu.SetActive(false);
        currentPlayer++;

        if (currentPlayer >= playerBattlers.Count)
        {
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();
        }
    }
}

[System.Serializable]
public class BattleEntities
{
    public enum Action { Attack, Run }
    public Action BattleAction;

    public string Name;
    public int CurrHealth;
    public int MaxHealth;
    public int Initiative;
    public int Strength;
    public int Level;
    public bool IsPlayer;
    public BattleVisuals BattleVisuals;
    public int Target;

    public void SetEntityValues(string name, int currHealth, int maxHealth, int initiative, int strength, int level, bool isPlayer)
    {
        Name = name;
        CurrHealth = currHealth;
        MaxHealth = maxHealth;
        Initiative = initiative;
        Strength = strength;
        Level = level;
        IsPlayer = isPlayer;
    }

    public void SetTartget(int target)
    {
        Target = target;
    }

    public void UpdateUI()
    {
        BattleVisuals.ChangeHealth(CurrHealth);
    }
}

