using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    CharacterScript character;
    CameraManager cameraManager;
    Ray myRay;
    LayerMask layerMaskTile, layerMaskObject;

    // Start is called before the first frame update
    void Start()
    {
        character = GameObject.FindGameObjectWithTag("PlayerCharacter").GetComponent<CharacterScript>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
        layerMaskTile = LayerMask.GetMask("Tiles");
        layerMaskObject = LayerMask.GetMask("Objects");
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
        
            if(Physics.Raycast(myRay, out hit, 1000, layerMaskObject))
            {
                character.MoveToLocation(hit.transform.parent.transform.position);
            }
            else if(Physics.Raycast(myRay, out hit, 1000, layerMaskTile))
            {
                Debug.Log("Hit");
                character.MoveToLocation(hit.transform.position);
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
            
            if(Physics.Raycast(myRay, out hit, 1000, layerMaskObject))
                {
                    character.MoveToLocation(hit.transform.parent.transform.position);
                }
            else if(Physics.Raycast(myRay, out hit, 1000, layerMaskTile))
                {
                    character.MoveToLocation(hit.transform.position);
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
