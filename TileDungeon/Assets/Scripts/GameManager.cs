using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    CharacterScript playerCharacter;
    InputManager inputManager;
    CameraManager cameraManager;

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
    }

    // Update is called once per frame
    void Update()
    {
        inputManager.ProcessInput();
    }
}
