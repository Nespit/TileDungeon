using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InteractableObject : MonoBehaviour
{
    public InteractableObjectType objectType;
    public bool dontReload = false;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.SaveEvent += SaveFunction;

        SavedListsPerScene localListOfSceneObjectsToLoad = GameManager.instance.GetListForScene(gameObject.scene.buildIndex);
        
        if(localListOfSceneObjectsToLoad != null && dontReload)
            Destroy(gameObject);
    }

    public void OnDestroy()
    {
        GameManager.instance.SaveEvent -= SaveFunction;
    }

    public void SaveFunction(object sender, EventArgs args)
    {
        //float tileID = transform.parent.GetComponent<TileScript>().tileID;
        int[] tileID = transform.parent.GetComponent<TileScript>().tileID;
        
        SavedInteractableObject savedObject = new SavedInteractableObject(tileID, transform.position, transform.rotation, objectType);

        GameManager.instance.GetListForScene().SavedInteractableObjects.Add(savedObject);  
    }

    public void Interact()
    {
        Destroy(gameObject);
    }
}
