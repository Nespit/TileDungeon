using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button newGameButton, continueButton, resumeButton, exitButton;
    public static MainMenu instance;
    
    void Awake()
	{
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);    
		}
		DontDestroyOnLoad(gameObject);
	}
    void Start()
    {
        
    }

    public void NewGame()
    {
        GameManager.instance.StartNewGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Save()
    {
        GameManager.instance.SaveData();
    }
    public void ResumeGame()
    {
        GameManager.instance.Menu();
    }

    public void LoadGame()
    {
        GameManager.instance.LoadGame();
    }
}
