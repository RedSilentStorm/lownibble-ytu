using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    private List<ITurnTaker> turnQueue = new List<ITurnTaker>();
    private int currentIndex = -1;

    private void Awake()
    {
        Instance = this;
    }

    public void AddTurnTaker(ITurnTaker taker)
    {
        turnQueue.Add(taker);
    }

    public void StartBattle()
    {
        // Sıralamayı hıza göre yap (büyükten küçüğe)
        turnQueue.Sort((a, b) => b.GetSpeed().CompareTo(a.GetSpeed()));
        currentIndex = -1;
        NextTurn();
    }

    public void NextTurn()
    {
        if (turnQueue.Count == 0) return;
        currentIndex = (currentIndex + 1) % turnQueue.Count;
        turnQueue[currentIndex].StartTurn();
    }

    public void EndTurn()
    {
        // Bir sonraki sıra
        NextTurn();
    }
}
