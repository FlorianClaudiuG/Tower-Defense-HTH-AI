using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Dictionary<UnitType, int> redConfiguration;
    public Dictionary<UnitType, int> blueConfiguration;
    public List<int> redConfigurationCounts;
    public List<int> blueConfigurationCounts;

    public PlayerType bluePlayerType;
    public PlayerType redPlayerType;

    public List<Unit> unitPrefabs;
    public List<Tower> towerPrefabs;

    public GameObject nodeCollection;
    public List<Node> towerNodes;
    public List<Node> redNodes;
    public List<Node> blueNodes;

    public List<Player> allPlayers;
    public List<Player> playerPrefabs;
    public List<GameObject> redTargets;
    public List<GameObject> blueTargets;

    public UnitButton gruntButton;
    public Text gruntText;
    public UnitButton birdButton;
    public Text birdText;
    public UnitButton tankButton;
    public Text tankText;

    public List<Unit> allUnits;

    public Text resourceText;
    public Text actionPlayerText;
    public Text timerText;
    public Text timerLabelText;
    public Text blueHealthText;
    public Text redHealthText;

    public int defaultResources = 0;

    public int roundBonus = 250;

    public Player actionPlayer;

    public List<Tower> allTowers;

    public int turnCount = 0;
    public bool buildPhase = true;
    private bool spawning = false;
    [SerializeField]
    private float defaultTimeLeft;
    private float timeLeft;

    private bool endGame = false;

    int mod = 0;
    int flyMod = 0;
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.Log("Destroyed a second GameManager instance attempt");
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        PopulateTowerNodes();
        InitializeDictionaries();
        InitializePlayers(bluePlayerType, redPlayerType);
        StartBuildPhase();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!endGame)
        {
            if (!buildPhase)
            {
                if (!spawning)
                {
                    SpawnUnits();
                    spawning = true;
                }
                if (allUnits.Count <= 0)
                {
                    StartBuildPhase();
                }
            }
            else
            {
                timeLeft -= Time.deltaTime;
                timerText.text = ((int)timeLeft).ToString();
                if (timeLeft <= 0)
                {
                    EndBuildPhase();
                }
            }
        }
        else
        {
            for(int i = 0; i < allUnits.Count; i++)
            {
                Destroy(allUnits[i]);
            }
        }

    }

    void SpawnUnits()
    {
        StartCoroutine(SpawnRedUnits());
        StartCoroutine(SpawnBlueUnits());
    }

    IEnumerator SpawnRedUnits()
    {
        foreach (UnitType unit in redConfiguration.Keys)
        {
            for (int i = 0; i < redConfiguration[unit]; i++)
            {
                SpawnUnit(Team.Red, GetUnitPrefabFromType(unit));
                yield return new WaitForSeconds(1.5f);
            }
        }
    }

    IEnumerator SpawnBlueUnits()
    {
        foreach (UnitType unit in blueConfiguration.Keys)
        {
            for (int i = 0; i < blueConfiguration[unit]; i++)
            {
                //Debug.Log(unit + " " + GetUnitPrefabFromType(unit));
                SpawnUnit(Team.Blue, GetUnitPrefabFromType(unit));              
                yield return new WaitForSeconds(1.5f);
            }
        }
    }

    private void SpawnUnit(Team team, Unit unit)
    {
        Transform where;
        switch(team)
        {
            case Team.Red:
                where = redTargets[0].transform;
                break;
            case Team.Blue:
                where = blueTargets[0].transform;
                break;
            default:
                where = blueTargets[1].transform;
                break;
        }
        Unit instance = Instantiate(unit, where.position, Quaternion.identity);
        instance.Initialize(team, turnCount);
        allUnits.Add(instance);
    }

    private Unit GetUnitPrefabFromType(UnitType type)
    {
        switch (type)
        {
            case UnitType.BaseUnit:
                return unitPrefabs[0];
            case UnitType.FlyingUnit:
                return unitPrefabs[1];
            case UnitType.TankUnit:
                return unitPrefabs[2];
            default:
                return unitPrefabs[0];
        }
    }

    private Tower GetUnitPrefabFromTower(TowerType type)
    {
        switch(type)
        {
            case TowerType.BaseTower:
                return towerPrefabs[0];
            case TowerType.AATower:
                return towerPrefabs[1];
            default:
                return towerPrefabs[0];
        }
    }

    private void InitializeDictionaries()
    {
        allUnits = new List<Unit>();
        redConfiguration = new Dictionary<UnitType, int>();
        blueConfiguration = new Dictionary<UnitType, int>();
        for (int i = 0; i < Enum.GetValues(typeof(UnitType)).Length; i++)
        {
            redConfiguration[(UnitType)i] = redConfigurationCounts[i];
            blueConfiguration[(UnitType)i] = blueConfigurationCounts[i];
        }
    }

    private void PopulateTowerNodes()
    {
        Component[] temp = nodeCollection.GetComponentsInChildren(typeof(Node));
        towerNodes = new List<Node>();
        redNodes = new List<Node>();
        blueNodes = new List<Node>();
        foreach (Component comp in temp)
        {
            towerNodes.Add((Node)comp);
            if(((Node)comp).Team == Team.Red)
            {
                redNodes.Add((Node)comp);
            }
            else
            {
                blueNodes.Add((Node)comp);
            }
        }
        /* Test node order
        foreach(Node node in redNodes)
        {
            Debug.Log(node.name);
        }
        foreach (Node node in blueNodes)
        {
            Debug.Log(node.name);
        }*/
    }

    public void SpawnTower(Team team, Vector2 location, Tower tower, bool human)
    {
        if(human)
        {
            if (actionPlayer.TakeResources(tower.Price))
            {
                Tower instance = Instantiate(towerPrefabs[(int)tower.TowerType], location, Quaternion.identity);
                instance.Initialize(team);
                allTowers.Add(instance);
            }
        }
        else
        {
            SpriteRenderer sr = towerPrefabs[(int)tower.TowerType].GetComponentInChildren<SpriteRenderer>();
            sr.enabled = false;
            Tower instance = Instantiate(towerPrefabs[(int)tower.TowerType], location, Quaternion.identity);
            sr.enabled = true;
            instance.Initialize(team);
            allTowers.Add(instance);
        }
    }

    private void EndBuildPhase()
    {
        buildPhase = false;
        ToggleTowerColliders();
        timerText.text = ((int)defaultTimeLeft).ToString();
        ToggleTimer();

        //Add turncount modifier + default/free units

        if(turnCount % 2 == 0)
        {
            mod++;
        }
        if(turnCount % 3 == 0)
        {
            flyMod++;
        }
        redConfiguration[UnitType.BaseUnit] += (6 + mod);
        redConfiguration[UnitType.FlyingUnit] += flyMod;
        blueConfiguration[UnitType.BaseUnit] += (6 + mod);
        blueConfiguration[UnitType.FlyingUnit] += flyMod;
        MakeTowersVisible();
    }

    private void StartBuildPhase()
    {
        ToggleTowerColliders();
        spawning = false;
        buildPhase = true;
        turnCount++;
        timeLeft = defaultTimeLeft;
        AwardPlayers(roundBonus);
        ToggleTimer();
        GetDecisions();
    }

    private void AwardPlayers(int amount)
    {
        foreach(Player player in allPlayers)
        {
            player.GiveResources(amount);
        }
    }

    private void ToggleTowerColliders() 
    {
        foreach(Tower tower in allTowers)
        {
            Collider2D collider = tower.GetComponent<Collider2D>();
            collider.enabled = !collider.enabled;
        }
    }

    private void MakeTowersVisible()
    {
        foreach(Tower tower in allTowers)
        {
            SpriteRenderer sr = tower.GetComponentInChildren<SpriteRenderer>();
            sr.enabled = true;
        }
    }

    private void InitializePlayers(PlayerType blueType, PlayerType redType)
    {

        foreach(Player player in playerPrefabs)
        {
            Player instance = Instantiate(player);
            instance.Resources = defaultResources;
            instance.PlayerType = redType;
            instance.lastOccupied = 1;
            allPlayers.Add(instance);
        }
        actionPlayer = allPlayers[0];
        actionPlayer.PlayerType = blueType;
        
        //Debug.Log(actionPlayer.Team);
        resourceText.text = actionPlayer.Resources.ToString();
       // actionPlayer.PlayerType = PlayerType.Human; //change here for NN
    }

    public void RewardPlayerForKill(Unit killedUnit)
    {
        Player playerToReward = null;
        switch (killedUnit.Team)
        {
            case Team.Blue:
                playerToReward = GetPlayerByTeam(Team.Red);
                break;
            case Team.Red:
                playerToReward = GetPlayerByTeam(Team.Blue);
                break;
            default:
                break;
        }
        playerToReward.GiveResources(killedUnit.Bounty);
    }

    public void RewardPlayerForDamage(Unit killedUnit)
    {
        Player playerToReward = null;
        switch (killedUnit.Team)
        {
            case Team.Blue:
                playerToReward = GetPlayerByTeam(Team.Blue);
                break;
            case Team.Red:
                playerToReward = GetPlayerByTeam(Team.Red);
                break;
            default:
                break;
        }
        playerToReward.GiveResources(killedUnit.Bounty);
    }

    private Player GetPlayerByTeam(Team team)
    {
        foreach (Player player in allPlayers)
        {
            if (player.Team == team)
            {
                return player;
            }
        }
        Debug.Log("Failed to find player in team " + team + "!");
        return null;
    }

    private void ToggleTimer()
    {
        timerText.enabled = !timerText.enabled;
        timerLabelText.enabled = !timerLabelText.enabled;
        gruntText.enabled = !gruntText.enabled;
        birdText.enabled = !birdText.enabled;
        tankText.enabled = !tankText.enabled;

        foreach(SpriteRenderer sr in gruntButton.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = !sr.enabled;
        }
        foreach (SpriteRenderer sr in birdButton.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = !sr.enabled;
        }
        foreach (SpriteRenderer sr in tankButton.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = !sr.enabled;
        }
    }

    public void EndGame(Team loser)
    {
        endGame = true;
        Debug.Log(loser.ToString() + " has lost the game!");
    }

    public void Cleanup()
    {
        
    }

    public void AddUnitToConfiguration(Unit prefab)
    {
        //add to team blue.
        if(GetPlayerByTeam(Team.Blue).TakeResources(prefab.Price))
        {
            blueConfiguration[prefab.UnitType]++;
        }
        
    }

    private void GetDecisions()
    {
        foreach(Player player in allPlayers)
        {
            RoundConfiguration rConfig;
            if(player.PlayerType != PlayerType.Human)
            {
                rConfig = player.Decide();
                switch (player.Team)
                {
                    case Team.Red:
                        for (int i = 0; i < Enum.GetValues(typeof(UnitType)).Length; i++)
                        {
                            redConfigurationCounts[i] = rConfig.UnitConfiguration[i];
                            redConfiguration[(UnitType)i] = rConfig.UnitConfiguration[i];
                           // Debug.Log(i + " is UnitType " + (UnitType)i);
                        }
                        for (int i = 0; i < redNodes.Count; i++)
                        {
                            if(rConfig.TowerConfiguration[i] != 0)
                            {
                                SpawnTower(Team.Red, redNodes[i].transform.position, towerPrefabs[rConfig.TowerConfiguration[i] - 1], false);
                            }
                        }
                    break;
                    case Team.Blue:
                        for (int i = 0; i < Enum.GetValues(typeof(UnitType)).Length; i++)
                        {
                            blueConfigurationCounts[i] = rConfig.UnitConfiguration[i];
                            blueConfiguration[(UnitType)i] = rConfig.UnitConfiguration[i];
                        }
                        for (int i = 0; i < blueNodes.Count; i++)
                        {
                            if (rConfig.TowerConfiguration[i] != 0)
                            {
                                SpawnTower(Team.Blue, blueNodes[i].transform.position, towerPrefabs[rConfig.TowerConfiguration[i] - 1], false);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
