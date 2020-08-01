using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TileScript : MonoBehaviour
{
    public float tileID;

    void Awake()
    {
        tileID = (100*transform.position.x) + transform.position.y + (0.01f*transform.position.z);
    }

    void Start()
    {
        GameManager.instance.currentSceneTiles.Add(tileID, transform);
    }
}
