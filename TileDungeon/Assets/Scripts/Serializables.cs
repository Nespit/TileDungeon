using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class SavedInteractableObject //keys, coins, doors
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public InteractableObjectType objectType;

    public SavedInteractableObject(Vector3 position, Quaternion rotation, InteractableObjectType objectType)
    {
        this.position = position;
        this.rotation = rotation;
        this.objectType = objectType;
    }
}

[Serializable]
public class SavedCharacter
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public int currentHealth;
    public SavedCharacter(Vector3 position, Quaternion rotation, int currentHealth)
    {
        this.position = position;
        this.rotation = rotation;
        this.currentHealth = currentHealth;
    }
}

[Serializable]
public class SavedObjectsList
{
    public int SceneID;
    public List<SavedInteractableObject> SavedInteractableObjects;
    public List<SavedCharacter> SavedCharacters;

    public SavedObjectsList(int newSceneID)
    {
        this.SceneID = newSceneID;
        this.SavedInteractableObjects = new List<SavedInteractableObject>();
        this.SavedCharacters = new List<SavedCharacter>();
    }
}

[Serializable]
 public struct SerializableVector3
 {
     public float x;
     
     public float y;
     
     public float z;
     
     public SerializableVector3(float rX, float rY, float rZ)
     {
         x = rX;
         y = rY;
         z = rZ;
     }
     
     public override string ToString()
     {
         return String.Format("[{0}, {1}, {2}]", x, y, z);
     }
     
     public static implicit operator Vector3(SerializableVector3 rValue)
     {
         return new Vector3(rValue.x, rValue.y, rValue.z);
     }
     
     public static implicit operator SerializableVector3(Vector3 rValue)
     {
         return new SerializableVector3(rValue.x, rValue.y, rValue.z);
     }
 }

[Serializable]
 public struct SerializableQuaternion
 {
     public float x;
     
     public float y;
     
     public float z;
     
     public float w;
     
     public SerializableQuaternion(float rX, float rY, float rZ, float rW)
     {
         x = rX;
         y = rY;
         z = rZ;
         w = rW;
     }
     
     public override string ToString()
     {
         return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
     }
     
     public static implicit operator Quaternion(SerializableQuaternion rValue)
     {
         return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
     }
     
     public static implicit operator SerializableQuaternion(Quaternion rValue)
     {
         return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
     }
 }
public enum InteractableObjectType
{
    key,
    coin,
    door 
}