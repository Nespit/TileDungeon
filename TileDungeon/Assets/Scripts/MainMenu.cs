using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenu : MonoBehaviour
{
    public Button newGameButton, continueButton, resumeButton, saveButton, loadButton, exitButton;
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
        CheckButtonLayout();
    }

    public void CheckButtonLayout()
    {
        if(File.Exists(GameManager.instance.filePath + "saveObjects.binary") && File.Exists(GameManager.instance.filePath + "saveGame.binary"))
            loadButton.gameObject.SetActive(true);
        else
            loadButton.gameObject.SetActive(false);

        if(SceneManager.sceneCount < 2)
        {
            saveButton.gameObject.SetActive(false);
            resumeButton.gameObject.SetActive(false);
        }
        else
        {
            saveButton.gameObject.SetActive(true);
            resumeButton.gameObject.SetActive(true);
        }
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
