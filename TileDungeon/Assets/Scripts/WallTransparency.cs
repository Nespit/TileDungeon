using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WallTransparency : MonoBehaviour
{
    MeshRenderer localRenderer;
    Material standardMat;
    public Material transparentMat;

    // Start is called before the first frame update
    void Start()
    {
        localRenderer = GetComponent<MeshRenderer>();
        standardMat = localRenderer.material;
        CameraManager.instance.TransparencyEvent += TransparencyFunction;
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (CameraManager.instance.IsBetweenCameraAndPlayer(transform))
    //         localRenderer.material = transparentMat;
    //     else
    //         localRenderer.material = standardMat;
    // }

    void TransparencyFunction(object sender, EventArgs args)
    {
        if (CameraManager.instance.IsBetweenCameraAndPlayer(transform))
            localRenderer.material = transparentMat;
        else
            localRenderer.material = standardMat;
    }

    public void OnDestroy()
    {
        CameraManager.instance.TransparencyEvent -= TransparencyFunction;
    }
}
