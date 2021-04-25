using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TileScript : MonoBehaviour
{
    //public float tileID;
    
    public int[] tileID;
    public Node node;
    public Text fCostField, hCostField, gCostField;
    public Image panel;

    // void Awake()
    // { 
    //     //tileID = (100*transform.position.x) + transform.position.y + (0.01f*transform.position.z);
    // }

    void Start()
    {
        if(GameManager.instance.debugMode)
            panel.gameObject.transform.parent.gameObject.SetActive(true);
        else
            panel.gameObject.transform.parent.gameObject.SetActive(false);

        tileID = new int[2];
        tileID[0] = Mathf.RoundToInt(transform.position.x);
        tileID[1] = Mathf.RoundToInt(transform.position.z);
        float yPos = transform.position.y;
       //GameManager.instance.currentSceneTiles.Add(tileID, transform);
        GameManager.instance.currentSceneTiles[GameManager.instance.TileListIndexConversion(tileID[0]), GameManager.instance.TileListIndexConversion(tileID[1])] = transform;
        
        node = new Node(tileID[0], yPos, tileID[1], true);
        GameManager.instance.currentSceneNodes[GameManager.instance.TileListIndexConversion(tileID[0]), GameManager.instance.TileListIndexConversion(tileID[1])] = node;

        
    }

    void Update()
    {
        if(GameManager.instance.debugMode)
            panel.gameObject.transform.parent.gameObject.SetActive(true);
        else
            panel.gameObject.transform.parent.gameObject.SetActive(false);
        
        if(GameManager.instance.debugMode)
        {
            fCostField.text = node.fCost().ToString();
            gCostField.text = node.gCost.ToString();
            hCostField.text = node.hCost.ToString();

            if(node.closed || !node.walkable)
            {
                panel.color = Color.red;
            }

            if(node.open)
            {
                panel.color = Color.green;
            }
        }
    }
}
