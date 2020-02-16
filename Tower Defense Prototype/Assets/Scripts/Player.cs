using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Team team;
    [SerializeField]
    private float resources;
    [SerializeField]
    private PlayerType pType;

    //Between 0 and 1: Controls how the AI player will split its resources: high = more attack, low = more defense, 0.5 = split down the middle. 
    [SerializeField]
    private float aggresivityRating = 0.5f;

    [SerializeField]
    public int lastOccupied = 0;

    #region Properties
    public Team Team { get { return team; } set { team = value; } }
    public float Resources { get { return resources; } set { resources = value; } }
    public PlayerType PlayerType { get { return pType; } set { pType = value; } }
    #endregion

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GiveResources(int amount)
    {
        resources += amount;
        if(pType == PlayerType.Human)
        {
            GameManager.Instance.resourceText.text = resources.ToString();
        }
    }

    public RoundConfiguration Decide()
    {
        RoundConfiguration output;
        switch (pType)
        {
            case PlayerType.Human:
                output = null;
                break;
            case PlayerType.BasicAI:
                output = BasicAIDecide(); //ARG = 0.5
                break;
            case PlayerType.ImprovedAI: //ARG 0.1 -> 0.9
                output = ImprovedAIDecide();
                break;
            case PlayerType.OffensiveAI:
                aggresivityRating = 0.7f;
                output = BasicAIDecide();
                break;
            case PlayerType.DefensiveAI:
                aggresivityRating = 0.3f;
                output = BasicAIDecide();
                break;
            case PlayerType.NN:
                output = NNDecide();    //Not yet implemented
                break;
            default:
                output = null;
                break;
        }
        return output;
    }

    private RoundConfiguration BasicAIDecide()
    {
        float offenseBudget = resources * aggresivityRating;
        float defenseBudget = resources - offenseBudget;

        RoundConfiguration rConfig = new RoundConfiguration();

        float unitPrefabCount = GameManager.Instance.unitPrefabs.Count;
        int maxUnitPrice = MaxUnitPrice();

        while(offenseBudget >= maxUnitPrice)
        {
            Unit randomUnit = GameManager.Instance.unitPrefabs[(int)Random.Range(0, unitPrefabCount)];
            offenseBudget -= randomUnit.Price;
            //Debug.Log(team + " " + randomUnit.name + " Resources left: " + offenseBudget);
            rConfig.UnitConfiguration[(int)randomUnit.UnitType]++;
        }

        Unit minUnit = MinPricedUnit();
        int minUnitPrice = minUnit.Price;

        while (offenseBudget >= minUnitPrice)
        {
            offenseBudget -= minUnitPrice;
            //Debug.Log(team + " " + minUnit.name + " Resources left: " + offenseBudget);
            rConfig.UnitConfiguration[(int)minUnit.UnitType]++;
        }

        float towerPrefabCount = GameManager.Instance.towerPrefabs.Count;
        int maxTowerPrice = MaxTowerPrice();

        while(defenseBudget >= maxTowerPrice && lastOccupied < GameManager.Instance.redNodes.Count)
        {
            Tower randomTower = GameManager.Instance.towerPrefabs[(int)Random.Range(0, towerPrefabCount)];
            defenseBudget -= randomTower.Price;
            rConfig.TowerConfiguration[GameManager.Instance.redNodes.Count - lastOccupied] = (int)randomTower.TowerType + 1;
            //Debug.Log(team + " " + randomTower.name + " Resources left: " + defenseBudget);
            lastOccupied++;
        }

        Tower minTower = MinPricedTower();
        int minTowerPrice = minTower.Price;

        while (defenseBudget >= minTowerPrice && lastOccupied < GameManager.Instance.redNodes.Count)
        {
            defenseBudget -= minTowerPrice;
            rConfig.TowerConfiguration[GameManager.Instance.redNodes.Count - lastOccupied] = (int)minTower.TowerType + 1;
            //Debug.Log(team + " " + minTower.name + " Resources left: " + defenseBudget);
            lastOccupied++;
        }

        float remainingBudget = defenseBudget + offenseBudget;

        if (aggresivityRating >= 0.5 || lastOccupied >= GameManager.Instance.redNodes.Count)
        {
            while (remainingBudget >= minUnitPrice)
            {
                remainingBudget -= minUnitPrice;
                rConfig.UnitConfiguration[(int)minUnit.UnitType]++;
                //Debug.Log(team + " " + minUnit.name + " Resources left: " + remainingBudget);
            }
        }
        else
        {
            while(remainingBudget >= minTowerPrice && lastOccupied < GameManager.Instance.redNodes.Count)
            {
                remainingBudget -= minTowerPrice;
                rConfig.TowerConfiguration[GameManager.Instance.redNodes.Count - lastOccupied] = (int)minTower.TowerType + 1;
                //Debug.Log(team + " " + minTower.name + " Resources left: " + remainingBudget);
                lastOccupied++;
            }
        }

        resources = remainingBudget;

        //string debugUnits = "";
        //for(int i = 0; i < rConfig.UnitConfiguration.Length; i++)
        //{
        //    debugUnits += (i + ": " + rConfig.UnitConfiguration[i] + " @ ");
        //}

        //string debugTowers = "";
        //for(int i = 0; i < rConfig.TowerConfiguration.Length; i++)
        //{
        //    debugTowers += (i + ": " + rConfig.TowerConfiguration[i] + " @ ");
        //}

        //Debug.Log(debugUnits);
        //Debug.Log(debugTowers);

        return rConfig;
    }

    private RoundConfiguration ImprovedAIDecide()
    {
        aggresivityRating = 0.1f * GameManager.Instance.turnCount;
        if(aggresivityRating > 0.9f)
        {
            aggresivityRating = 0.9f;
        }
        return BasicAIDecide();
    }

    private RoundConfiguration NNDecide()
    {
        return new RoundConfiguration();
    }

    public bool TakeResources(int amount)
    {
        if(resources < amount)
        {
            return false;
        }
        else
        {
            resources -= amount;
            if(pType == PlayerType.Human)
            {
                GameManager.Instance.resourceText.text = resources.ToString();
            }
            return true;
        }
    }

    private int MaxUnitPrice()
    {
        int maxPrice = 0;

        List<Unit> units = GameManager.Instance.unitPrefabs;
        foreach(Unit unit in units)
        {
            if(unit.Price > maxPrice)
            {
                maxPrice = unit.Price;
            }
        }

        return maxPrice;
    }

    private Unit MinPricedUnit()
    {
        List<Unit> units = GameManager.Instance.unitPrefabs;

        int minPrice = units[0].Price;
        Unit minUnit = units[0];

        foreach (Unit unit in units)
        {
            if (unit.Price < minPrice)
            {
                minPrice = unit.Price;
                minUnit = unit;
            }
        }

        return minUnit;
    }

    private int MaxTowerPrice()
    {
        int maxPrice = 0;

        List<Tower> towers = GameManager.Instance.towerPrefabs;
        foreach(Tower tower in towers)
        {
            if(tower.Price > maxPrice)
            {
                maxPrice = tower.Price;
            }
        }

        return maxPrice;
    }

    private Tower MinPricedTower()
    {
        List<Tower> towers = GameManager.Instance.towerPrefabs;

        int minPrice = towers[0].Price;
        Tower minTower = towers[0];

        foreach (Tower tower in towers)
        {
            if (tower.Price < minPrice)
            {
                minPrice = tower.Price;
                minTower = tower;
            }
        }

        return minTower;
    }
}
