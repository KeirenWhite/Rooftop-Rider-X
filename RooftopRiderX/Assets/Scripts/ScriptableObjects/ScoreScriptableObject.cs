using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ScoreScriptableObject")]
public class ScoreScriptableObject : ScriptableObject
{
    [SerializeField] private string prefabName;

    private string scoreBearer;
    private int score;

    public void SetScore(int newScore)
    {
        score = newScore;
    }

    public void SetScoreBearer(string newScoreBearer)
    {
        scoreBearer = newScoreBearer;
    }

    public int Score()
    {
        return score;
    }

    public string ScoreBearer()
    {
        return scoreBearer;
    }
}
