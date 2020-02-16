using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitButton : MonoBehaviour
{
    [SerializeField]
    private Unit prefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if(GameManager.Instance.actionPlayer.PlayerType == PlayerType.Human)
        {
            if (GameManager.Instance.buildPhase)
            {
                GameManager.Instance.AddUnitToConfiguration(prefab);
            }
        }
        
    }
}
