using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    //Doors blocking without key, fix blocking issue, key, coins, display points
    
    Vector3 currentDirection;
    Vector3 moveDestination, nextPos;
    Quaternion targetRotation;

    public float movementSpeed = 2f;
    public float rotationSpeed = 5f;

    bool move = false;


    void Start()
    {
        currentDirection = transform.position;
        targetRotation = transform.rotation;
    }
    Ray myRay;
    void Update()
    {
        CharacterInput();
        Movement();
    
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

    }

    void MoveTowardsDestination(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, movementSpeed*Time.deltaTime);
    }

    void SetRotationTowardsTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(direction);
    }
    void LookTowards(Quaternion rotation)
    {
       transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
    }

    public void Movement()
    {
        if(!move)
        {
            return;
        }
        
        if(transform.rotation != targetRotation)
        {
            LookTowards(targetRotation);
        }
            
        else if (transform.position != moveDestination)
        {
            MoveTowardsDestination(moveDestination);
        } 
        
        else
        move = false;
    }

    bool AttemptMove(Vector3 target)
    {
        myRay = new Ray(target, new Vector3(0,-1,0));
        RaycastHit hit;

        if(Physics.Raycast(myRay, out hit))
        {
            if(hit.collider.tag == "Tile")
            return true;
        }
        return false;
    }

    void CharacterInput()
    {
        if(move)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Vector3 target = transform.position+transform.forward; 
            
            if(AttemptMove(target))
            {
                moveDestination = target;
                SetRotationTowardsTarget(target);
                move = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Vector3 target = transform.position-transform.forward; 
            
            if(AttemptMove(target))
            {
                moveDestination = target;
                SetRotationTowardsTarget(target);
                move = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Vector3 target = transform.position+transform.right; 

            if(AttemptMove(target))
            {
                moveDestination = target;
                SetRotationTowardsTarget(target);
                move = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Vector3 target = transform.position-transform.right; 

            if(AttemptMove(target))
            {
                moveDestination = target;
                SetRotationTowardsTarget(target);
                move = true;
            }
        }
    }
}
