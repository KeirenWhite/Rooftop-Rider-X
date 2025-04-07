using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private Button mmButton;
    [SerializeField] private TMP_Text score;

    [SerializeField] private SpawnManagerScriptableObject leaderboardObject;

    void Start()
    {
        mmButton.onClick.AddListener(ReturnToMainMenu);

        SetLeaderboard();
    }

    private void SetLeaderboard()
    {
        score.text = leaderboardObject.ScoreAtPlacement(0).ToString();
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
