using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public Camera mainCamera;
    public Transform target;
    public Vector3 initialPos, targetPos, rotationTarget, velocity, targetTargetPos;
    Vector3[] offset = new Vector3[4];
    WaitUntil m_cameraMovementCondition;
    Coroutine m_camera;
    int offsetIndex = 0;
    int zoomIndex = 0;
    public float cameraUpdateSpeed = 4;
    public float tUpdatePos = 2;
    Bounds transparencyArea;
    public Transform meshTransform;
    public Mesh mesh;
    Vector3 boundingBoxExtend;
    Vector3 pointBetweenCameraAndTarget;
    public delegate void TransparencyDelegate(object sender, EventArgs args);
    public event TransparencyDelegate TransparencyEvent;
    public float boundingBoxSideLength;
    bool rotating;

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
        offset[1] = new Vector3(-offset[0].z, offset[0].y, offset[0].x);
        offset[2] = new Vector3(offset[0].x , offset[0].y, -offset[0].z);
        offset[3] = new Vector3(offset[0].z, offset[0].y, offset[0].x);
        targetPos = targetTargetPos + offset[offsetIndex];
        transform.LookAt(target);
        velocity = Vector3.zero;

        // CalculateDynamicTransparencyBoundingBoxExtend();
        // CalculateDynamicTransparencyBoundingBox();
        CalculateStaticTransparencyBohundingBoxExtend(boundingBoxSideLength);
        CalculateStaticTransparencyBoundingBox(boundingBoxSideLength);
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position != target.position + offset[offsetIndex])
        {
            // CalculateDynamicTransparencyBoundingBox();
            //CalculateStaticTransparencyBoundingBox(boundingBoxSideLength);
        }
    }
    void LateUpdate()
    {
        //targetPos = target.position + offset[offsetIndex];

        // if(transform.position != target.position + offset[offsetIndex] && tUpdatePos > 1)
        // {
        //     initialPos = transform.position;
        //     FollowTarget(targetPos);
        // }

        if(tUpdatePos <= 1)
        {
            UpdatePos();
        }
        else if(rotating)
            rotating = false;
    }

    void UpdatePos()
    {
        tUpdatePos += Time.deltaTime * cameraUpdateSpeed;
        transform.position = Vector3.Lerp(initialPos, targetTargetPos + offset[offsetIndex], tUpdatePos);

        if(rotating)
            transform.LookAt(target);
    }

    public void FollowTarget()
    {
        targetPos = targetTargetPos + offset[offsetIndex];
        tUpdatePos = 0;
        initialPos = transform.position;
        CalculateStaticTransparencyBoundingBox(boundingBoxSideLength);
        //transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, 0.3f);
    }

    public void RotateClockwise()
    {
        tUpdatePos = 0;

        initialPos = transform.position;
        
        if(offsetIndex == 3)
            offsetIndex = 0;
        else
            offsetIndex += 1;


        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        transform.position = target.position + offset[offsetIndex];
        transform.LookAt(target);

        transform.position = originalPos;
        transform.rotation = originalRot;

        targetPos = targetTargetPos + offset[offsetIndex];
        rotating = true;

        // CalculateDynamicTransparencyBoundingBox();
        CalculateStaticTransparencyBoundingBox(boundingBoxSideLength);
    }

    public void RotateCounterClockwise()
    {
        tUpdatePos = 0;

        initialPos = transform.position;
        
        if(offsetIndex == 0)
            offsetIndex = 3;
        else
            offsetIndex -= 1;

        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        transform.position = target.position + offset[offsetIndex];
        transform.LookAt(target);

        transform.position = originalPos;
        transform.rotation = originalRot;

        targetPos = targetTargetPos + offset[offsetIndex];
        rotating = true;

        // CalculateDynamicTransparencyBoundingBox();
        CalculateStaticTransparencyBoundingBox(boundingBoxSideLength);
    }

    public void ZoomIn()
    {
        if(zoomIndex == 0 || tUpdatePos < 1)
            return;
        
        initialPos = transform.position;
        zoomIndex -= 1;
        tUpdatePos = 0;

        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        transform.position = target.position + offset[0];
        transform.LookAt(target);
        transform.position = transform.position + (transform.forward * 1.9f);

        offset[0] = transform.position - target.position;
        offset[1] = new Vector3(-offset[0].z, offset[0].y, offset[0].x);
        offset[2] = new Vector3(offset[0].x , offset[0].y, -offset[0].z);
        offset[3] = new Vector3(offset[0].z, offset[0].y, offset[0].x);

        transform.position = originalPos;
        transform.rotation = originalRot;

        targetPos = targetTargetPos + offset[offsetIndex];

        m_camera = StartCoroutine(CalculateTransparencyAfterZoomingIn());
    }

    public void ZoomOut()
    {
        if (zoomIndex == 2 || tUpdatePos < 1)
            return;

        initialPos = transform.position;
        zoomIndex += 1;
        tUpdatePos = 0;

        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        transform.position = target.position + offset[0];
        transform.LookAt(target);
        transform.position = transform.position + (transform.forward * -1.9f);

        offset[0] = transform.position - target.position;
        offset[1] = new Vector3(-offset[0].z, offset[0].y, offset[0].x);
        offset[2] = new Vector3(offset[0].x , offset[0].y, -offset[0].z);
        offset[3] = new Vector3(offset[0].z, offset[0].y, offset[0].x);
        
        //Calculate transparency bounding box extend according to the targeted camera position.
        transform.position = target.position + offset[offsetIndex];
        // CalculateDynamicTransparencyBoundingBoxExtend();
        // CalculateDynamicTransparencyBoundingBox();
        
        CalculateStaticTransparencyBoundingBox(boundingBoxSideLength);

        transform.position = originalPos;
        transform.rotation = originalRot;

        targetPos = targetTargetPos + offset[offsetIndex];
    }

    IEnumerator CalculateTransparencyAfterZoomingIn()
    {
        WaitUntil wait = new WaitUntil(()=> UpdateFinished() == true);

        yield return wait;

        //Calculate transparency bounding box extend according to the targeted camera position.
        // CalculateDynamicTransparencyBoundingBoxExtend();
        // CalculateDynamicTransparencyBoundingBox();
        CalculateStaticTransparencyBoundingBox(boundingBoxSideLength);

        m_camera = null;
    }

    void CalculateDynamicTransparencyBoundingBoxExtend()
    {
        Transform lowPointTransform = new GameObject().transform;
        lowPointTransform.position = new Vector3(transform.position.x, target.position.y, transform.position.z);
        lowPointTransform.rotation = Quaternion.LookRotation(lowPointTransform.position - target.position);
        lowPointTransform.position += lowPointTransform.forward * 1f;
        float distanceToLowpoint = Vector3.Distance(lowPointTransform.position, target.position);
        boundingBoxExtend = new Vector3(distanceToLowpoint, distanceToLowpoint, distanceToLowpoint);
    }

    void CalculateStaticTransparencyBohundingBoxExtend(float length)
    {
        boundingBoxExtend = new Vector3(length, length, length);
    }

    void CalculateDynamicTransparencyBoundingBox()
    {   
        pointBetweenCameraAndTarget = (transform.position + target.position) / 2;
        pointBetweenCameraAndTarget = new Vector3(pointBetweenCameraAndTarget.x, pointBetweenCameraAndTarget.y-0.2f, pointBetweenCameraAndTarget.z);
        
        Transform center = new GameObject().transform;
        center.position = pointBetweenCameraAndTarget;
        Vector3 faceAwayFrom = new Vector3(target.position.x, center.position.y, target.position.z);
        center.rotation = Quaternion.LookRotation(center.position - faceAwayFrom);
        center.position += center.forward * 0.6f;

        pointBetweenCameraAndTarget = center.position;

        transparencyArea = new Bounds(pointBetweenCameraAndTarget, boundingBoxExtend);
        FireTransparencyCheck();
    }

    void CalculateStaticTransparencyBoundingBox(float length)
    {
        Vector3 originalPos = transform.position;
        transform.position = targetTargetPos + offset[offsetIndex];

        Transform center = new GameObject().transform;
        center.position = new Vector3(targetTargetPos.x, targetTargetPos.y + length/4, targetTargetPos.z);
        Vector3 faceTowards = new Vector3(transform.position.x, center.position.y, transform.position.z);
        center.LookAt(faceTowards);
        center.position += center.forward * (length/2 + 0.1f);

        pointBetweenCameraAndTarget = center.position;

        transparencyArea = new Bounds(center.position, boundingBoxExtend);

        transform.position = originalPos;
        FireTransparencyCheck();
    }

    public bool UpdateFinished()
    {
        if(tUpdatePos > 1)
            return true;
        else   
            return false;
    }
    public bool IsBetweenCameraAndPlayer(Transform transform)
    {
        if(transparencyArea.Contains(transform.position))
            return true;
        else
            return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pointBetweenCameraAndTarget, boundingBoxExtend);
    }

    void FireTransparencyCheck()
    {
        if (TransparencyEvent != null)
            TransparencyEvent(null, null);
    }
}
