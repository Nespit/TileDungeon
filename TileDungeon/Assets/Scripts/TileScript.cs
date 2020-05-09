using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TileScript : MonoBehaviour
{
    public float tileID;

    void Awake()
    {
        tileID = (1000*transform.position.x) + transform.position.y + (0.001f*transform.position.z);
    }

    void Start()
    {
        GameManager.instance.currentSceneTiles.Add(tileID, transform);
    }
}
