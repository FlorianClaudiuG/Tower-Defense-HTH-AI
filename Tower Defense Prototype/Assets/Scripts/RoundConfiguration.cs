using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundConfiguration
{
    //Unit Configuration
    /// <summary>
    /// Integer array containing unit counts. Index 0 is the base unit count, index 1 is the flying unit count.
    /// </summary>
    public int[] UnitConfiguration;
    //Tower Configuration
    /// <summary>
    /// Integer array, each index corresponds to a tower node (0-10 index for 1-11 nodes). The integer value 
    /// represents tower type: 0 for no tower, 1 for base, 2 for AA.
    /// </summary>
    public int[] TowerConfiguration;

    public RoundConfiguration()
    {
        UnitConfiguration = new int[GameManager.Instance.unitPrefabs.Count]; // 0-2
        TowerConfiguration = new int[GameManager.Instance.towerNodes.Count / 2]; //0-10

        //Initialize to 0.
        for(int i = 0; i < UnitConfiguration.Length; i++)
        {
            UnitConfiguration[i] = 0;
        }
        for(int i = 0; i < TowerConfiguration.Length; i++)
        {
            TowerConfiguration[i] = 0;
        }
    }
}
