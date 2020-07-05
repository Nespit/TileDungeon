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
    public string filePath;
    CharacterScript playerCharacter;
    InputManager inputManager;
    CameraManager cameraManager;
    public WaitUntil m_setSceneActiveCondition;
    Coroutine m_setSceneActive;
    public List<SavedListsPerScene> savedLists;
    public SavedDataPerGame savedGameData;
    public delegate void SaveDelegate(object sender, EventArgs args);
    public event SaveDelegate SaveEvent;
    public bool IsSceneBeingLoaded = false;
    public GameObject keyPrefab, coinPrefab, doorPrefab, enemyPrefab;
    public Dictionary<float, Transform> currentSceneTiles;
    GameState gameState;
    public MainMenu mainMenu;


	void Awake()
	{
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);    
		}
		DontDestroyOnLoad(gameObject);

        if(Application.platform == RuntimePlatform.Android)
            filePath = Application.persistentDataPath + "/TileDungeon/SaveFiles/";
        else
            filePath = Application.persistentDataPath + "/SaveFiles/";
	}

    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));

        InitializeSceneList();
        PrepareTileDictionary();

        playerCharacter = GameObject.FindGameObjectWithTag("PlayerCharacter").GetComponent<CharacterScript>();
        inputManager = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputManager>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();        

        if (SceneManager.sceneCount >= 2)
        {
            //SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
            m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneAt(1)));

            if(mainMenu.gameObject.activeInHierarchy)
                Menu();
            
            gameState = GameState.Game;
            return;
        }

        if(!mainMenu.gameObject.activeInHierarchy)
            Menu();

        gameState = GameState.MainMenu;
    }

    // Update is called once per frame
    void Update()
    {
        if(gameState == GameState.Game)
        {
            TurnManager.instance.Turn();
            inputManager.ProcessInput();
        }      
    }

    IEnumerator NewGame()
    {
        DiscardData();

        if(SceneManager.sceneCount > 1)
        {
            playerCharacter.transform.parent = transform;
            Scene scene = SceneManager.GetActiveScene();

            m_setSceneActiveCondition = new WaitUntil(() => !scene.isLoaded);
            SceneManager.UnloadSceneAsync(scene);
        }
        else m_setSceneActiveCondition = new WaitUntil(() => true);

        yield return m_setSceneActiveCondition;

        SceneManager.LoadScene(1, LoadSceneMode.Additive);

        playerCharacter.transform.position = new Vector3(0f, 0.26f, 0f);
        CameraManager.instance.targetTargetPos = playerCharacter.transform.position;
        CameraManager.instance.transform.position = playerCharacter.transform.position + CameraManager.instance.offset[CameraManager.instance.offsetIndex];
        CameraManager.instance.transform.LookAt(CameraManager.instance.target);
        playerCharacter.transform.rotation = Quaternion.Euler(0, 0, 0);
        playerCharacter.hasKey = false;
        playerCharacter.currentHealth = 100;

        if(mainMenu.gameObject.activeInHierarchy)
        {
            yield return new WaitForEndOfFrame();

            Menu();
        }

        m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(1).isLoaded);
        m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(1)));
    }

    IEnumerator OldGame()
    {
        if(!File.Exists(filePath + "saveObjects.binary") || !File.Exists(filePath + "saveGame.binary"))
            yield break;

        DiscardData();

        if(SceneManager.sceneCount > 1)
        {
            playerCharacter.transform.parent = transform;
            Scene scene = SceneManager.GetActiveScene();

            m_setSceneActiveCondition = new WaitUntil(() => !scene.isLoaded);
            SceneManager.UnloadSceneAsync(scene);
        }
        else m_setSceneActiveCondition = new WaitUntil(() => true);

        yield return m_setSceneActiveCondition;
       
        LoadData();

        SceneManager.LoadScene(savedGameData.SceneID, LoadSceneMode.Additive);

        playerCharacter.transform.position = savedGameData.position;
        CameraManager.instance.targetTargetPos = playerCharacter.transform.position;
        CameraManager.instance.transform.position = playerCharacter.transform.position + CameraManager.instance.offset[CameraManager.instance.offsetIndex];
        CameraManager.instance.transform.LookAt(CameraManager.instance.target);
        playerCharacter.transform.rotation = savedGameData.rotation;
        playerCharacter.hasKey = savedGameData.hasKey;
        playerCharacter.currentHealth = savedGameData.currentHealth;

        if(mainMenu.gameObject.activeInHierarchy)
        {
            yield return new WaitForEndOfFrame();

            Menu();
        }

        m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(savedGameData.SceneID).isLoaded);
        m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(savedGameData.SceneID)));
    }

    public void LoadGame()
    {
        m_setSceneActive = StartCoroutine(OldGame());
    }

    public void StartNewGame()
    {
        m_setSceneActive = StartCoroutine(NewGame());
    }
    IEnumerator SetSceneActive(Scene scene)
    {
        yield return m_setSceneActiveCondition;
        
        SceneManager.SetActiveScene(scene);

        SavedListsPerScene localListOfSceneObjectsToLoad = GetListForScene();

        if (localListOfSceneObjectsToLoad != null)
        {
            GameObject spawnedObject;

            for (int i = 0; i < localListOfSceneObjectsToLoad.SavedInteractableObjects.Count; i++)
            {
                switch(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].objectType)
                {
                    case InteractableObjectType.door:
                        //Debug.Log("Spawn door");
                        spawnedObject = Instantiate(doorPrefab,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
                        spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID)];
                        break;
                    case InteractableObjectType.coin:
                        //Debug.Log("Spawn coin");
                        spawnedObject = Instantiate(coinPrefab,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
                        spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID)];                            
                        break;
                    case InteractableObjectType.key:
                        //Debug.Log("Spawn key");
                        spawnedObject = Instantiate(keyPrefab,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
                        spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID)];
                        break;
                }
            }

            for (int i = 0; i < localListOfSceneObjectsToLoad.SavedCharacters.Count; i++)
            {
                spawnedObject = Instantiate(enemyPrefab,
                                            localListOfSceneObjectsToLoad.SavedCharacters[i].position,
                                            localListOfSceneObjectsToLoad.SavedCharacters[i].rotation);
                var script = spawnedObject.GetComponent<CharacterScript>();
                script.SetCurrentHealth(localListOfSceneObjectsToLoad.SavedCharacters[i].currentHealth);
                script.maxHealth = localListOfSceneObjectsToLoad.SavedCharacters[i].maxHealth;
                script.maxActionPoints = localListOfSceneObjectsToLoad.SavedCharacters[i].maxActionPoints;
                script.turnOrderRating = localListOfSceneObjectsToLoad.SavedCharacters[i].turnOrderRating;
                script.attackStrength = localListOfSceneObjectsToLoad.SavedCharacters[i].attackStrength;
                script.defenseStrength = localListOfSceneObjectsToLoad.SavedCharacters[i].defenseStrength;
                spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedCharacters[i].tileID)];
            }
        }
        else   
            InitializeSceneList();

        CameraManager.instance.UpdateStaticTransparencyBoundingBox();
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
        PrepareTileDictionary();

        IsSceneBeingLoaded = true;
        m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex+1).isLoaded);
        SceneManager.LoadSceneAsync(currentSceneIndex+1, LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(currentSceneIndex);
        m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(currentSceneIndex+1)));
    }

    public void MoveToPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if(currentSceneIndex - 1 < 1)
        {
            return;
        }
        
        InitializeSceneList();
        SaveData();
        PrepareTileDictionary();

        IsSceneBeingLoaded = true;
        m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex-1).isLoaded);
        SceneManager.LoadSceneAsync(currentSceneIndex-1, LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(currentSceneIndex);
        m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(currentSceneIndex-1)));
    }

    void PrepareTileDictionary()
    {
        currentSceneTiles = new Dictionary<float, Transform>();
    }

    public void InitializeSceneList()
    {
        if (savedLists == null)
        {
            //print("Saved lists was null");
            savedLists = new List<SavedListsPerScene>();
        }

        bool found = false;

        //We need to find if we already have a list of saved items for this level:
        for (int i = 0; i < savedLists.Count; i++)
        {
            if (savedLists[i].SceneID == SceneManager.GetActiveScene().buildIndex)
            {
                found = true;
            }
        }

        //If not, we need to create it:
        if (!found)
        {           
            SavedListsPerScene newList = new SavedListsPerScene(SceneManager.GetActiveScene().buildIndex);
            savedLists.Add(newList);
        }
    }

    public SavedListsPerScene GetListForScene(int sceneBuildIndex = -1)
    {
        if(sceneBuildIndex >= 0)
        {
            for (int i = 0; i < savedLists.Count; i++)
            {
                if (savedLists[i].SceneID == sceneBuildIndex)
                    return savedLists[i];
            }
        }
        
        else
        {
            for (int i = 0; i < savedLists.Count; i++)
            {
                if (savedLists[i].SceneID == SceneManager.GetActiveScene().buildIndex)
                    return savedLists[i];
            }
        }

        return null;
    }

    public void FireSaveEvent()
    {
        savedGameData = new SavedDataPerGame(SceneManager.GetActiveScene().buildIndex, playerCharacter.gameObject.transform.position, playerCharacter.gameObject.transform.rotation, playerCharacter.currentHealth, playerCharacter.hasKey);
        GetListForScene().SavedCharacters = new List<SavedCharacter>();
        GetListForScene().SavedInteractableObjects = new List<SavedInteractableObject>();
        //If we have any functions in the event:
        if (SaveEvent != null)
            SaveEvent(null, null);
    }

    public void SaveData()
    {
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        FireSaveEvent();  

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveObjects = File.Create(filePath + "saveObjects.binary");

        formatter.Serialize(saveObjects, savedLists);

        saveObjects.Close();


        saveObjects = File.Create(filePath + "saveGame.binary");

        formatter.Serialize(saveObjects, savedGameData);
        
        saveObjects.Close();
    }

    public void LoadData()
    {
        if(!File.Exists(filePath + "saveObjects.binary") || !File.Exists(filePath + "saveGame.binary"))
            return;

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveObjects = File.Open(filePath + "saveObjects.binary", FileMode.Open);

        savedLists = (List<SavedListsPerScene>)formatter.Deserialize(saveObjects);
    
        saveObjects.Close();

        saveObjects = File.Open(filePath + "saveGame.binary", FileMode.Open);

        savedGameData = (SavedDataPerGame)formatter.Deserialize(saveObjects);
    
        saveObjects.Close();
    }

    public void DiscardData()
    {
        if (savedLists != null)
        {
            savedLists = new List<SavedListsPerScene>();
            savedGameData = null;
            PrepareTileDictionary();
        }
    }

    public void Menu()
    {
        mainMenu.gameObject.SetActive(!mainMenu.gameObject.activeInHierarchy);
        mainMenu.CheckButtonLayout();

        if(mainMenu.gameObject.activeInHierarchy)
            gameState = GameState.MainMenu;
        else    
            gameState = GameState.Game;
    }
}

enum GameState
{
    MainMenu,
    Game
}
