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
    WaitUntil m_movementCondition, m_characterTurnCondition;
    public Coroutine m_characterAnimation;
    Coroutine m_characterTurn;
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
    bool animationActive = false;
    public int maxHealth;
    public int currentHealth;
    public int attackStrength;
    public int defenseStrength;
    public bool dontReload = false;
    public CharacterCanvasController characterCanvas;
    public Animator characterAnimator;
    public int maxActionPoints = 1;
    public int currentActionPoints; 
    public int turnOrderRating = 1; 
    public bool turnActive = false;

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
        m_movementCondition = new WaitUntil(() => moving == true && animationActive == false);
        currentActionPoints = maxActionPoints;
        TurnManager.instance.TurnEvent += TurnOrderAssignment;

        SetHealthbarFill();
        
        if (gameObject.tag == "Enemy")
        {
            GameManager.instance.SaveEvent += SaveFunction;

            SavedListsPerScene localListOfSceneObjectsToLoad = GameManager.instance.GetListForScene(gameObject.scene.buildIndex);
        
            if(localListOfSceneObjectsToLoad != null && dontReload)
                Destroy(gameObject);
        }

        myRay = new Ray(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), new Vector3(0,-1,0));
        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 4, layerMaskTile))
        {
            transform.parent = hit.transform;
            transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + offsetY, hit.transform.position.z);
        }
    }

    public void OnDestroy()
    {
        TurnManager.instance.TurnEvent -= TurnOrderAssignment;

        if (gameObject.tag == "Enemy")
            GameManager.instance.SaveEvent -= SaveFunction;
    }

    public void SaveFunction(object sender, EventArgs args)
    {
        float tileID = transform.parent.GetComponent<TileScript>().tileID;

        SavedCharacter savedCharacter = new SavedCharacter(tileID, transform.position, transform.rotation, currentHealth, 
                                                           maxHealth, attackStrength, defenseStrength, maxActionPoints, turnOrderRating);

        GameManager.instance.GetListForScene().SavedCharacters.Add(savedCharacter);
    }

    public void TurnOrderAssignment(object sender, EventArgs args)
    {
        if(gameObject.activeSelf)
        {
            currentActionPoints = maxActionPoints;
            TurnManager.instance.rawTurnQueue.Add(this);
        }
    }

    void MoveTowardsDestination(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, movementSpeed*Time.deltaTime);
        
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
       t0 = Mathf.Clamp01(t0 + Time.deltaTime * rotationSpeed);
    
       transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t0);
    }

    public IEnumerator Movement()
    {
        yield return m_movementCondition;
        
        if(t0 < 1)
        {
            LookTowards(targetRotation);
        }
            
        else if (transform.position == moveDestination)
        {
            turnFinishedCheck();
            moving = false;
            StopCoroutine(m_characterAnimation);
            m_characterAnimation = null;
            yield break;
        } 
        
        else
        {
            MoveTowardsDestination(moveDestination);
        }
            

        m_characterAnimation = StartCoroutine(Movement());
    }

    IEnumerator AttackAnimation(CharacterScript target, bool hitLanded = false)
    {   
        yield return null;

        if(t0 < 1)
        {
            LookTowards(targetRotation);
        }

        else if(t1 < 0.8)
        {
            t1 = Mathf.Clamp01(t1 + Time.deltaTime * attackSpeed);
            transform.position = Vector3.Lerp(initialPos, moveDestination, t1);
            t2 = t1;
        } 

        else
        {
            if(!hitLanded)
            {
                int damage = attackStrength - target.defenseStrength;
                target.currentHealth -= damage;
                target.Attacked(damage);
                hitLanded = true;

                if(target.currentHealth <= 0)
                {
                    // target.transform.SetParent(null);
                    // target.gameObject.SetActive(false);
                    Destroy(target.gameObject);
                }
            }
            
            t2 = Mathf.Clamp01(t2 - Time.deltaTime * attackSpeed);
            transform.position = Vector3.Lerp(initialPos, moveDestination, t2);
            
            if(t2 == 0)
            {
                t0 = 0;
                t1 = 0;
                animationActive = false;
                turnFinishedCheck();
                StopCoroutine(m_characterAnimation);
                m_characterAnimation = null;
                yield break;
            } 
        }

        m_characterAnimation = StartCoroutine(AttackAnimation(target, hitLanded));
    }

    IEnumerator AttemptToOpenLockedDoorWithoutKeyAnimation(Vector3 target)
    {
        yield return null;

        if(t0 < 1)
        {
            LookTowards(targetRotation);
        }

        else if(t1 < 0.8)
        {
            t1 = Mathf.Clamp01(t1 + Time.deltaTime * attackSpeed);
            transform.position = Vector3.Lerp(initialPos, moveDestination, t1);
            t2 = t1;
        } 

        else
        {
            t2 = Mathf.Clamp01(t2 - Time.deltaTime * attackSpeed);
            transform.position = Vector3.Lerp(initialPos, moveDestination, t2);
            
            if(t2 == 0)
            {
                t0 = 0;
                t1 = 0;
                animationActive = false;
                turnFinishedCheck();
                StopCoroutine(m_characterAnimation);
                m_characterAnimation = null;
                yield break;
            } 
        }

        m_characterAnimation = StartCoroutine(AttemptToOpenLockedDoorWithoutKeyAnimation(target));
    }

    bool AttemptMove(Vector3 target)
    {
        if(target.x < transform.position.x-1.49 ||
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
        }

        myRay = new Ray(new Vector3(target.x, target.y + 0.5f, target.z), new Vector3(0,-1,0));

        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 4, layerMaskTile))
        {
            return CheckForTileInteractions(hit.collider.transform);
        }

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
                    Destroy(t.gameObject);
                }
                else
                {
                    AttemptToOpenLockedDoorWithoutKey(t);
                    return false;
                }  
            }
            if (t.tag == "Enemy" && gameObject.tag == "PlayerCharacter")
            {
                CharacterScript enemy = t.GetComponent<CharacterScript>();
                Attack(enemy);
                return false;
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
                //Debug.Log("Down");
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
            Destroy(collision.gameObject);
        }
        else if(collision.gameObject.tag == "Coin")
        {
            // collision.transform.SetParent(null);
            // collision.gameObject.SetActive(false);
            coinCount += 1;
            coinCounter.text = coinCount.ToString();
            Destroy(collision.gameObject);

            if (coinCount >= coinsTilWin)
            {
                winAnnouncement.transform.gameObject.SetActive(true);
            }
        }
    }

    public void MoveForward()
    {
        Vector3 target = (transform.position+transform.forward); 
        
        if(AttemptMove(target))
        {
            t0 = 1;
            initialRotation = transform.rotation;
            moveDestination = target;
            SetRotationTowardsTarget(target);
            moving = true;
            m_characterAnimation = StartCoroutine(Movement());

            if (gameObject.tag == "PlayerCharacter")
            {
                CameraManager.instance.targetTargetPos = target;
                CameraManager.instance.FollowTarget();
            }
        }
    }

    public void MoveBackwards()
    {
        Vector3 target = (transform.position-transform.forward); 
        
        if(AttemptMove(target))
        {
            t0 = 0;
            rotationSpeed = 1 / rotation90dSec;
            initialRotation = transform.rotation;
            moveDestination = target;
            SetRotationTowardsTarget(target);
            moving = true;
            m_characterAnimation = StartCoroutine(Movement());

            if (gameObject.tag == "PlayerCharacter")
            {
                CameraManager.instance.targetTargetPos = target;
                CameraManager.instance.FollowTarget();
            }
        }
    }

    public void MoveRight()
    {
        Vector3 target = (transform.position+transform.right); 

        if(AttemptMove(target))
        {
            t0 = 0;
            rotationSpeed = 2 / rotation90dSec;
            initialRotation = transform.rotation;
            moveDestination = target;
            SetRotationTowardsTarget(target);
            moving = true;
            m_characterAnimation = StartCoroutine(Movement());

            if (gameObject.tag == "PlayerCharacter")
            {
                CameraManager.instance.targetTargetPos = target;
                CameraManager.instance.FollowTarget();
            }
        } 
    }

    public void MoveLeft()
    {
        Vector3 target = (transform.position-transform.right); 

        if(AttemptMove(target))
        {
            t0 = 0;
            rotationSpeed = 2 / rotation90dSec;
            initialRotation = transform.rotation;
            moveDestination = target;
            SetRotationTowardsTarget(target);
            moving = true;
            m_characterAnimation = StartCoroutine(Movement());

            
            if (gameObject.tag == "PlayerCharacter")
            {
                CameraManager.instance.targetTargetPos = target;
                CameraManager.instance.FollowTarget();
            }
        } 
    }

    public void MoveToLocation(Vector3 target)
    {   
        if (gameObject.tag == "Enemy")
        {
            ++currentActionPoints;
        }

        if(turnFinishedCheck())
        {
            return;   
        }
        
        target = new Vector3(target.x, target.y+offsetY, target.z);

        if(target == transform.position)
        {
            return;
        }

        if(AttemptMove(target))
        {
            if (gameObject.tag == "Enemy")
            {
                --currentActionPoints;
            }

            myRay = new Ray(new Vector3(target.x, target.y + 1f, target.z), new Vector3(0,-1,0));
            RaycastHit hit;

            if(Physics.Raycast(myRay, out hit, 4, layerMaskTile))
            {
                transform.parent = hit.transform;
            }

            t0 = 0;
            rotationSpeed = 2 / rotation90dSec;
            initialRotation = transform.rotation;
            moveDestination = target;
            SetRotationTowardsTarget(target);
            moving = true;
            m_characterAnimation = StartCoroutine(Movement());

            if (gameObject.tag == "PlayerCharacter")
            {
                --currentActionPoints;
                CameraManager.instance.targetTargetPos = target;
                CameraManager.instance.FollowTarget();
            }
        }
    }

    

    public void Attack(CharacterScript target)
    {
        if(gameObject.tag == "PlayerCharacter")
        {
            if(turnFinishedCheck())
            {
                return;   
            }
        }
        
        --currentActionPoints;
        Vector3 targetPosition = target.transform.position;
        t0 = 0;
        t1 = 0;
        t2 = 0;
        initialPos = transform.position;
        rotationSpeed = 2 / rotation90dSec;
        initialRotation = transform.rotation;
        moveDestination = targetPosition;
        SetRotationTowardsTarget(targetPosition);
        animationActive = true;
        m_characterAnimation = StartCoroutine(AttackAnimation(target));
    }

    public void AttemptToOpenLockedDoorWithoutKey(Transform target)
    {
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        t0 = 0;
        t1 = 0;
        t2 = 0;
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
        
        GameManager.instance.MoveToPreviousScene();

        m_characterAnimation = StartCoroutine(EnterScene(false));
    }
    IEnumerator MoveToNextScene()
    {
        WaitUntil moveDown = new WaitUntil(() => moving == false);
        
        yield return moveDown;               
        
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
        CameraManager.instance.targetTargetPos = transform.position;
        CameraManager.instance.tUpdatePos = 1;

        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y+1, transform.position.z);
        Vector3 rayDirection = new Vector3(0,-1,0);
        
        myRay = new Ray(rayOrigin, rayDirection);
        
        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 2, layerMaskTile))
        {
            transform.parent = hit.transform;
            transform.position = new Vector3(transform.position.x, hit.point.y + offsetY, transform.position.z);
        }

        CameraManager.instance.UpdateStaticTransparencyBoundingBox();
        m_characterAnimation = null;
    }

    public void StartMoveToPreviousScene()
    {
        transform.parent = transform;
        m_characterAnimation = StartCoroutine(MoveToPreviousScene());
    }
    public void StartMoveToNextScene()
    {
        transform.parent = transform;
        m_characterAnimation = StartCoroutine(MoveToNextScene());
    }

    public void AttemptMoveTowardsLocation(Vector3 target)
    {
        if(turnFinishedCheck())
            return;

        --currentActionPoints;

        Quaternion originalRot = transform.rotation;
        Vector3 originalPos = transform.position;

        Vector3 faceTowards = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(faceTowards);
        target = transform.position + transform.forward;
        faceTowards = transform.position + transform.forward * 1.5f;

        Debug.Log(target);

        transform.position = originalPos;
        transform.rotation = originalRot;

        myRay = new Ray(new Vector3(target.x, target.y + 1f, target.z), new Vector3(0,-1,0));
        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 4, layerMaskTile))
        {
            MoveToLocation(hit.transform.position);
        }
        else
        {
            myRay = new Ray(new Vector3(faceTowards.x, faceTowards.y + 1f, faceTowards.z), new Vector3(0,-1,0));
            
            if(Physics.Raycast(myRay, out hit, 4, layerMaskTile))
            {
                MoveToLocation(hit.transform.position);
            }
            else
                turnFinishedCheck();
        }
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
            //AI behaviour goes here
            if(!animationActive && !moving && turnActive)
                AttemptMoveTowardsLocation(GameObject.FindGameObjectWithTag("PlayerCharacter").transform.position);
            
            yield return m_characterTurnCondition;
            if(turnActive)
                 m_characterTurn = StartCoroutine(TakeTurn());
        }
        else
        {
            //Player behaviour goes here
            yield return m_characterTurnCondition;
        }
    }

    bool turnFinishedCheck()
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
}
