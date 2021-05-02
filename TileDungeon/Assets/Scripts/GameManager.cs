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
    public GameObject keyPrefab, coinPrefab, doorPrefab, enemyPrefab, playerCharacterPrefab;
    //public Dictionary<float, Transform> currentSceneTiles;
    int mapSizeRoot = 100; //needs to be dividable by 2
    public Transform[,] currentSceneTiles;
    public Node[,] currentSceneNodes;
    public GameState gameState;
    public MainMenu mainMenu;
    public bool debugMode;

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

        Application.targetFrameRate = 60;
	}

    void Start()
    {
        currentSceneTiles = new Transform[mapSizeRoot,mapSizeRoot];
        currentSceneNodes = new Node[mapSizeRoot,mapSizeRoot];
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
        if(gameState == GameState.Game && !IsSceneBeingLoaded)
        {
            TurnManager.instance.Turn();
            inputManager.ProcessInput();
        }      
    }
    public int TileListIndexConversion(int tileID)
    {
        tileID = tileID + mapSizeRoot/2;
        return tileID;
    }

    public List<Node> GetViableNodeNeighbours(Node node, Node targetNode)
    {
        List<Node> neighbours = new List<Node>();

        for(int x = -1; x <= 1; x++)
        {
            for(int z = -1; z <= 1; z++)
            {
                if(x == 0 && z == 0)
                    continue;

                if(currentSceneNodes[TileListIndexConversion((int)node.position.x + x), TileListIndexConversion((int)node.position.z + z)] == targetNode)
                {
                    neighbours.Add(currentSceneNodes[TileListIndexConversion((int)node.position.x + x), TileListIndexConversion((int)node.position.z + z)]);
                    continue;
                }
                
                if(Mathf.Abs(node.position.x + x) <= mapSizeRoot && Mathf.Abs(node.position.z + z) <= mapSizeRoot &&
                    currentSceneNodes[TileListIndexConversion((int)node.position.x + x), TileListIndexConversion((int)node.position.z + z)] != null && 
                    !currentSceneNodes[TileListIndexConversion((int)node.position.x + x), TileListIndexConversion((int)node.position.z + z)].closed && 
                    currentSceneNodes[TileListIndexConversion((int)node.position.x + x), TileListIndexConversion((int)node.position.z + z)].walkable &&
                    !currentSceneNodes[TileListIndexConversion((int)node.position.x + x), TileListIndexConversion((int)node.position.z + z)].occupied)
                neighbours.Add(currentSceneNodes[TileListIndexConversion((int)node.position.x + x), TileListIndexConversion((int)node.position.z + z)]);
            }
        }

        return neighbours;
    }

    void InstantiateCharacter()
    {
        if(playerCharacter != null)
            return;

        var player = Instantiate(playerCharacterPrefab, transform.position, transform.rotation, transform);
        playerCharacter = player.GetComponent<CharacterScript>();
        playerCharacter.winAnnouncement = UIManager.instance.announcement;
        playerCharacter.winAnnouncement.gameObject.SetActive(false);
        playerCharacter.coinCounter = UIManager.instance.coinCounter;
        InputManager.instance.character = playerCharacter;
        CameraManager.instance.target = playerCharacter.transform;
    }

    IEnumerator NewGame()
    {
        DiscardData();

        InstantiateCharacter();

        yield return new WaitForEndOfFrame();

        if(SceneManager.sceneCount > 1)
        {
            playerCharacter.transform.parent = transform;
            Scene scene = SceneManager.GetActiveScene();

            m_setSceneActiveCondition = new WaitUntil(() => !scene.isLoaded);
            SceneManager.UnloadSceneAsync(scene);
        }
        else m_setSceneActiveCondition = new WaitUntil(() => true);

        yield return m_setSceneActiveCondition;

        m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(1).isLoaded);
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

        m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(1)));
    }

    IEnumerator OldGame()
    {
        if(!File.Exists(filePath + "saveObjects.binary") || !File.Exists(filePath + "saveGame.binary"))
            yield break;

        DiscardData();

        InstantiateCharacter();

        yield return new WaitForEndOfFrame();

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

        m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(savedGameData.SceneID).isLoaded);
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

        m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(savedGameData.SceneID), true));
    }

    public void LoadGame()
    {
        m_setSceneActive = StartCoroutine(OldGame());
    }

    public void StartNewGame()
    {
        m_setSceneActive = StartCoroutine(NewGame());
    }
    IEnumerator SetSceneActive(Scene scene, bool loadSavedPlayerCharacter = false)
    {
        yield return m_setSceneActiveCondition;
        //Debug.Log("Scene loading finished, begin setting the scene active.");
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
                        spawnedObject = Instantiate(doorPrefab,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
                        //spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID)];
                        spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID[0]), (localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID[1])];
                        break;
                    case InteractableObjectType.coin:
                        spawnedObject = Instantiate(coinPrefab,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
                        //spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID)];                            
                        spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID[0]), (localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID[1])];
                        break;
                    case InteractableObjectType.key:
                        spawnedObject = Instantiate(keyPrefab,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].position,
                                                    localListOfSceneObjectsToLoad.SavedInteractableObjects[i].rotation);
                        //spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID)];
                        spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID[0]), (localListOfSceneObjectsToLoad.SavedInteractableObjects[i].tileID[1])];
                        break;
                }
            }

            for (int i = 0; i < localListOfSceneObjectsToLoad.SavedCharacters.Count; i++)
            {
                if(localListOfSceneObjectsToLoad.SavedCharacters[i].behaviour == CharacterBehaviourType.player && !loadSavedPlayerCharacter)
                {    
                    continue;
                }
                else if(localListOfSceneObjectsToLoad.SavedCharacters[i].behaviour == CharacterBehaviourType.player)
                {
                    spawnedObject = Instantiate(playerCharacterPrefab,
                                                localListOfSceneObjectsToLoad.SavedCharacters[i].position,
                                                localListOfSceneObjectsToLoad.SavedCharacters[i].rotation);
                }
                else
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
                script.behaviour = localListOfSceneObjectsToLoad.SavedCharacters[i].behaviour;
                //spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedCharacters[i].tileID)];
                spawnedObject.transform.parent = currentSceneTiles[(localListOfSceneObjectsToLoad.SavedCharacters[i].tileID[0]), (localListOfSceneObjectsToLoad.SavedCharacters[i].tileID[1])];
            }
        }
        else   
            InitializeSceneList();

        CameraManager.instance.UpdateStaticTransparencyBoundingBox();
        
        yield return new WaitForFixedUpdate();
        TurnManager.instance.StartNewTurn();
        IsSceneBeingLoaded = false;
        m_setSceneActive = null;
    }

    public void MoveToNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        //Debug.Log("Current Scene Index = " + currentSceneIndex);
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        TurnManager.instance.WipeTurnQueue();

        if(currentSceneIndex + 1 >= sceneCount)
        {
            //Debug.Log("Tried loading an invalid scene. Number of scenes in the build settings = " + sceneCount + ". Index of scene that was attempted to be loaded = " + (currentSceneIndex + 1));
            return;
        }
        
        InitializeSceneList();
        SaveData();
        PrepareTileDictionary();

        IsSceneBeingLoaded = true;

        m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex+1).isLoaded);
        SceneManager.LoadSceneAsync((currentSceneIndex+1), LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(currentSceneIndex);
        m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(currentSceneIndex+1)));
    }

    public void MoveToPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        //Debug.Log("Current Scene Index = " + currentSceneIndex);
        TurnManager.instance.WipeTurnQueue();

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
        //currentSceneTiles = new Dictionary<float, Transform>();
        currentSceneTiles = new Transform[mapSizeRoot, mapSizeRoot];
        currentSceneNodes = new Node[mapSizeRoot, mapSizeRoot];
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
        else if(playerCharacter != null)   
            gameState = GameState.Game;
    }
}

public enum GameState
{
    MainMenu,
    Game
}
