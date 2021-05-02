using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public CharacterScript character;
    CameraManager cameraManager;
    Ray myRay;
    LayerMask layerMaskTile, layerMaskObject;
    float pinchStartDistance, touchDuration, startTime;
    Vector2 startPos;
    bool isSwipe;
    public float minSwipeDist, maxSwipeDist, maxSwipeDuration, minPinchDist, pressMaxDistance;
    int phaseBeginCount, phaseEndedCount = 0;

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
        character = GameObject.FindGameObjectWithTag("PlayerCharacter").GetComponent<CharacterScript>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraManager>();
        layerMaskTile = LayerMask.GetMask("Tiles");
        layerMaskObject = LayerMask.GetMask("Objects");
    }

    public void ProcessInput()
    {   
        if(Application.platform == RuntimePlatform.Android)
            Touch();
        else
        {
            MouseClick();
            Keyboard();
            MouseScroll();
        }
    }

    void MouseClick()
    {
        if(!character.turnActive)
            return;

        if (Input.GetMouseButtonUp(0)) //0 is the primary mouse button.
        {
            RaycastHit hit;
            myRay = cameraManager.mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(myRay, out hit, 1000, layerMaskObject))
            {
                if(UIManager.instance.tileSelector.transform.position != hit.transform.parent.transform.position)
                {
                    UIManager.instance.SelectTile(hit.transform.parent.transform);
                    character.TracePath(hit.transform.parent.transform.position);
                    return;
                }
                
                character.StartFollowingPath();
            }
            else if (Physics.Raycast(myRay, out hit, 1000, layerMaskTile))
            {
                if(UIManager.instance.tileSelector.transform.position != hit.transform.position)
                {
                    UIManager.instance.SelectTile(hit.transform);
                    character.TracePath(hit.transform.position);
                    return;
                }
                
                character.StartFollowingPath();
            }
        }
    }

    void MouseScroll()
    {
        // if(Input.mouseScrollDelta.y != 0)
        //     cameraManager.Zoom(Input.mouseScrollDelta.y);

        if (Input.mouseScrollDelta.y < 0)
            CameraManager.instance.ZoomOut();

        else if (Input.mouseScrollDelta.y > 0)
            CameraManager.instance.ZoomIn();
    }

    void Touch()
    {
        if (Input.touchCount > 0)
        {
            Touch[] touch = new Touch[Input.touchCount];

            for (int i = 0; i < Input.touchCount; ++i)
            {
                touch[i] = Input.GetTouch(i);
            }

            if (Input.touchCount > 1)
            {
                if (touch[1].phase == TouchPhase.Began)
                {
                    pinchStartDistance = Vector2.Distance(touch[0].position, touch[1].position);
                    isSwipe = false;
                }

                float pinchCurrentDistance = Vector2.Distance(touch[0].position, touch[1].position);

                if (Mathf.Abs(pinchCurrentDistance - pinchStartDistance) > minPinchDist)
                {
                    if (pinchCurrentDistance > pinchStartDistance)
                    {
                        pinchStartDistance = Vector2.Distance(touch[0].position, touch[1].position);
                        CameraManager.instance.ZoomIn();
                    }
                    else if (pinchCurrentDistance < pinchStartDistance)
                    {
                        pinchStartDistance = Vector2.Distance(touch[0].position, touch[1].position);
                        CameraManager.instance.ZoomOut();
                    }
                }
            }
            else
            {
                if (touch[0].phase == TouchPhase.Began)
                {
                    phaseBeginCount += 1;
                    isSwipe = true;
                    startPos = touch[0].position;
                    startTime = Time.time;
                }
                // else if (touch[0].phase == TouchPhase.Moved)
                // {
                //     float swipeDuration = Time.time - startTime;
                //     float swipeDist = Mathf.Abs(touch[0].position.y - startPos.y);

                //     if (swipeDist > maxSwipeDist || swipeDist < minSwipeDist || swipeDuration > maxSwipeDuration)
                //     {
                //         couldBeSwipe = false;
                //     }
                // }
                // else if (touch[0].phase == TouchPhase.Stationary)
                // {
                //     // couldBeSwipe = false;
                //     // Debug.Log("swipe interupted in stationary phase");
                // }
                else if (touch[0].phase == TouchPhase.Ended)
                {
                    float swipeDuration = Time.time - startTime;
                    float swipeDist = Vector2.Distance(touch[0].position, startPos);

                    //Debug.Log("swipe duration == " + swipeDuration);
                    //Debug.Log("swipe distance == " + swipeDist);

                    if (swipeDist > maxSwipeDist || swipeDist < minSwipeDist || swipeDuration > maxSwipeDuration)
                    {
                        isSwipe = false;
                    }

                    if (isSwipe)
                    {                        
                        // It's a swiiiiiiiiiiiipe!
                        float swipeDirection = touch[0].position.x - startPos.x;

                        // Do something here in reaction to the swipe.
                        if (swipeDirection > 0)
                            CameraManager.instance.RotateClockwise();
                        else if (swipeDirection < 0)
                            CameraManager.instance.RotateCounterClockwise();
                    }
                    
                    if (!isSwipe && swipeDist < pressMaxDistance)
                    {
                        if(!character.turnActive)
                            return;

                        RaycastHit hit;
                        myRay = cameraManager.mainCamera.ScreenPointToRay(touch[0].position);

                        if (Physics.Raycast(myRay, out hit, 1000, layerMaskObject))
                        {
                            if(!UIManager.instance.tileSelector.activeSelf || UIManager.instance.tileSelector.transform.position != hit.transform.parent.transform.position)
                            {
                                UIManager.instance.SelectTile(hit.transform.parent.transform);
                                character.TracePath(hit.transform.parent.transform.position);
                                return;
                            }
                            
                            character.StartFollowingPath();
                        }
                        else if (Physics.Raycast(myRay, out hit, 1000, layerMaskTile))
                        {
                            if(!UIManager.instance.tileSelector.activeSelf || UIManager.instance.tileSelector.transform.position != hit.transform.position)
                            {
                                UIManager.instance.SelectTile(hit.transform);
                                character.TracePath(hit.transform.position);
                                return;
                            }
                            
                            character.StartFollowingPath();
                        }
                    }
                }
            }
        }
    }

    void Keyboard()
    {
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            cameraManager.RotateCounterClockwise();
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            cameraManager.RotateClockwise();
        }
    }
}
