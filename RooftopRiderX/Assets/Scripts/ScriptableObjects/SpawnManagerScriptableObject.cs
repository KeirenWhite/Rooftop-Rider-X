using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject")]
public class SpawnManagerScriptableObject : ScriptableObject
{
    private ScoreScriptableObject [] scoreArray = new ScoreScriptableObject [10];

    private bool firstUse = false;

    private void FirstUse()
    {
        if (firstUse) return;
        firstUse = true;

        for (int i = 0; i < scoreArray.Length - 1; i++)
        {
            ScoreScriptableObject score = CreateInstance<ScoreScriptableObject>();
            score.SetScore(0);
            score.SetScoreBearer("N/A");
            scoreArray[i] = score;
        }
    }

    public void AddNewScore(string bearer, int score)
    {
        FirstUse();

        ScoreScriptableObject scoreObj = CreateInstance<ScoreScriptableObject>();
        scoreObj.SetScore(score);
        scoreObj.SetScoreBearer(bearer);

        scoreArray[9] = scoreObj;

        BubbleSort(scoreArray);
    }

    static void BubbleSort(ScoreScriptableObject[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
            for (int j = 0; j < n - i - 1; j++)
                if (arr[j].Score() < arr[j + 1].Score())
                {
                    ScoreScriptableObject temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                }
    }

    public int ScoreAtPlacement(int placement)
    {
        if (scoreArray[placement] != null) return scoreArray[placement].Score();

        return 0;
    }

    public string BearerAtPlacement(int placement)
    {
        if (scoreArray[placement] != null) return scoreArray[placement].ScoreBearer();

        return "N/A";
    }
}
