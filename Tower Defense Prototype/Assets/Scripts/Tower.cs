using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Tower : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Projectile projectile;
    [SerializeField]
    private TowerType type;
    private Queue<Unit> inRange;
    private Unit target;
    [SerializeField]
    private Team team;
    [SerializeField]
    private Transform firingLocation;
    [SerializeField]
    private float fireRate = 1f;
    [SerializeField]
    private int price = 100;
    private bool firing = false;

    #region Properties
    public int Price { get { return price; } }
    public TowerType TowerType { get { return type; } }
    #endregion

    void Awake()
    {
        inRange = new Queue<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (!firing)
            {
                StartCoroutine("FireWithDelay");
            }
        }
        else if (inRange.Count != 0)
        {
            target = inRange.Dequeue();
        }
        else
        {
            target = null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.gameObject);
        try
        {
            Unit enemyUnit = (Unit)other.gameObject.GetComponentInChildren(typeof(Unit));
            //Debug.Log(enemyUnit);
            if (enemyUnit.Team != team)
            {
                if (type == TowerType.HighDamageTower && enemyUnit.UnitType == UnitType.TankUnit)
                {
                    target = enemyUnit;
                }
                else
                {
                    if (target == null)
                    {
                        target = enemyUnit;
                    }
                    else
                    {
                        inRange.Enqueue(enemyUnit);
                    }
                }
            }
        }
        catch (UnityException e)
        {
            Debug.LogError(e.Message);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Unit enemyUnit = (Unit)other.gameObject.GetComponentInChildren(typeof(Unit));
        
        if (target != null)
        {
            //if target is unit that is leaving collider area
            if (target == enemyUnit)
            {
                if (inRange.Count != 0)
                {
                    target = inRange.Dequeue();
                }
                else
                {
                    target = null;
                }
            }
            else
            {
                _ = inRange.Dequeue();
            }
        }
    }

    public void Fire()
    {
        Projectile instance = Instantiate(projectile, firingLocation.position, Quaternion.identity);
        instance.Initialize(firingLocation.position, target);
        firing = true;
    }

    private IEnumerator FireWithDelay()
    {
        if (!firing)
        {
            Fire();
        }
        yield return new WaitForSeconds(fireRate);
        firing = false;
    }

    public void Initialize(Team team)
    {
        this.team = team;
    }
}


