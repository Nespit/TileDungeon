using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCanvasController : MonoBehaviour
{
    void Start()
    {

    }

    void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
