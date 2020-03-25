using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScript : MonoBehaviour
{
    //Fix camera, Input mouse, Mobile build, Input touch, Fix item pickup, Add enemy that block movement (no AI) and combat.
    
    Vector3 currentDirection;
    Vector3 moveDestination, initialPos, offsetY;
    Quaternion targetRotation;
    Quaternion initialRotation;
    int layerMaskTile;
    WaitUntil m_movementCondition;
    Coroutine m_movement, m_attackAnimation;
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
    bool move = false;
    bool attack = false;
    CharacterStats characterStats;

    void Start()
    {
        currentDirection = transform.position;
        targetRotation = transform.rotation;
        layerMaskTile = LayerMask.GetMask("Tiles");
        m_movementCondition = new WaitUntil(() => move == true && attack == false);
        m_movement = StartCoroutine(Movement());
        offsetY = new Vector3(0, transform.position.y, 0);
        characterStats = new CharacterStats(100, 50, 0);
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

    IEnumerator Movement()
    {
        yield return m_movementCondition;
        
        if(t0 < 1)
        {
            LookTowards(targetRotation);
            //Debug.Log("Trying to look from " + transform.rotation + " to " + targetRotation + " while t is " + t + " and the original rotation was " + initialRotation);
        }
            
        else if (transform.position != moveDestination)
        {
            MoveTowardsDestination(moveDestination);
        } 
        
        else
            move = false;

        m_movement = StartCoroutine(Movement());
    }

    IEnumerator AttackAnimation(EnemyScript enemy)
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
            Debug.Log("It should go back");
            t2 = Mathf.Clamp01(t2 - Time.deltaTime * attackSpeed);
            transform.position = Vector3.Lerp(initialPos, moveDestination, t2);
            
            if(t2 == 0)
            {
                if(enemy.characterStats.health <= 0)
                {
                    enemy.transform.SetParent(null);
                    enemy.gameObject.SetActive(false);
                }

                Debug.Log("It should end");
                t0 = 0;
                t1 = 0;
                attack = false;
                StopCoroutine(m_attackAnimation);
                m_attackAnimation = null;
                yield break;
            } 
        }

        m_attackAnimation = StartCoroutine(AttackAnimation(enemy));
    }

    bool AttemptMove(Vector3 target)
    {
        if(target.x < transform.position.x-1.49 ||
           target.x > transform.position.x+1.49 ||
           target.z < transform.position.z-1.49 ||
           target.z > transform.position.z+1.49)
        {
            return false;
        }

        myRay = new Ray(target, new Vector3(0,-1,0));
        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 2, layerMaskTile))
        {
            if(hit.collider.tag == "Tile")
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
                    Debug.Log("Locked door detected.");
                    return false;
                }  
            }
            if (t.tag == "Enemy")
            {
                Attack(t.gameObject);
                return false;
            }
        }
        return true;
    }

    void OnCollisionEnter(Collision collision)
    {
            Debug.Log("Collision");
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
            move = true;
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
            move = true;
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
            move = true;
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
            move = true;
        } 
    }

    public void MoveToLocation(Vector3 target)
    {
        target = target+offsetY;

        if(AttemptMove(target))
        {
            t0 = 0;
            rotationSpeed = 2 / rotation90dSec;
            initialRotation = transform.rotation;
            moveDestination = target;
            SetRotationTowardsTarget(target);
            move = true;
        } 
    }

    public void Attack(GameObject target)
    {
        EnemyScript enemy = target.GetComponent<EnemyScript>();
        Vector3 targetPosition = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        t0 = 0;
        t1 = 0;
        t2 = 0;
        initialPos = transform.position;
        rotationSpeed = 2 / rotation90dSec;
        initialRotation = transform.rotation;
        moveDestination = targetPosition;
        SetRotationTowardsTarget(targetPosition);
        attack = true;
        m_attackAnimation = StartCoroutine(AttackAnimation(enemy));
        int damage = characterStats.attack - enemy.characterStats.defense;
        enemy.characterStats.health -= damage;
    }
}
