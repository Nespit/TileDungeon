﻿using System.Collections;
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
            StartNewTurn();
        }
    }

    public void StartNewTurn()
    {
        WipeTurnQueue();

        if (TurnEvent == null)
            return;
        
        TurnEvent(null, null);
        SortTurnQueue();
        m_unitTurn = StartCoroutine(ExecuteCharacterTurns());
    }

    void SortTurnQueue()
    {
        if(rawTurnQueue.Count < 1)
            return;
        
        sortedTurnQueue = rawTurnQueue.OrderBy(o => o.turnOrderRating).ToList();
    }

    public void WipeTurnQueue()
    {
        if(m_unitTurn != null)
            StopCoroutine(m_unitTurn);

        rawTurnQueue = new List<CharacterScript>();
        sortedTurnQueue = new List<CharacterScript>();
        turnOrderIndex = 0;
    }

    IEnumerator ExecuteCharacterTurns()
    {
        if(sortedTurnQueue.Count < 1)
            yield break;


        for (turnOrderIndex = 0; turnOrderIndex < sortedTurnQueue.Count; ++turnOrderIndex)
        {
            if(sortedTurnQueue[turnOrderIndex] == null)
                continue;

            m_unitTurnIsOver = new WaitUntil(() => sortedTurnQueue[turnOrderIndex].turnActive == false);
            
            sortedTurnQueue[turnOrderIndex].turnActive = true;

            sortedTurnQueue[turnOrderIndex].StartTurn();

            yield return m_unitTurnIsOver;
        }

        ++turnOrderIndex;
    }
}
