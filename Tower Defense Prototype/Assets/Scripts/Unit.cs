using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField]
    protected float health;
    [SerializeField]
    protected float armor = 1f;
    [SerializeField]
    protected List<GameObject> targets;
    [SerializeField]
    protected int currentTargetIndex = 0;
    [SerializeField]
    protected Team team;

    [SerializeField]
    private UnitType unitType;

    [SerializeField]
    protected int price;

    [SerializeField]
    protected int bounty;

    [SerializeField]
    protected float damage;

    private float lerp = 0f;
    [SerializeField]
    private float speed = 0.01f;
    [SerializeField]
    private GameObject aimTarget;

    [SerializeField]
    private int additiveHits = 0;

    #region Properties
    public Team Team { get { return team; } }
    public UnitType UnitType { get { return unitType; } }
    public float Health { get { return health; } set { health = value; } }
    public int Price { get { return price; } }
    public int Bounty { get { return bounty; } }
    public float Damage { get { return damage; } }
    public Vector2 AimTarget { get { return aimTarget.transform.position; } }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if(targets.Count > 0)
        {
            transform.position = targets[0].transform.position;
            currentTargetIndex = 1;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lerp = lerp + speed;
        if (lerp <= 1)
        {
            transform.position = Vector2.Lerp(targets[currentTargetIndex - 1].transform.position, targets[currentTargetIndex].transform.position, lerp);
        }
        else
        {
            if(currentTargetIndex + 1 < targets.Count)
            {
                currentTargetIndex++;
                lerp = 0f;
            }
        }
    }

    public void TakeDamage(DamageType damageType, float amount)
    {
        float damage;
        float armorReduction = 1 - (armor * 0.1f);

        switch (damageType)
        {
            case DamageType.BaseDamage:
                damage = amount * armorReduction;
                break;
            case DamageType.AADamage:
                if(unitType == UnitType.FlyingUnit)
                {
                    damage = 2 * amount * armorReduction;
                }
                else
                {
                    damage = amount * armorReduction;
                }
                break;
            case DamageType.AdditiveDamage:
                
                if(unitType == UnitType.TankUnit)
                {
                    damage = (amount + additiveHits) * armorReduction;
                    additiveHits += 2;
                }
                else
                {
                    damage = (amount + additiveHits++) * armorReduction;
                }
                break;
            default:
                damage = amount * armorReduction;
                break;
        }

        if (health > damage)
        {
            health -= damage;
        }
        else
        {
            Die(true);
        }
    }

    public void Die(bool giveRewards)
    {
        try
        {
            GameManager.Instance.allUnits.Remove(this);
        }
        catch(UnityException e)
        {

        }
        finally
        {
            if (giveRewards)
            {
                GameManager.Instance.RewardPlayerForKill(this);
            }
            else
            {
                GameManager.Instance.RewardPlayerForDamage(this);
            }
            Destroy(gameObject);
        }
    }

    public void Initialize(Team team, int turnCount)
    {
        this.team = team;
        switch(team)
        {
            case Team.Red:
                targets = GameManager.Instance.redTargets;
                break;
            case Team.Blue:
                targets = GameManager.Instance.blueTargets;
                SpriteRenderer sr = (SpriteRenderer)gameObject.GetComponentInChildren(typeof(SpriteRenderer));
                sr.flipX = !sr.flipX;
                break;
            default:
                break;
        }
        //Scale
        health = health + (turnCount * 10) + turnCount * turnCount;
        if(turnCount % 2 == 0)
        {
            //cap
            armor+=(turnCount/2);
            if (armor > 7)
            {
                armor = 7;
            }
        }
    }
}
