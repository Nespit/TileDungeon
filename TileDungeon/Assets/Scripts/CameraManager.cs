using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public Camera mainCamera;
    public Transform target;
    public Vector3 initialPos, targetPos, rotationTarget, velocity;
    Vector3[] offset = new Vector3[4];
    WaitUntil m_cameraMovementCondition;
    Coroutine m_cameraMovement;
    int offsetIndex = 0;
    float zoomDistanceTarget, zoomDistanceFrame;
    float zoomSensitivity = 3;
    float zoomSpeed = 4;
    float zoomMin = -0.5f;
    float zoomMax = 0.5f;
    public float rotateDegSec = 1f;
    public float moveUnitSec = 16f;
    public float tFollow = 2;
    public float tRotate = 2;
    public float tZoom = 2;
    Bounds transparencyArea;

    void Awake()
	{
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);    
		}
		DontDestroyOnLoad(gameObject);
	}

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
            
        if((zoomDistanceTarget > 0 && transform.position.y > 1.19) || (zoomDistanceTarget < 0 && transform.position.y < 3.5))
        {
            zoomDistanceFrame = Mathf.Clamp(Mathf.Round(Mathf.Lerp(zoomDistanceFrame, zoomDistanceTarget, Time.deltaTime * zoomSpeed)*100)/100, zoomMin, zoomMax);
            zoomDistanceTarget -= zoomDistanceFrame;

            Vector3 originalPos = transform.position;
            Quaternion originalRot = transform.rotation;

            transform.position = target.position + offset[0];
            transform.LookAt(target);
            transform.position = transform.position + (transform.forward * zoomDistanceFrame);

            offset[0] = transform.position - target.position;
            offset[1] = new Vector3(-offset[0].x, offset[0].y, offset[0].z);
            offset[2] = new Vector3(-offset[0].x, offset[0].y, -offset[0].z);
            offset[3] = new Vector3(offset[0].x, offset[0].y, -offset[0].z);
            
            transform.position = originalPos;
            transform.rotation = originalRot;
            transform.position = transform.position + (transform.forward * zoomDistanceFrame);

            zoomDistanceFrame = 0;
        }

        Vector3 pointBetweenCameraAndTarget = (transform.position + target.position) / 2;
        Vector3 pointBelowCamera = new Vector3(transform.position.x, target.position.y, transform.position.z);

        float distanceToMidpoint = Vector3.Distance(pointBelowCamera, target.position);
        Vector3 boundingBoxExtend = new Vector3(distanceToMidpoint, distanceToMidpoint, distanceToMidpoint);

        transparencyArea = new Bounds(pointBetweenCameraAndTarget, boundingBoxExtend);
    }

    void FollowTarget(Vector3 target)
    {
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, 0.3f);
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
        zoomDistanceTarget = Mathf.Clamp(zoomDistanceTarget + (magnitude * zoomSensitivity), -2, 2);
    }

    public bool IsBetweenCameraAndPlayer(Transform transform)
    {
        if(transparencyArea.Contains(transform.position))
            return true;
        else
            return false;
    }
}
