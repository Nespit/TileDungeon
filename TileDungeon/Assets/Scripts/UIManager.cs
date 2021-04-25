using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Text announcement;
    public Text coinCounter;
    public GameObject tileSelectorPrefab;
    public GameObject tileSelector;
    public GameObject activityIndicatorPrefab;
    public GameObject activityIndicator;

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

    public void SelectTile(Transform tile)
    {
        tileSelector.transform.position = tile.transform.position;
        tileSelector.SetActive(true);
    }

    public void DeselectTile()
    {
        tileSelector.SetActive(false);
    }
}
