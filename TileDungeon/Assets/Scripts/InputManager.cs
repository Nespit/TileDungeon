using System.Collections;
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
        character = GameObject.FindGameObjectWithTag("PlayerCharacter").GetComponent<CharacterScript>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
        layerMaskTile = LayerMask.GetMask("Tiles");
    }

    public void ProcessInput()
    {
        MouseClick();
        Touch();
        Keyboard();
        MouseScroll();
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

    void MouseScroll()
    {
        if(Input.mouseScrollDelta.y != 0)
            cameraManager.Zoom(Input.mouseScrollDelta.y);
    }

    void Touch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if(touch.phase == TouchPhase.Ended)
            {
                RaycastHit hit;
                myRay = cameraManager.mainCamera.ScreenPointToRay(touch.position); 
            
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

    void Keyboard()
    {
        if(Input.GetKeyUp(KeyCode.LeftArrow))
        {
            cameraManager.RotateCounterClockwise();
        }
        
        if(Input.GetKeyUp(KeyCode.RightArrow))
        {
            cameraManager.RotateClockwise();
        }
    }
}
