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
    public Image debugPanel, movementIndicatorPanel;

    void Start()
    {
        if(GameManager.instance.debugMode)
            debugPanel.gameObject.transform.parent.gameObject.SetActive(true);
        else
            debugPanel.gameObject.transform.parent.gameObject.SetActive(false);

        tileID = new int[2];
        tileID[0] = Mathf.RoundToInt(transform.position.x);
        tileID[1] = Mathf.RoundToInt(transform.position.z);
        float yPos = transform.position.y;
        node = new Node(tileID[0], yPos, tileID[1], true);
        
        GameManager.instance.currentSceneTiles[GameManager.instance.TileListIndexConversion(tileID[0]), GameManager.instance.TileListIndexConversion(tileID[1])] = this;
        
        GameManager.instance.currentSceneNodes[GameManager.instance.TileListIndexConversion(tileID[0]), GameManager.instance.TileListIndexConversion(tileID[1])] = node;

        PathfindingManager.instance.ResetEvent += ResetAssignment;
    }

    private void OnDestroy() 
    {
        PathfindingManager.instance.ResetEvent -= ResetAssignment;    
        UIManager.instance.TileUIEvent -= TileUIAssignment;
    }

    void Update()
    {
        if(GameManager.instance.debugMode && debugPanel.gameObject.transform.parent.gameObject.activeInHierarchy == false)
            debugPanel.gameObject.transform.parent.gameObject.SetActive(true);
        else if(!GameManager.instance.debugMode && debugPanel.gameObject.transform.parent.gameObject.activeInHierarchy == true)
            debugPanel.gameObject.transform.parent.gameObject.SetActive(false);
        
        if(GameManager.instance.debugMode)
        {
            fCostField.text = node.fCost().ToString();
            gCostField.text = node.gCost.ToString();
            hCostField.text = node.hCost.ToString();

            if(node.closed || !node.walkable || node.occupied)
            {
                debugPanel.color = Color.red;
            }

            if(node.open)
            {
                debugPanel.color = Color.green;
            }

            if(!node.closed && !node.open && node.walkable && !node.occupied)
            {
                debugPanel.color = Color.gray;
            }
        }
    }

    public void ResetAssignment(object sender, EventArgs args)
    {
        node.hCost = 0;
        node.gCost = 0;
        node.parent = null;
        node.open = false;
        node.closed = false;
    }

    public void TileUIAssignment(object sender, EventArgs args)
    {
        if(movementIndicatorPanel.IsActive())
        {
            movementIndicatorPanel.gameObject.transform.parent.gameObject.SetActive(false);
            RemoveUIAssignment();
        }
        else
            movementIndicatorPanel.gameObject.transform.parent.gameObject.SetActive(true);
    }

    public void RemoveUIAssignment()
    {
        UIManager.instance.TileUIEvent -= TileUIAssignment; 
    }

    public void AddUIAssignment()
    {
        UIManager.instance.TileUIEvent += TileUIAssignment; 
    }
}
