using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScript : MonoBehaviour
{
    //Fix camera, Input mouse, Mobile build, Input touch, Fix item pickup, Add enemy that block movement (no AI) and combat.
    
    Vector3 currentDirection;
    Vector3 moveDestination, nextPos, offsetY;
    Quaternion targetRotation;
    Quaternion initialRotation;
    int layerMaskTile;
    WaitUntil m_movementCondition;
    Coroutine m_movement;
    Ray myRay;
    public bool hasKey;
    int coinCount;
    public int coinsTilWin;
    public float movementSpeed = 2f;
    float rotationSpeed;
    float t = 0;
    public float rotation90dSec = 0.25f;
    public Text coinCounter;
    public Text winAnnouncement;
    bool move = false;

    void Start()
    {
        currentDirection = transform.position;
        targetRotation = transform.rotation;
        layerMaskTile = LayerMask.GetMask("Tiles");
        m_movementCondition = new WaitUntil(() => move == true);
        m_movement = StartCoroutine(Movement());
        offsetY = new Vector3(0, transform.position.y, 0);
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
       t = Mathf.Clamp01(t + Time.deltaTime * rotationSpeed);
    
       transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t);
    }

    IEnumerator Movement()
    {
        yield return m_movementCondition;
        
        if(t < 1)
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
            t = 1;
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
            t = 0;
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
            t = 0;
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
            t = 0;
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
            t = 0;
            rotationSpeed = 2 / rotation90dSec;
            initialRotation = transform.rotation;
            moveDestination = target;
            SetRotationTowardsTarget(target);
            move = true;
        } 
    }
}
