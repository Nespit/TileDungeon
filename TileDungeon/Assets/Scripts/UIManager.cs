using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Text announcement;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
