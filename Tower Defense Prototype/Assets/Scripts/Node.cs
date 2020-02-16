using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Tower node script
public class Node : MonoBehaviour
{
    [SerializeField]
    private Team team;
    private bool occupied = false;

    public Team Team { get { return team; } }

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer sr = (SpriteRenderer)GetComponentInChildren(typeof(SpriteRenderer));
        
        switch(team)
        {
            case Team.Red:
                sr.color = Color.red;
                break;
            case Team.Blue:
                sr.color = Color.blue;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseOver()
    {
        if(GameManager.Instance.actionPlayer.PlayerType == PlayerType.Human)
        {
            if (GameManager.Instance.buildPhase)
            {
                if (!occupied)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameManager.Instance.SpawnTower(team, gameObject.transform.position, GameManager.Instance.towerPrefabs[0], true);
                        occupied = true;
                    }
                    if (Input.GetMouseButtonDown(1))
                    {
                        GameManager.Instance.SpawnTower(team, gameObject.transform.position, GameManager.Instance.towerPrefabs[1], true);
                        occupied = true;
                    }
                    if (Input.GetMouseButtonDown(2))
                    {
                        GameManager.Instance.SpawnTower(team, gameObject.transform.position, GameManager.Instance.towerPrefabs[2], true);
                        occupied = true;
                    }
                }
            }
        }
    }
}
