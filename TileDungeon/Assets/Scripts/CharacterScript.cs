using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScript : MonoBehaviour
{
    //Doors blocking without key, fix blocking issue, key, coins, display points
    
    Vector3 currentDirection;
    Vector3 moveDestination, nextPos;
    Quaternion targetRotation;
    Quaternion initialRotation;
    int layerMaskTile;
    WaitUntil m_movementCondition;
    Coroutine m_movement;
    float t = 0;
    Ray myRay;
    public bool hasKey;
    int coinCount;
    public int coinsTilWin;
    public float movementSpeed = 2f;
    public float rotationSpeed = 6f;
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
    }

    void Update()
    {
        CharacterInput();
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);
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
       t += Time.deltaTime;
    
       transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t * rotationSpeed);
    }

    IEnumerator Movement()
    {
        yield return m_movementCondition;
        
        if((t*rotationSpeed) < 1) //transform.rotation != targetRotation
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
        myRay = new Ray(target, new Vector3(0,-1,0));
        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit, 2, layerMaskTile))
        {
            if(hit.collider.tag == "Tile")
            return CheckForInteractions(hit.collider.transform);
        }
        return false;
    }

    bool CheckForInteractions(Transform tile)
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
            if(t.tag == "Key")
            {
                t.SetParent(null);
                t.gameObject.SetActive(false);
                hasKey = true;
            }
            else if(t.tag == "Coin")
            {
                t.SetParent(null);
                t.gameObject.SetActive(false);
                coinCount += 1;
                coinCounter.text = coinCount.ToString();

                if (coinCount >= coinsTilWin)
                {
                    winAnnouncement.transform.gameObject.SetActive(true);
                }
            }
        }
        return true;
    }

    void CharacterInput()
    {
        if(move)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Vector3 target = (transform.position+transform.forward); 
            
            if(AttemptMove(target))
            {
                t = 0;
                initialRotation = transform.rotation;
                moveDestination = target;
                SetRotationTowardsTarget(target);
                move = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Vector3 target = (transform.position-transform.forward); 
            
            if(AttemptMove(target))
            {
                t = 0;
                initialRotation = transform.rotation;
                moveDestination = target;
                SetRotationTowardsTarget(target);
                move = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Vector3 target = (transform.position+transform.right); 

            if(AttemptMove(target))
            {
                t = 0;
                initialRotation = transform.rotation;
                moveDestination = target;
                SetRotationTowardsTarget(target);
                move = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Vector3 target = (transform.position-transform.right); 

            if(AttemptMove(target))
            {
                t = 0;
                initialRotation = transform.rotation;
                moveDestination = target;
                SetRotationTowardsTarget(target);
                move = true;
            }
        }
    }
}
