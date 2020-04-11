using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Transform target;
    public Vector3 initialPos, targetPos, rotationTarget, velocity;
    Vector3[] offset = new Vector3[4];
    WaitUntil m_cameraMovementCondition;
    Coroutine m_cameraMovement;
    int offsetIndex = 0;
    public float zoomDistanceTotal, zoomDistanceFrame;
    public float rotateDegSec = 1f;
    public float moveUnitSec = 16f;
    public float tFollow = 2;
    public float tRotate = 2;
    public float tZoom = 2;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        initialPos = transform.position;
        offset[0] = transform.position - target.position;
        offset[1] = new Vector3(-offset[0].x, offset[0].y, offset[0].z);
        offset[2] = new Vector3(-offset[0].x, offset[0].y, -offset[0].z);
        offset[3] = new Vector3(offset[0].x, offset[0].y, -offset[0].z);
        targetPos = target.position + offset[offsetIndex];
        transform.LookAt(target);
        velocity = Vector3.zero;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(transform.position != target.position + offset[offsetIndex])
        {
            initialPos = transform.position;
            targetPos = target.position + offset[offsetIndex];
            FollowTarget(targetPos);
        }

        if(tRotate <= 1)
        {
            RotateAround(target);
        }
            
        if((zoomDistanceTotal > 0 && transform.position.y > 1.19) || (zoomDistanceTotal < 0 && transform.position.y < 3.5))
        {
            zoomDistanceFrame = Mathf.Clamp(Mathf.Ceil((zoomDistanceTotal * Time.deltaTime * 4)*100)/100, -0.7f, 0.7f);
            
            Vector3 originalPos = transform.position;

            
            Debug.Log("Original Offset[0] = " + offset[0]);
            transform.position = target.position + offset[0];
            transform.LookAt(target);
            transform.position = transform.position + (transform.forward * zoomDistanceFrame);
            offset[0] = transform.position - target.position;
            offset[1] = new Vector3(-offset[0].x, offset[0].y, offset[0].z);
            offset[2] = new Vector3(-offset[0].x, offset[0].y, -offset[0].z);
            offset[3] = new Vector3(offset[0].x, offset[0].y, -offset[0].z);
            Debug.Log("New Offset[0] = " + offset[0]);
            
            transform.position = originalPos;
            transform.LookAt(target);
            transform.position = transform.position + (transform.forward * zoomDistanceFrame);

            zoomDistanceTotal -= zoomDistanceFrame;
        }
    }

    void FollowTarget(Vector3 target)
    {
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, 0.3f);
        // t0 += Time.deltaTime * moveUnitSec;
        // transform.position = Vector3.Lerp(initialPos, target, t0);
    }

    void RotateAround(Transform target)
    {
        tRotate += Time.deltaTime * rotateDegSec;
        transform.position = Vector3.Lerp(initialPos, targetPos, tRotate);
        transform.LookAt(target);
    }

    public void RotateClockwise()
    {
        tRotate = 0;

        initialPos = transform.position;
        
        if(offsetIndex == 3)
            offsetIndex = 0;
        else
            offsetIndex += 1;
    }

    public void RotateCounterClockwise()
    {
        tRotate = 0;

        initialPos = transform.position;
        
        if(offsetIndex == 0)
            offsetIndex = 3;
        else
            offsetIndex -= 1;
    }

    public void Zoom(float magnitude)
    {
        if(magnitude < 0 && zoomDistanceTotal > 0)
            zoomDistanceTotal = 0;
        else if(magnitude > 0 && zoomDistanceTotal < 0)
            zoomDistanceTotal = 0;
        
        zoomDistanceTotal = Mathf.Clamp(zoomDistanceTotal + magnitude, -1, 1);
    }
}
