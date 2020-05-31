using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CharacterScript : MonoBehaviour
{
    //underastand difference between abstract interfaces and inheritance from classes, multiple static levels, lateral movement, second enemy type 
    
    Vector3 currentDirection;
    Vector3 moveDestination, initialPos, offsetY;
    Quaternion targetRotation;
    Quaternion initialRotation;
    int layerMaskTile, layerMaskObject;
    WaitUntil m_movementCondition;
    public Coroutine m_characterAnimation;
    Ray myRay;
    public bool hasKey;
    int coinCount;
    public int coinsTilWin;
    public float movementSpeed = 2f;
    public float attackSpeed = 4f;
    float rotationSpeed;
    float t0 = 0;
    float t1 = 0;
    float t2 = 0;
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
    CharacterCanvasController characterCanvas;
    public Animator characterAnimator;


    void Awake()
    {
        characterCanvas = GetComponentInChildren<CharacterCanvasController>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        currentDirection = transform.position;
        targetRotation = transform.rotation;
        layerMaskTile = LayerMask.GetMask("Tiles");
        layerMaskObject = LayerMask.GetMask("Objects");
        m_movementCondition = new WaitUntil(() => moving == true && animationActive == false);
        offsetY = new Vector3(0, transform.position.y, 0);
        
        SetHealthbarFill();
        
        if (gameObject.tag == "Enemy")
        {
            GameManager.instance.SaveEvent += SaveFunction;

            SavedListsPerScene localListOfSceneObjectsToLoad = GameManager.instance.GetListForScene(gameObject.scene.buildIndex);
        
            if(localListOfSceneObjectsToLoad != null && dontReload)
                Destroy(gameObject);
        }
    }

    public void OnDestroy()
    {
        if (gameObject.tag != "PlayerCharacter)")
            GameManager.instance.SaveEvent -= SaveFunction;
    }

    public void SaveFunction(object sender, EventArgs args)
    {
        float tileID = transform.parent.GetComponent<TileScript>().tileID;

        SavedCharacter savedCharacter = new SavedCharacter(tileID, transform.position, transform.rotation, currentHealth);

        GameManager.instance.GetListForScene().SavedCharacters.Add(savedCharacter);
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
            transform.position = new Vector3(transform.position.x, hit.point.y + offsetY.y, transform.position.z);
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
            moving = false;
            t0 = 0;
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
            if (t.tag == "Enemy")
            {
                CharacterScript enemy = t.GetComponent<CharacterScript>();
                Attack(enemy);
                return false;
            }
            if(t.tag == "StairUp")
            {
                moving = true;
                StartMoveToPreviousScene();
            }
            if(t.tag == "StairDown")
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
            CameraManager.instance.targetTargetPos = target;
            SetRotationTowardsTarget(target);
            moving = true;
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
            CameraManager.instance.targetTargetPos = target;
            SetRotationTowardsTarget(target);
            moving = true;
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
            CameraManager.instance.targetTargetPos = target;
            SetRotationTowardsTarget(target);
            moving = true;
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
            CameraManager.instance.targetTargetPos = target;
            SetRotationTowardsTarget(target);
            moving = true;
        } 
    }

    public void MoveToLocation(Vector3 target)
    {
        target = target+offsetY;

        if(target == transform.position)
        {
            return;
        }

        if(AttemptMove(target))
        {
            t0 = 0;
            rotationSpeed = 2 / rotation90dSec;
            initialRotation = transform.rotation;
            moveDestination = target;
            CameraManager.instance.targetTargetPos = target;
            SetRotationTowardsTarget(target);
            moving = true;
            m_characterAnimation = StartCoroutine(Movement());

            CameraManager.instance.FollowTarget();
        } 
    }

    public void Attack(CharacterScript target)
    {
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
            transform.position = new Vector3(transform.position.x, hit.point.y + offsetY.y, transform.position.z);
        }

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
}
