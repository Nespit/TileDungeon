using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    public Camera mainCamera;
    public Transform target;
    public Vector3 initialPos, targetPos, rotationTarget, velocity;
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
        targetPos = target.position + offset[offsetIndex];
        transform.LookAt(target);
        velocity = Vector3.zero;

        CalculateTransparencyBoundingBoxExtend();
        CalculateTransparencyBoundingBox();
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position != target.position + offset[offsetIndex] && tUpdatePos > 1 && tUpdatePos > 1)
        {
            CalculateTransparencyBoundingBox();
        }
    }
    void LateUpdate()
    {
        targetPos = target.position + offset[offsetIndex];

        if(transform.position != target.position + offset[offsetIndex] && tUpdatePos > 1 && tUpdatePos > 1)
        {
            initialPos = transform.position;
            FollowTarget(targetPos);
        }

        if(tUpdatePos <= 1)
        {
            UpdatePos();
        }
    }

    void FollowTarget(Vector3 target)
    {
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, 0.3f);
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
        CalculateTransparencyBoundingBox();

        transform.position = originalPos;
        transform.rotation = originalRot;
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
        CalculateTransparencyBoundingBox();

        transform.position = originalPos;
        transform.rotation = originalRot;
    }

    void UpdatePos()
    {
        tUpdatePos += Time.deltaTime * cameraUpdateSpeed;
        transform.position = Vector3.Lerp(initialPos, targetPos, tUpdatePos);
        transform.LookAt(target);
    }

    public void ZoomIn()
    {
        if(zoomIndex == 0 || tUpdatePos <= 1)
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

        m_camera = StartCoroutine(CalculateTransparencyAfterZoomingIn());

        transform.position = originalPos;
        transform.rotation = originalRot;
    }

    public void ZoomOut()
    {
        if (zoomIndex == 2 || tUpdatePos <= 1)
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
        CalculateTransparencyBoundingBoxExtend();
        CalculateTransparencyBoundingBox();

        transform.position = originalPos;
        transform.rotation = originalRot;
    }

    IEnumerator CalculateTransparencyAfterZoomingIn()
    {
        WaitUntil wait = new WaitUntil(()=> UpdateFinished() == true);

        yield return wait;

        //Calculate transparency bounding box extend according to the targeted camera position.
        CalculateTransparencyBoundingBoxExtend();
        CalculateTransparencyBoundingBox();

        m_camera = null;
    }

    void CalculateTransparencyBoundingBoxExtend()
    {
        Vector3 pointBelowCamera = new Vector3(transform.position.x, target.position.y, transform.position.z);
        float distanceToLowpoint = Vector3.Distance(pointBelowCamera, target.position);
        boundingBoxExtend = new Vector3(distanceToLowpoint, distanceToLowpoint, distanceToLowpoint);
    }

    void CalculateTransparencyBoundingBox()
    {   
        pointBetweenCameraAndTarget = (transform.position + target.position) / 2;
        pointBetweenCameraAndTarget = new Vector3(pointBetweenCameraAndTarget.x, pointBetweenCameraAndTarget.y-0.2f, pointBetweenCameraAndTarget.z);
        transparencyArea = new Bounds(pointBetweenCameraAndTarget, boundingBoxExtend);
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
