﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    CharacterScript character;
    GameManager gameManager;
    CameraManager cameraManager;
    Ray myRay;
    LayerMask layerMaskTile;

    // Start is called before the first frame update
    void Start()
    {
        character = GameObject.FindGameObjectWithTag("Character").GetComponent<CharacterScript>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
        layerMaskTile = LayerMask.GetMask("Tiles");
    }

    // Update is called once per frame
    void Update()
    {
        MouseClick();
    }

    void ProcessInput()
    {

    }

    void MouseClick()
    {
        if(Input.GetMouseButtonUp(0)) //0 is the primary mouse button.
        {
            RaycastHit hit;
            myRay = cameraManager.mainCamera.ScreenPointToRay(Input.mousePosition); 
        
            if(Physics.Raycast(myRay, out hit, 1000, layerMaskTile))
            {
                if(hit.collider.tag == "Tile")
                {
                   character.MoveToLocation(hit.transform.position);
                }
            }
        }  
    }
}