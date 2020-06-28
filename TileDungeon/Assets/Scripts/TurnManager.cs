using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance = null;
    public delegate void TurnDelegate(object sender, EventArgs args);
    public event TurnDelegate TurnEvent;
    public List<CharacterScript> rawTurnQueue;
    public List<CharacterScript> sortedTurnQueue;
    int turnOrderIndex = 0;
    public WaitUntil m_unitTurnIsOver;
    Coroutine m_unitTurn;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Turn()
    {
        if(turnOrderIndex > sortedTurnQueue.Count || sortedTurnQueue.Count < 1)
        {
            PrepareNewTurn();
            m_unitTurn = StartCoroutine(ExecuteCharacterTurns());
        }
    }

    void PrepareNewTurn()
    {
        rawTurnQueue = new List<CharacterScript>();
        sortedTurnQueue = new List<CharacterScript>();
        turnOrderIndex = 0;

        if (TurnEvent == null)
            return;
        
        TurnEvent(null, null);
        SortTurnQueue();
    }

    void SortTurnQueue()
    {
        if(rawTurnQueue.Count < 1)
            return;
        
        sortedTurnQueue = rawTurnQueue.OrderBy(o => o.turnOrderRating).ToList();
    }

    IEnumerator ExecuteCharacterTurns()
    {
        if(sortedTurnQueue.Count < 1)
            yield break;


        for (turnOrderIndex = 0; turnOrderIndex < sortedTurnQueue.Count; ++turnOrderIndex)
        {
            m_unitTurnIsOver = new WaitUntil(() => sortedTurnQueue[turnOrderIndex].turnFinished == true);
            
            sortedTurnQueue[turnOrderIndex].turnFinished = false;

            sortedTurnQueue[turnOrderIndex].StartTurn();

            yield return m_unitTurnIsOver;
        }

        ++turnOrderIndex;
    }
}
