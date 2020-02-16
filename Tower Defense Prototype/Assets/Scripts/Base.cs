using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    public float health = 100f;
    public Team team;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.blueHealthText.text = health.ToString();
        GameManager.Instance.redHealthText.text = health.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.gameObject);
        try
        {
            Unit enemyUnit = (Unit)other.gameObject.GetComponentInChildren(typeof(Unit));
            if(enemyUnit.Team != team)
            {
                TakeDamage(enemyUnit.Damage);
                enemyUnit.Die(false);
            }
        }
        catch (UnityException e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void TakeDamage(float amount)
    {
        if (health <= amount)
        {
            health = 0;
            UpdateHealthText();
            Die();
        }
        else
        {
            health -= amount;
            UpdateHealthText();
        }
    }

    public void Die()
    {
        //this should end the game;
        Debug.Log("Base " + team.ToString() + " is dead!");
        GameManager.Instance.EndGame(team);
        Destroy(gameObject);
    }

    public void UpdateHealthText()
    {
        switch(team)
        {
            case Team.Blue:
                GameManager.Instance.blueHealthText.text = health.ToString();
                break;
            case Team.Red:
                GameManager.Instance.redHealthText.text = health.ToString();
                break;
            default:
                break;
        }
    }
}
