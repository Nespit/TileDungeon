using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TileScript : MonoBehaviour
{
    //public float tileID;
    public int[] tileID;
    // void Awake()
    // { 
    //     //tileID = (100*transform.position.x) + transform.position.y + (0.01f*transform.position.z);
    // }

    void Start()
    {
        tileID = new int[2];
        tileID[0] = Mathf.RoundToInt(transform.position.x);
        tileID[1] = Mathf.RoundToInt(transform.position.z);
       //GameManager.instance.currentSceneTiles.Add(tileID, transform);
        GameManager.instance.currentSceneTiles[GameManager.instance.TileID(tileID[0]), GameManager.instance.TileID(tileID[1])] = transform;
    }
}
