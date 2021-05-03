using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CharacterScript : MonoBehaviour
{
    //underastand difference between abstract interfaces and inheritance from classes, multiple static levels, lateral movement, second enemy type 
    
    public float offsetY;
    Vector3 moveDestination, initialPos;
    Quaternion targetRotation;
    Quaternion initialRotation;
    int layerMaskTile, layerMaskObject;
    WaitUntil m_movementCondition, m_characterTurnCondition, m_followPathCondition;
    public Coroutine m_characterAnimation;
    Coroutine m_characterTurn, m_followingPath;
    Ray myRay;
    public bool hasKey;
    int coinCount;
    public int coinsTilWin;
    public float movementSpeed = 2f;
    public float attackSpeed = 4f;
    float rotationSpeed;
    float t0;
    float t1;
    float t2;
    public float rotation90dSec = 0.25f;
    public Text coinCounter;
    public Text winAnnouncement;
    public bool moving = false;
    public bool animationActive = false;
    public int maxHealth;
    public int currentHealth;
    public int attackStrength;
    public int defenseStrength;
    public bool placedManually = false;
    public CharacterCanvasController characterCanvas;
    public Animator characterAnimator;
    public int maxActionPoints = 1;
    public int currentActionPoints; 
    public int turnOrderRating = 1; 
    public bool turnActive = false;
    public AudioSource audioSource;
    public CharacterBehaviourType behaviour;
    public bool skipAnimations = false;
    List<Node> unitPath;

    void Awake()
    {
        characterCanvas = GetComponentInChildren<CharacterCanvasController>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        targetRotation = transform.rotation;
        layerMaskTile = LayerMask.GetMask("Tiles");
        layerMaskObject = LayerMask.GetMask("Objects");
        unitPath = new List<Node>();
        m_movementCondition = new WaitUntil(() => moving == true && animationActive == false);
        m_followPathCondition = new WaitUntil(() => moving == false && animationActive == false);
        // currentActionPoints = maxActionPoints;
        // characterCanvas.SetActionPointsToMax(this);
        TurnManager.instance.TurnEvent += TurnOrderAssignment;
        SetBehaviourTo(behaviour);
        
        SetHealthbarFill();
        
        if (gameObject.tag == "Enemy")
        {
            GameManager.instance.SaveEvent += SaveFunction;

            SavedListsPerScene localListOfSceneObjectsToLoad = GameManager.instance.GetListForScene(gameObject.scene.buildIndex);
        
            if(localListOfSceneObjectsToLoad != null && placedManually)
                Destroy(gameObject);
        }
        // else
        //     GameManager.instance.SaveEvent += SaveFunction;

        AdjustCharacterPositionToTile();
    }

    void AdjustCharacterPositionToTile()
    {
        myRay = new Ray(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), new Vector3(0,-1,0));
        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 4, layerMaskTile))
        {
            transform.parent = hit.transform;
            StartCoroutine(AdjustNode(hit.transform));
            transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + offsetY, hit.transform.position.z);

            if(gameObject.tag == "PlayerCharacter")
            {
                CameraManager.instance.targetTargetPos = transform.position;
                CameraManager.instance.EnterSceneCameraUpdate();
                CameraManager.instance.UpdateStaticTransparencyBoundingBox();
            }
        }
    }

    IEnumerator AdjustNode(Transform t)
    {
        WaitUntil wait = new WaitUntil(() => t.GetComponent<TileScript>().node != null);
        yield return wait;
        t.GetComponent<TileScript>().node.occupied = true;
    }

    public void OnDestroy()
    {
        TurnManager.instance.TurnEvent -= TurnOrderAssignment;
        
        TileScript t = transform.parent.GetComponent<TileScript>();
        if(t != null)
            t.node.occupied = false;

        if (gameObject.tag == "Enemy")
            GameManager.instance.SaveEvent -= SaveFunction;
        else if(gameObject.tag == "PlayerCharacter")
        {
            GameManager.instance.SaveEvent -= SaveFunction;
            winAnnouncement.transform.gameObject.SetActive(true);
            winAnnouncement.text = "You died, that's it.";
            GameManager.instance.gameState = GameState.MainMenu;
        }
    }

    public void SaveFunction(object sender, EventArgs args)
    {
        //float tileID = transform.parent.GetComponent<TileScript>().tileID;
        int[] tileID = transform.parent.GetComponent<TileScript>().tileID;

        SavedCharacter savedCharacter = new SavedCharacter(tileID, transform.position, transform.rotation, currentHealth, 
                                                           maxHealth, attackStrength, defenseStrength, maxActionPoints, turnOrderRating, behaviour);

        GameManager.instance.GetListForScene().SavedCharacters.Add(savedCharacter);
    }

    public void TurnOrderAssignment(object sender, EventArgs args)
    {
        if(gameObject.activeSelf)
        {
            currentActionPoints = maxActionPoints;
            characterCanvas.SetActionPointsToMax(this);
            TurnManager.instance.rawTurnQueue.Add(this);
        }

        //Reset unit's last path unless it's the player's
        if(gameObject.tag == "PlayerCharacter" && UIManager.instance.tileSelector.activeInHierarchy == true && unitPath.Count > 1 &&
           Mathf.RoundToInt(UIManager.instance.tileSelector.transform.position.x) ==  Mathf.RoundToInt(unitPath[unitPath.Count-1].position.x) &&  Mathf.RoundToInt(UIManager.instance.tileSelector.transform.position.z) ==  Mathf.RoundToInt(unitPath[unitPath.Count-1].position.z))
            {
                TracePath(unitPath[unitPath.Count-1].position);
            }
        else
            unitPath = null;
    }

    void MoveTowardsDestination(Vector3 destination)
    {
    //     transform.position = Vector3.MoveTowards(transform.position, destination, movementSpeed*Time.deltaTime);
        
    //     Vector3 rayOrigin = transform.position;
    //     Vector3 rayDirection = new Vector3(0,-1,0);
        
    //     myRay = new Ray(rayOrigin, rayDirection);
        
    //     RaycastHit hit;

    //    if(Physics.Raycast(myRay, out hit, 2, layerMaskTile))
    //     {
    //         transform.position = new Vector3(transform.position.x, hit.point.y + offsetY, transform.position.z);
    //         moveDestination = new Vector3(moveDestination.x, transform.position.y, moveDestination.z);
    //     }
        t1 = t1 +  movementSpeed*Time.deltaTime;
        transform.position = Vector3.MoveTowards(initialPos, destination, t1);
        
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = new Vector3(0,-1,0);
        
        myRay = new Ray(rayOrigin, rayDirection);
        
        RaycastHit hit;

       if(Physics.Raycast(myRay, out hit, 2, layerMaskTile))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + offsetY, transform.position.z);
            moveDestination = new Vector3(moveDestination.x, transform.position.y, moveDestination.z);
        }
    }

    void SetRotationTowardsTarget(Vector3 target)
    {
        target = new Vector3(target.x, transform.position.y, target.z);
        Vector3 direction = (target - transform.position);
        targetRotation = Quaternion.LookRotation(direction, Vector3.up);
    }
    void LookTowards(Quaternion targetRotation)
    {
       t0 = t0 + Time.deltaTime * rotationSpeed;
    
       transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t0);
    }

    public IEnumerator Movement(bool startedWalking = false)
    {
        yield return m_movementCondition;
        
        if(t0 <= 1)
        {
            LookTowards(targetRotation);
        }
            
        else if (transform.position == moveDestination)
        {
            turnFinished();
            moving = false;

            if(gameObject.tag == "PlayerCharacter")
            {
                //Character proximity behaviour changes
                for(int i = 0; i < TurnManager.instance.rawTurnQueue.Count; ++i)
                {
                    if(TurnManager.instance.rawTurnQueue[i] != null && TurnManager.instance.rawTurnQueue[i].gameObject.tag == "Enemy")
                    {
                        float distance = Vector3.Distance(TurnManager.instance.rawTurnQueue[i].transform.position, transform.position);
                        if(distance < 2f
                           && TurnManager.instance.rawTurnQueue[i].behaviour == CharacterBehaviourType.proximity)
                        {
                            TurnManager.instance.rawTurnQueue[i].SetBehaviourTo(CharacterBehaviourType.aggressive);
                        }
                        else if(distance >= 10)
                        {
                            // Debug.Log("SKipAnimations");
                            TurnManager.instance.rawTurnQueue[i].skipAnimations = true;
                        }
                        else if(distance < 10)
                        {
                            TurnManager.instance.rawTurnQueue[i].skipAnimations = false;
                        }
                    }
                }
            }
            StopCoroutine(m_characterAnimation);
            m_characterAnimation = null;
            yield break;
        } 
        
        else
        {
            if(!startedWalking && !skipAnimations)
            {
                audioSource.clip = SoundLibrary.instance.doorOpen;
                audioSource.Play();
                startedWalking = true;
            }
            MoveTowardsDestination(moveDestination);
        }
            

        m_characterAnimation = StartCoroutine(Movement(startedWalking));
    }

    IEnumerator AttackAnimation(CharacterScript target, bool hitLanded = false)
    {   
        yield return null;

        if(t0 <= 1)
        {
            LookTowards(targetRotation);
        }

        else if(t1 < 0.8)
        {
            t1 = t1 + Time.deltaTime * attackSpeed;
            transform.position = Vector3.Lerp(initialPos, moveDestination, t1);
            t2 = t1;
        } 

        else
        {
            if(!hitLanded)
            {
                int damage = Mathf.Clamp((attackStrength - target.defenseStrength), 0, 999);
                target.currentHealth -= damage;
                target.Attacked(damage);
                hitLanded = true;
                audioSource.clip = SoundLibrary.instance.attackHit;
                audioSource.Play();

                if(target.currentHealth <= 0)
                {
                    // target.transform.SetParent(null);
                    // target.gameObject.SetActive(false);
                    Destroy(target.gameObject);
                }
            }
            
            t2 = t2 - Time.deltaTime * attackSpeed;
            transform.position = Vector3.Lerp(initialPos, moveDestination, t2);
            
            if(t2 <= 0)
            {
                t0 = 0;
                t1 = 0;
                animationActive = false;
                turnFinished();
                StopCoroutine(m_characterAnimation);
                m_characterAnimation = null;
                yield break;
            } 
        }

        m_characterAnimation = StartCoroutine(AttackAnimation(target, hitLanded));
    }

    IEnumerator AttemptToOpenLockedDoorWithoutKeyAnimation(Vector3 target, bool reachedDoor = false)
    {
        yield return null;

        if(t0 <= 1)
        {
            LookTowards(targetRotation);
        }

        else if(t1 < 0.8)
        {
            t1 = t1 + Time.deltaTime * attackSpeed;
            transform.position = Vector3.Lerp(initialPos, moveDestination, t1);
            t2 = t1;
        } 

        else
        {
            if(!reachedDoor && !skipAnimations)
            {
                audioSource.clip = SoundLibrary.instance.doorLocked;
                audioSource.Play();
                reachedDoor = true;
            }

            t2 = t2 - Time.deltaTime * attackSpeed;
            transform.position = Vector3.Lerp(initialPos, moveDestination, t2);
            
            if(t2 <= 0)
            {
                t0 = 0;
                t1 = 0;
                animationActive = false;
                turnFinished();
                StopCoroutine(m_characterAnimation);
                m_characterAnimation = null;
                yield break;
            } 
        }

        m_characterAnimation = StartCoroutine(AttemptToOpenLockedDoorWithoutKeyAnimation(target, reachedDoor));
    }

    bool AttemptMove(Vector3 target)
    {   
        /* if(target.x < transform.position.x-1.49 ||
           target.x > transform.position.x+1.49 ||
           target.z < transform.position.z-1.49 ||
           target.z > transform.position.z+1.49 ||
           (target.x > transform.position.x-0.49 &&
           target.x < transform.position.x+0.49 &&
           target.z > transform.position.z-0.49 &&
           target.z < transform.position.z+0.49) ||
           moving || animationActive)
        {
            return false;
        } */

        myRay = new Ray(new Vector3(target.x, target.y + 0.5f, target.z), new Vector3(0,-1,0));

        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 4, layerMaskTile))
        {
            return CheckForTileInteractions(hit.collider.transform);
        }

        return false;
    }

    public bool TracePath(Vector3 target)
    {
        myRay = new Ray(new Vector3(target.x, target.y + 0.5f, target.z), new Vector3(0,-1,0));

        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 4, layerMaskTile))
        {
            if(gameObject.tag == "PlayerCharacter")
            {
                // Debug.Log("StartNode X: " + Mathf.RoundToInt(gameObject.transform.position.x).ToString() + "StartNode z: " + Mathf.RoundToInt(gameObject.transform.position.z).ToString() + ", " + 
                //             "TargetNode X: " + Mathf.RoundToInt(hit.collider.transform.position.x).ToString() + "TargetNode z: " + Mathf.RoundToInt(hit.collider.transform.position.z).ToString());
            
                unitPath = PathfindingManager.instance.FindPath(GameManager.instance.currentSceneNodes[GameManager.instance.TileListIndexConversion(Mathf.RoundToInt(gameObject.transform.position.x)), GameManager.instance.TileListIndexConversion(Mathf.RoundToInt(gameObject.transform.position.z))],
                                                        GameManager.instance.currentSceneNodes[GameManager.instance.TileListIndexConversion(Mathf.RoundToInt(hit.collider.transform.position.x)), GameManager.instance.TileListIndexConversion(Mathf.RoundToInt(hit.collider.transform.position.z))], true, currentActionPoints);

                if (unitPath != null && unitPath.Count > 0)
                    return true;
                else
                    return false;

            }
            else
            {
                unitPath = PathfindingManager.instance.FindPath(GameManager.instance.currentSceneNodes[GameManager.instance.TileListIndexConversion(Mathf.RoundToInt(gameObject.transform.position.x)), GameManager.instance.TileListIndexConversion(Mathf.RoundToInt(gameObject.transform.position.z))],
                                                        GameManager.instance.currentSceneNodes[GameManager.instance.TileListIndexConversion(Mathf.RoundToInt(hit.collider.transform.position.x)), GameManager.instance.TileListIndexConversion(Mathf.RoundToInt(hit.collider.transform.position.z))], false);
                if (unitPath != null && unitPath.Count > 0)
                    return true;
                else
                    return false;
            }
        }
        else
            return false;
    }

    bool CheckForTileInteractions(Transform tile)
    {
        foreach(Transform t in tile)
        {
            if (t.tag == "Door")
            {
                if(hasKey)
                {
                    // t.SetParent(null);
                    // t.gameObject.SetActive(false);
                    audioSource.clip = SoundLibrary.instance.doorOpen;
                    audioSource.Play();
                    Destroy(t.gameObject);
                }
                else
                {
                    AttemptToOpenLockedDoorWithoutKey(t);
                    return false;
                }  
            }
            if (t.tag == "Enemy")
            {
                if(gameObject.tag == "PlayerCharacter")
                {
                    CharacterScript enemy = t.GetComponent<CharacterScript>();
                    Attack(enemy);
                    return false;
                }
                else if(t.tag == "Enemy")
                {
                    AttemptToOpenLockedDoorWithoutKey(t);
                    return false;
                }
            }
            if (t.tag == "PlayerCharacter" && gameObject.tag == "Enemy")
            {
                CharacterScript enemy = t.GetComponent<CharacterScript>();
                Attack(enemy);
                return false;
            }
            if(t.tag == "StairUp" && gameObject.tag == "PlayerCharacter")
            {
                moving = true;
                StartMoveToPreviousScene();
            }
            if(t.tag == "StairDown" && gameObject.tag == "PlayerCharacter")
            {
                moving = true;
                StartMoveToNextScene();
            }
        }
        return true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(gameObject.tag != "PlayerCharacter")
            return;

        if(collision.gameObject.tag == "Key")
        {
            // collision.transform.SetParent(null);
            // collision.gameObject.SetActive(false);
            hasKey = true;
            audioSource.clip = SoundLibrary.instance.keyPickUp;
            audioSource.Play();
            Destroy(collision.gameObject);
        }
        else if(collision.gameObject.tag == "Coin")
        {
            // collision.transform.SetParent(null);
            // collision.gameObject.SetActive(false);
            coinCount += 1;
            coinCounter.text = coinCount.ToString();
            audioSource.clip = SoundLibrary.instance.coinPickUp;
            audioSource.Play();
            Destroy(collision.gameObject);

            if (coinCount >= coinsTilWin)
            {
                winAnnouncement.transform.gameObject.SetActive(true);
                winAnnouncement.text = "You won, I guess.";
            }
        }
    }

    // public void MoveForward()
    // {
    //     Vector3 target = (transform.position+transform.forward); 
        
    //     if(AttemptMove(target))
    //     {
    //         t0 = 1;
    //         initialRotation = transform.rotation;
    //         moveDestination = target;
    //         SetRotationTowardsTarget(target);
    //         moving = true;
    //         m_characterAnimation = StartCoroutine(Movement());

    //         if (gameObject.tag == "PlayerCharacter")
    //         {
    //             CameraManager.instance.targetTargetPos = target;
    //             CameraManager.instance.FollowTarget();
    //         }
    //     }
    // }

    // public void MoveBackwards()
    // {
    //     Vector3 target = (transform.position-transform.forward); 
        
    //     if(AttemptMove(target))
    //     {
    //         t0 = 0;
    //         rotationSpeed = 1 / rotation90dSec;
    //         initialRotation = transform.rotation;
    //         moveDestination = target;
    //         SetRotationTowardsTarget(target);
    //         moving = true;
    //         m_characterAnimation = StartCoroutine(Movement());

    //         if (gameObject.tag == "PlayerCharacter")
    //         {
    //             CameraManager.instance.targetTargetPos = target;
    //             CameraManager.instance.FollowTarget();
    //         }
    //     }
    // }

    // public void MoveRight()
    // {
    //     Vector3 target = (transform.position+transform.right); 

    //     if(AttemptMove(target))
    //     {
    //         t0 = 0;
    //         rotationSpeed = 2 / rotation90dSec;
    //         initialRotation = transform.rotation;
    //         moveDestination = target;
    //         SetRotationTowardsTarget(target);
    //         moving = true;
    //         m_characterAnimation = StartCoroutine(Movement());

    //         if (gameObject.tag == "PlayerCharacter")
    //         {
    //             CameraManager.instance.targetTargetPos = target;
    //             CameraManager.instance.FollowTarget();
    //         }
    //     } 
    // }

    // public void MoveLeft()
    // {
    //     Vector3 target = (transform.position-transform.right); 

    //     if(AttemptMove(target))
    //     {
    //         t0 = 0;
    //         rotationSpeed = 2 / rotation90dSec;
    //         initialRotation = transform.rotation;
    //         moveDestination = target;
    //         SetRotationTowardsTarget(target);
    //         moving = true;
    //         m_characterAnimation = StartCoroutine(Movement());

            
    //         if (gameObject.tag == "PlayerCharacter")
    //         {
    //             CameraManager.instance.targetTargetPos = target;
    //             CameraManager.instance.FollowTarget();
    //         }
    //     } 
    // }

    public void StartFollowingPath()
    {
        m_followingPath = StartCoroutine(FollowingPath());
    }

    IEnumerator FollowingPath()
    {
        yield return m_followPathCondition;

        if(unitPath == null)
            yield break;

        if(unitPath.Count > 1)
        {
            if(turnFinished())
                yield break;
            
            MoveToNextLocationOnPath();
            m_followingPath = StartCoroutine(FollowingPath());
        } 
    }

    public void MoveToNextLocationOnPath()
    {   
        if(unitPath == null || unitPath.Count < 2)
        {
            if (gameObject.tag == "Enemy")
            {
                turnActive = false;
            }
            return;
        }
            

        // if (gameObject.tag == "Enemy")
        // {
        //     AddActionPoint();
        // }

        if(turnFinished())
        {
            return;   
        }
        
        Vector3 target = new Vector3(unitPath[1].position.x, unitPath[1].position.y+offsetY, unitPath[1].position.z);

        if(target == transform.position)
        {
            return;
        }

        if(AttemptMove(target))
        {
            if (gameObject.tag == "Enemy")
            {
                RemoveActionPoint();
            }

            myRay = new Ray(new Vector3(target.x, target.y + 1f, target.z), new Vector3(0,-1,0));
            RaycastHit hit;

            if(Physics.Raycast(myRay, out hit, 4, layerMaskTile))
            {
                TileScript tile = transform.parent.GetComponent<TileScript>();
                tile.node.occupied = false;

                transform.parent = hit.transform;
                tile = hit.transform.GetComponent<TileScript>();
                tile.node.occupied = true;
            }

            if(skipAnimations)
            {
                t0 = 1;
                t1 = 1;
            }
            else
            {   
                t0 = 0;
                t1 = 0;
            }

            rotationSpeed = 2 / rotation90dSec;
            initialPos = transform.position;
            initialRotation = transform.rotation;
            moveDestination = target;
            SetRotationTowardsTarget(target);
            moving = true;
            m_characterAnimation = StartCoroutine(Movement());

            unitPath.RemoveAt(0);

            if (gameObject.tag == "PlayerCharacter")
            {
                RemoveActionPoint();
                CameraManager.instance.targetTargetPos = target;
                CameraManager.instance.FollowTarget();
                PathfindingManager.instance.DrawPath(unitPath, currentActionPoints);
            }
        }
    }

    void AddActionPoint()
    {
        ++currentActionPoints;
        characterCanvas.AddActionPoints(1);
        // Debug.Log("Action Point added. Current points: " + currentActionPoints.ToString());
    }

    void RemoveActionPoint()
    {
        --currentActionPoints;
        characterCanvas.RemoveActionPoints(1);
        // Debug.Log("Action Point removed. Current points: " + currentActionPoints.ToString());
    }

    public void Attack(CharacterScript target)
    {
        if(gameObject.tag == "PlayerCharacter")
        {
            if(turnFinished())
            {
                return;   
            }
        }
        
        RemoveActionPoint();
        Vector3 targetPosition = target.transform.position;

        if(skipAnimations)
        {
            t0 = 1;
            t1 = 1;
            t2 = 0;
        }
        else
        {
            t0 = 0;
            t1 = 0;
            t2 = 0;
        }
        
        initialPos = transform.position;
        rotationSpeed = 2 / rotation90dSec;
        initialRotation = transform.rotation;
        moveDestination = targetPosition;
        SetRotationTowardsTarget(targetPosition);
        animationActive = true;

        if(target.behaviour == CharacterBehaviourType.passive)
            target.SetBehaviourTo(CharacterBehaviourType.aggressive);

        m_characterAnimation = StartCoroutine(AttackAnimation(target));
    }

    public void AttemptToOpenLockedDoorWithoutKey(Transform target)
    {
        if(gameObject.tag == "Enemy")
        {
            RemoveActionPoint();
        }
        
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        
        if(skipAnimations)
        {
            t0 = 1;
            t1 = 1;
            t2 = 0;
        }
        else
        {
            t0 = 0;
            t1 = 0;
            t2 = 0;
        }
        
        initialPos = transform.position;
        rotationSpeed = 2 / rotation90dSec;
        initialRotation = transform.rotation;
        moveDestination = targetPosition;
        SetRotationTowardsTarget(targetPosition);
        animationActive = true;
        m_characterAnimation = StartCoroutine(AttemptToOpenLockedDoorWithoutKeyAnimation(targetPosition));
    }

    public void Attacked(int damage)
    {
        characterCanvas.Attacked(maxHealth, currentHealth, damage);
    }

    public void SetHealthbarFill()
    {
        characterCanvas.SetHealthbarFill(maxHealth, currentHealth);
    }

    public void SetCurrentHealth(int health)
    {
        if(health > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = health;
        }

        SetHealthbarFill();
    }

    IEnumerator MoveToPreviousScene()
    {
        WaitUntil moveUp = new WaitUntil(() => moving == false);

        yield return moveUp;
        
        transform.parent = GameObject.FindGameObjectWithTag("MainSceneAnchor").transform;

        GameManager.instance.MoveToPreviousScene();

        m_characterAnimation = StartCoroutine(EnterScene(false));
    }
    IEnumerator MoveToNextScene()
    {
        WaitUntil moveDown = new WaitUntil(() => moving == false);
        
        yield return moveDown;               
        
        transform.parent = GameObject.FindGameObjectWithTag("MainSceneAnchor").transform;

        GameManager.instance.MoveToNextScene();  

        m_characterAnimation = StartCoroutine(EnterScene(true));
    }

    IEnumerator EnterScene(bool next)
    {
        yield return GameManager.instance.m_setSceneActiveCondition;
        Transform target;

        if(next)
        {
            target = GameObject.FindGameObjectWithTag("StairUp").transform.parent.transform;
            transform.rotation = target.rotation;
        }
        else
        {
            target = GameObject.FindGameObjectWithTag("StairDown").transform.parent.transform;
            transform.rotation = Quaternion.Euler(target.rotation.x, target.rotation.y + 180f, target.rotation.z);
        }

        transform.position = target.position;
        transform.position = transform.position + transform.forward;
        // CameraManager.instance.targetTargetPos = transform.position;
        // CameraManager.instance.tUpdatePos = 1;

        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y+1, transform.position.z);
        Vector3 rayDirection = new Vector3(0,-1,0);
        
        myRay = new Ray(rayOrigin, rayDirection);
        
        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 2, layerMaskTile))
        {
            transform.parent = hit.transform;
            TileScript tile = hit.transform.GetComponent<TileScript>();
            tile.node.occupied = true;
            transform.position = new Vector3(transform.position.x, hit.point.y + offsetY, transform.position.z);
        }

        CameraManager.instance.targetTargetPos = transform.position;
        CameraManager.instance.EnterSceneCameraUpdate();
        CameraManager.instance.UpdateStaticTransparencyBoundingBox();
        m_characterAnimation = null;
    }

    public void StartMoveToPreviousScene()
    {
        m_characterAnimation = StartCoroutine(MoveToPreviousScene());
    }
    public void StartMoveToNextScene()
    {
        m_characterAnimation = StartCoroutine(MoveToNextScene());
    }

    public void StartTurn()
    {
        if(gameObject.tag == "PlayerCharacter")
            m_characterTurnCondition = new WaitUntil(() => turnActive == false);
        else if (gameObject.tag == "Enemy")
            m_characterTurnCondition = new WaitUntil(() => !animationActive && !moving);
        m_characterTurn = StartCoroutine(TakeTurn());
    }

    IEnumerator TakeTurn()
    {
        if (gameObject.tag == "Enemy")
        {
            switch(behaviour)
            {
                case CharacterBehaviourType.aggressive:
                {
                    if(!animationActive && !moving && turnActive)
                    {
                        var player = GameObject.FindGameObjectWithTag("PlayerCharacter");

                        if(player == null)
                        {
                            Debug.Log("Player can not be found. This should not happen");
                            turnActive = false;
                        }

                        if(unitPath == null)
                            TracePath(player.transform.position);
                        
                        MoveToNextLocationOnPath();
                    }
            
                    yield return m_characterTurnCondition;
                    if(turnActive)
                        m_characterTurn = StartCoroutine(TakeTurn());
                    yield break;
                }
                case CharacterBehaviourType.passive:
                {
                    turnActive = false;
                    yield break;
                }
                case CharacterBehaviourType.proximity:
                {
                    turnActive = false;
                    yield break;
                }
            }
            
        }
        else
        {
            //Player behaviour goes here
            yield return m_characterTurnCondition;
        }
    }

    bool turnFinished()
    {
        if(currentActionPoints < 1)
        {
            turnActive = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetBehaviourTo(CharacterBehaviourType type)
    {
        switch(type)
        {
            case CharacterBehaviourType.aggressive:
            {
                if(behaviour == CharacterBehaviourType.passive)
                    turnOrderRating = -1;

                characterCanvas.statusAggressive.gameObject.SetActive(true);
                characterCanvas.statusPassive.gameObject.SetActive(false);
                characterCanvas.statusProximity.gameObject.SetActive(false);
                break;
            }
            case CharacterBehaviourType.passive:
            {
                characterCanvas.statusAggressive.gameObject.SetActive(false);
                characterCanvas.statusPassive.gameObject.SetActive(true);
                characterCanvas.statusProximity.gameObject.SetActive(false);
                break;
            }
            case CharacterBehaviourType.proximity:
            {
                characterCanvas.statusAggressive.gameObject.SetActive(false);
                characterCanvas.statusPassive.gameObject.SetActive(false);
                characterCanvas.statusProximity.gameObject.SetActive(true);
                break;
            }
        }
        
        behaviour = type;
    }
}
