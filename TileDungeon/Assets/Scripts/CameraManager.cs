using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    Camera mainCamera;
    public Transform target;
    Vector3 initialPos, targetPos, offset, velocity;
    WaitUntil m_cameraMovementCondition;
    Coroutine m_cameraMovement;
    float movementSpeed;
    public float moveUnitSec = 1f;
    float t = 0;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        //m_cameraMovementCondition = new WaitUntil(() => );
        initialPos = transform.position;
        offset = transform.position - target.position;
        targetPos = target.position + offset;
        velocity = Vector3.zero;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(targetPos != target.position + offset)
        {
            initialPos = transform.position;
            targetPos = target.position + offset;
        }
            
        followTarget(targetPos);
    }

    void followTarget(Vector3 target)
    {
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, 0.3f);
    }
}
