using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Text announcement;
    public Text coinCounter;
    public GameObject tileSelectorPrefab;
    public GameObject tileSelector;
    public GameObject activityIndicatorPrefab;
    public GameObject activityIndicator;

    public delegate void TileUIDelegate(object sender, EventArgs args);
    public event TileUIDelegate TileUIEvent;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) 
        {
			instance = this;
		} 
        else if (instance != this) 
        {
			Destroy(gameObject);    
		}
		DontDestroyOnLoad(gameObject);
    }

    void Start() 
    {
        tileSelector = Instantiate(tileSelectorPrefab);
        tileSelector.SetActive(false);
    }

    public void SelectTile(Vector3 position, bool inRange)
    {
        tileSelector.transform.position = position;

        MeshRenderer renderer = tileSelector.GetComponent<MeshRenderer>();
        
        if(inRange)
            renderer.material.color = Color.green;
        else
            renderer.material.color = Color.red;

        tileSelector.SetActive(true);
    }

    public void DeselectTile()
    {
        tileSelector.SetActive(false);
    }

    public void HighlightReachableTiles(TileScript characterTile, int actionPoints)
    {
        if(TileUIEvent != null)
        {
            TileUIEvent(null, null); //clears currently activated tiles
        }
        
        List<TileScript> tiles = GameManager.instance.GetReachableTileNeighboursInRadius(characterTile, actionPoints);

        for(int i = 0; i < tiles.Count; ++i)
        {
            tiles[i].AddUIAssignment();
        }

        //Highlights reachable tiles
        if(TileUIEvent != null)
        {
            TileUIEvent(null, null);
        } 
    }

    public void UnHighlightLitTiles()
    {
        if(TileUIEvent != null)
        {
            TileUIEvent(null, null);
        }
    }
}
