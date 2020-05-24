using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    CharacterScript character;
    CameraManager cameraManager;
    Ray myRay;
    LayerMask layerMaskTile, layerMaskObject;
    float pinchPreviousDistance, touchDuration, startTime;
    Vector2 startPos;
    bool couldBeSwipe;
    public float minSwipeDist, maxSwipeDist, maxSwipeDuration;

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
        // if(Input.mouseScrollDelta.y != 0)
        //     cameraManager.Zoom(Input.mouseScrollDelta.y);

        if(Input.mouseScrollDelta.y < 0)
            CameraManager.instance.ZoomOut();

        else if(Input.mouseScrollDelta.y > 0)
            CameraManager.instance.ZoomIn();
    }

    void Touch()
    {
        if (Input.touchCount > 0)
        {
            Touch[] touch = new Touch[Input.touchCount];

            for(int i = 0; i < Input.touchCount; ++i)
            {
                touch[i] = Input.GetTouch(i);
            }

            if(Input.touchCount > 1)
            {
                float pinchCurrentDistance = Vector2.Distance(touch[0].position, touch[1].position);

                if (pinchCurrentDistance > pinchPreviousDistance)
                {
                    CameraManager.instance.ZoomOut();
                } 
                else if (pinchCurrentDistance < pinchPreviousDistance)
                {
                    CameraManager.instance.ZoomIn();
                }

                pinchPreviousDistance = pinchCurrentDistance;
            }
            else if(touch[0].phase == TouchPhase.Began)
            {
                couldBeSwipe = true;
                startPos = touch[0].position;
                startTime = Time.time;
            }
            else if(touch[0].phase == TouchPhase.Moved)
            {
                float swipeDuration = Time.time - startTime;

                if (Mathf.Abs(touch[0].position.x - startPos.x) > maxSwipeDist || swipeDuration > maxSwipeDuration) 
                {
                    couldBeSwipe = false;
                }
            }
            else if(touch[0].phase == TouchPhase.Stationary)
            {
                couldBeSwipe = false;
            }
            else if(touch[0].phase == TouchPhase.Ended)
            {
                if(couldBeSwipe)
                {
                    float swipeDuration = Time.time - startTime;
                    float swipeDist = (touch[0].position - startPos).magnitude;

                    if ((swipeDuration < maxSwipeDuration) && (swipeDist > minSwipeDist)) 
                    {
                        // It's a swiiiiiiiiiiiipe!
                        float swipeDirection = Mathf.Sign(touch[0].position.x - startPos.x);
                   
                        // Do something here in reaction to the swipe.
                        if (swipeDirection > 0)
                            CameraManager.instance.RotateCounterClockwise();
                        else if(swipeDirection < 0)
                            CameraManager.instance.RotateClockwise();
                    }
                }
                
                else
                {
                    RaycastHit hit;
                    myRay = cameraManager.mainCamera.ScreenPointToRay(touch[0].position); 
                
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
