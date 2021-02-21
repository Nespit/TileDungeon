using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class SavedInteractableObject //keys, coins, doors
{
    //public float tileID;
    public int[] tileID = new int[2];
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public InteractableObjectType objectType;

    public SavedInteractableObject(int[] tileID, Vector3 position, Quaternion rotation, InteractableObjectType objectType)
    {
        this.tileID = tileID;
        this.position = position;
        this.rotation = rotation;
        this.objectType = objectType;
    }
}

[Serializable]
public class SavedCharacter
{   
    //public float tileID;
    public int[] tileID = new int[2];
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public int currentHealth;
    public int maxHealth;
    public int attackStrength;
    public int defenseStrength;
    public int maxActionPoints; 
    public int turnOrderRating; 
    public CharacterBehaviourType behaviour;

    public SavedCharacter(int[] tileID, Vector3 position, Quaternion rotation, int currentHealth, 
                          int maxHealth, int attackStrength, int defenseStrength, int maxActionPoints, int turnOrderRating, CharacterBehaviourType behaviour)
    {
        this.tileID = tileID;
        this.position = position;
        this.rotation = rotation;
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        this.attackStrength = attackStrength;
        this.defenseStrength = defenseStrength;
        this.maxActionPoints = maxActionPoints;
        this.turnOrderRating = turnOrderRating;
        this.behaviour = behaviour;
    }
}

[Serializable]
public class SavedListsPerScene
{
    public int SceneID;
    public List<SavedInteractableObject> SavedInteractableObjects;
    public List<SavedCharacter> SavedCharacters;

    public SavedListsPerScene(int newSceneID)
    {
        this.SceneID = newSceneID;
        this.SavedInteractableObjects = new List<SavedInteractableObject>();
        this.SavedCharacters = new List<SavedCharacter>();
    }
}

[Serializable]
public class SavedDataPerGame
{
    public int SceneID;
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public int currentHealth;
    public bool hasKey;

    public SavedDataPerGame(int newSceneID, Vector3 position, Quaternion rotation, int currentHealth, bool hasKey)
    {
        this.SceneID = newSceneID;
        this.position = position;
        this.rotation = rotation;
        this.currentHealth = currentHealth;
        this.hasKey = hasKey;
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

public enum CharacterBehaviourType
{
    passive,
    proximity,
    aggressive,
    player 
}