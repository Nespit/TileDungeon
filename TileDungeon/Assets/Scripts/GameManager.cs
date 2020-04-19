using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    CharacterScript playerCharacter;
    InputManager inputManager;
    CameraManager cameraManager;
    public WaitUntil m_setSceneActiveCondition;
    Coroutine m_setSceneActive;
    

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
        
        foreach(GameObject g in scene.GetRootGameObjects())
        {
            g.SetActive (true);
        }

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
            
        if(SceneManager.GetSceneByBuildIndex(currentSceneIndex+1).isLoaded)
        {
            foreach(GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                g.SetActive (false);
            }

            m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex+1).isLoaded);

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(currentSceneIndex+1));

            foreach(GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                g.SetActive (true);
            }
        }

        else
        {
            foreach(GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                g.SetActive (false);
            }


            SceneManager.LoadScene(currentSceneIndex+1, LoadSceneMode.Additive);
            m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex+1).isLoaded);
            m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(currentSceneIndex+1)));
        }
    }

    public void MoveToPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if(currentSceneIndex <= 1)
        {
            return;
        }
            
        if(SceneManager.GetSceneByBuildIndex(currentSceneIndex-1).isLoaded)
        {
            foreach(GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                g.SetActive (false);
            }

            m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex-1).isLoaded);

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(currentSceneIndex-1));

            foreach(GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                g.SetActive (true);
            }
        }

        else
        {
            foreach(GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                g.SetActive (false);
            }


            SceneManager.LoadScene(currentSceneIndex-1, LoadSceneMode.Additive);
            m_setSceneActiveCondition = new WaitUntil(() => SceneManager.GetSceneByBuildIndex(currentSceneIndex-1).isLoaded);
            m_setSceneActive = StartCoroutine(SetSceneActive(SceneManager.GetSceneByBuildIndex(currentSceneIndex-1)));
        }
    }
}
