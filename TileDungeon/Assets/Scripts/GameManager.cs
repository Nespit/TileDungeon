using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    CharacterScript playerCharacter;
    InputManager inputManager;
    CameraManager cameraManager;
    public WaitUntil m_setSceneActiveCondition;
    Coroutine m_setSceneActive;
    public List<SavedObjectsList> SavedLists;
    public delegate void SaveDelegate(object sender, EventArgs args);
    public event SaveDelegate SaveEvent;
    public bool IsSceneBeingLoaded = false;
    public GameObject keyPrefab, coinPrefab, doorPrefab, enemyPrefab;

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
        InitializeSceneList();

        playerCharacter = GameObject.FindGameObjectWithTag("PlayerCharacter").GetComponent<CharacterScript>();
        inputManager = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputManager>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();        

        if (SceneManager.sceneCount < 2)
        {
            SceneManager.LoadScene(1, LoadSceneMode.Additive);
            m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(1).isLoaded);
            m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(1)));
        } 
        else
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        }
    }

    // Update is called once per frame
    void Update()
    {
        inputManager.ProcessInput();
    }

    IEnumerator SetSceneActive(Scene scene)
    {
        yield return m_setSceneActiveCondition;
        
        SceneManager.SetActiveScene(scene);

        SavedObjectsList localListOfSceneObjectsToLoad = GetListForScene();

        if (localListOfSceneObjectsToLoad != null)
        {
            GameObject spawnedObject;

            for (int i = 0; i < localListOfSceneObjectsToLoad.SavedInteractableObjects.Count; i++)
            {
                switch(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].objectType)
                {
                    case InteractableObjectType.door:
                        Debug.Log("Spawn door");
                         spawnedObject = Instantiate(doorPrefab,
                                                     localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                                     localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
                        
                        break;
                    case InteractableObjectType.coin:
                        Debug.Log("Spawn coin");
                        spawnedObject = Instantiate(coinPrefab,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
                        
                        break;
                    case InteractableObjectType.key:
                        Debug.Log("Spawn key");
                        spawnedObject = Instantiate(keyPrefab,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
                        break;
                }
            }

            for (int i = 0; i < localListOfSceneObjectsToLoad.SavedCharacters.Count; i++)
            {
                Debug.Log("Spawn character");
                spawnedObject = Instantiate(enemyPrefab,
                                            localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                            localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
            }
        }
        else   
            InitializeSceneList();

        IsSceneBeingLoaded = false;
        // foreach(GameObject g in scene.GetRootGameObjects())
        // {
        //     g.SetActive (true);
        // }

        m_setSceneActive = null;
    }

    public void MoveToNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        if(currentSceneIndex + 1 >= sceneCount)
        {
            return;
        }
        
        InitializeSceneList();
        SaveData();

        IsSceneBeingLoaded = true;
        SceneManager.LoadSceneAsync(currentSceneIndex+1, LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(currentSceneIndex);
        m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex+1).isLoaded);
        m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(currentSceneIndex+1)));
    }

    public void MoveToPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if(currentSceneIndex + 1 <= 1)
        {
            return;
        }
        
        InitializeSceneList();
        SaveData();

        IsSceneBeingLoaded = true;
        SceneManager.LoadSceneAsync(currentSceneIndex-1, LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(currentSceneIndex);
        m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex-1).isLoaded);
        m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(currentSceneIndex-1)));
    }

    // public void MoveToPreviousScene()
    // {
    //     int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

    //     if(currentSceneIndex <= 1)
    //     {
    //         return;
    //     }
            
    //     if(SceneManager.GetSceneByBuildIndex(currentSceneIndex-1).isLoaded)
    //     {
    //         foreach(GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
    //         {
    //             g.SetActive (false);
    //         }

    //         m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex-1).isLoaded);

    //         SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(currentSceneIndex-1));

    //         foreach(GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
    //         {
    //             g.SetActive (true);
    //         }
    //     }

    //     else
    //     {
    //         foreach(GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
    //         {
    //             g.SetActive (false);
    //         }


    //         SceneManager.LoadScene(currentSceneIndex-1, LoadSceneMode.Additive);
    //         m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex-1).isLoaded);
    //         m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(currentSceneIndex-1)));
    //     }
    // }

    public void InitializeSceneList()
    {
        if (SavedLists == null)
        {
            print("Saved lists was null");
            SavedLists = new List<SavedObjectsList>();
        }

        bool found = false;

        //We need to find if we already have a list of saved items for this level:
        for (int i = 0; i < SavedLists.Count; i++)
        {
            if (SavedLists[i].SceneID == SceneManager.GetActiveScene().buildIndex)
            {
                found = true;
                print("Scene was found in saved lists!");
            }
        }

        //If not, we need to create it:
        if (!found)
        {           
            SavedObjectsList newList = new SavedObjectsList(SceneManager.GetActiveScene().buildIndex);
            SavedLists.Add(newList);

            print("Created new list!");
        }
    }

    public SavedObjectsList GetListForScene(int sceneBuildIndex = -1)
    {
        if(sceneBuildIndex >= 0)
        {
            for (int i = 0; i < SavedLists.Count; i++)
            {
                if (SavedLists[i].SceneID == sceneBuildIndex)
                    return SavedLists[i];
            }
        }
        
        else
        {
            for (int i = 0; i < SavedLists.Count; i++)
            {
                if (SavedLists[i].SceneID == SceneManager.GetActiveScene().buildIndex)
                    return SavedLists[i];
            }
        }
        

        print("Total list count: " + SavedLists.Count.ToString() + " , not found index: " + SceneManager.GetActiveScene().buildIndex.ToString());
        return null;
    }

    public void FireSaveEvent()
    {
        GetListForScene().SavedCharacters = new List<SavedCharacter>();
        GetListForScene().SavedInteractableObjects = new List<SavedInteractableObject>();
        //If we have any functions in the event:
        if (SaveEvent != null)
            SaveEvent(null, null);
    }

    public void SaveData()
    {
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");

        FireSaveEvent();  

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream SaveObjects = File.Create("saves/saveObjects.binary");

        formatter.Serialize(SaveObjects, SavedLists);

        SaveObjects.Close();

        print("Saved!");
    }

    public void LoadData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveObjects = File.Open("Saves/saveObjects.binary", FileMode.Open);

        SavedLists = (List<SavedObjectsList>)formatter.Deserialize(saveObjects);
    
        saveObjects.Close();

        print("Loaded");
    }
}
