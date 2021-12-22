using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEngine : MonoBehaviour
{
    public event Action LevelStarted;
    [SerializeField] private GameObject tapToStartButton;
    [SerializeField] private GameObject tapToPlayAgainButton;
    [SerializeField] private CinemachineVirtualCamera gameCam;
    
    public void TappedToStart()
    {
        LevelStarted?.Invoke();
        tapToStartButton.SetActive(false);
        gameCam.Priority = 11;
    }
    public void LevelEnded()
    {
        tapToPlayAgainButton.SetActive(true);
    }
    public void TappedToPlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
