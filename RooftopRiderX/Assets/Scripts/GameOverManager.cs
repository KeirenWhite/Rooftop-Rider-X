using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private Button mmButton;
    [SerializeField] private TMP_Text bearerList;
    [SerializeField] private TMP_Text scoreList;

    [SerializeField] private SpawnManagerScriptableObject leaderboardObject;

    void Start()
    {
        mmButton.onClick.AddListener(ReturnToMainMenu);

        SetLeaderboard();
    }

    private void SetLeaderboard()
    {
        for (int i = 0; i < 10; i++)
        {
            bearerList.text += (i == 0 ? "" : "\n") + leaderboardObject.BearerAtPlacement(i);
            scoreList.text += (i == 0 ? "" : "\n") + leaderboardObject.ScoreAtPlacement(i);
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
