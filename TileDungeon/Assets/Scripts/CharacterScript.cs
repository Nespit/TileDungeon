using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScript : MonoBehaviour
{
    //underastand difference between abstract interfaces and inheritance from classes, multiple static levels, lateral movement, second enemy type 
    
    Vector3 currentDirection;
    Vector3 moveDestination, initialPos, offsetY;
    Quaternion targetRotation;
    Quaternion initialRotation;
    int layerMaskTile;
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
    bool moving = false;
    bool attacking = false;
    public int maxHealth;
    int currentHealth;
    public int attackStrength;
    public int defenseStrength;
    CharacterCanvasController characterCanvas;


    void Start()
    {
        currentDirection = transform.position;
        targetRotation = transform.rotation;
        layerMaskTile = LayerMask.GetMask("Tiles");
        m_movementCondition = new WaitUntil(() => moving == true && attacking == false);
        offsetY = new Vector3(0, transform.position.y, 0);
        currentHealth = maxHealth;
        characterCanvas = GetComponentInChildren<CharacterCanvasController>();
        SetHealthbarFill();
    }

    void MoveTowardsDestination(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, movementSpeed*Time.deltaTime);
    }

    void SetRotationTowardsTarget(Vector3 target)
    {
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
                    target.transform.SetParent(null);
                    target.gameObject.SetActive(false);
                }
            }
            
            t2 = Mathf.Clamp01(t2 - Time.deltaTime * attackSpeed);
            transform.position = Vector3.Lerp(initialPos, moveDestination, t2);
            
            if(t2 == 0)
            {
                t0 = 0;
                t1 = 0;
                attacking = false;
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
                attacking = false;
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
           moving || attacking)
        {
            return false;
        }

        myRay = new Ray(target, new Vector3(0,-1,0));
        RaycastHit hit;

       if(Physics.Raycast(myRay, out hit, 2, layerMaskTile))
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
                    t.SetParent(null);
                    t.gameObject.SetActive(false);
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
        }
        return true;
    }

    void OnCollisionEnter(Collision collision)
    {
            if(collision.gameObject.tag == "Key")
            {
                collision.transform.SetParent(null);
                collision.gameObject.SetActive(false);
                hasKey = true;
            }
            else if(collision.gameObject.tag == "Coin")
            {
                collision.transform.SetParent(null);
                collision.gameObject.SetActive(false);
                coinCount += 1;
                coinCounter.text = coinCount.ToString();

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
            SetRotationTowardsTarget(target);
            moving = true;
            m_characterAnimation = StartCoroutine(Movement());
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
        attacking = true;
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
        attacking = true;
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
}
