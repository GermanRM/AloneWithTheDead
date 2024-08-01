using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    [SerializeField] private GameObject gameLoopHolder;
    [SerializeField] private GameObject gameOverHolder;

    [SerializeField] private TMP_Text zombiesKilledText;
    [SerializeField] private TMP_Text gameOverText;

    private void Update()
    {
        zombiesKilledText.text = $"Zombies Killed: {GameManager.Instance.zombiesKilled} / {GameManager.Instance.zombiesKilledGoal}";
    }

    public void LoadScene(string sceneName)
    {
        GameManager.Instance.LoadScene(sceneName);
    }

    public void SetGameOverText(string text) { gameOverText.text = text ; }

    public void EnableDisableGameLoopHolder(bool isActive) {  gameLoopHolder.SetActive(isActive); }
    public void EnableDisableGameOverHolder(bool isActive) {  gameOverHolder.SetActive(isActive); }

}
