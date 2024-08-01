using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScritp : MonoBehaviour
{
    public void RestartButtom()
    {
        SceneManager.LoadScene("MainLevel");
    }

    public void ExitBottom()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
