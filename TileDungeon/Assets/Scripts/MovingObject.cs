using System.Collections;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    //underastand difference between abstract interfaces and inheritance from classes, multiple static levels, lateral movement, second enemy type 
    Vector3 moveDestination, offsetY;
    Quaternion targetRotation;
    Quaternion initialRotation;
    int layerMaskTile, layerMaskObject;
    WaitUntil m_movementCondition;
    public Coroutine m_characterAnimation;
    Ray myRay;
    public float movementSpeed = 2f;
    float rotationSpeed;
    float t0 = 0;
    public float rotation90dSec = 0.25f;
    public bool moving = false;
    bool animationActive = false;


    protected virtual void Start()
    {
        targetRotation = transform.rotation;
        layerMaskTile = LayerMask.GetMask("Tiles");
        layerMaskObject = LayerMask.GetMask("Objects");
        m_movementCondition = new WaitUntil(() => moving == true && animationActive == false);
        offsetY = new Vector3(0, transform.position.y, 0);
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
            return true;
        }

        return false;
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
}
