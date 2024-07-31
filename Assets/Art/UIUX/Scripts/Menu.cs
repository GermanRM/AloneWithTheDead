using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Menu : MonoBehaviour
{
    [SerializeField] private CanvasGroup myCanvasUI;
    [SerializeField] private bool fadeIn = false;
    [SerializeField] private bool fadeOut = false;
    [SerializeField] private float fadeSpeed = 1.0f; // Controla la velocidad del fade

    public void ShowUI()
    {
        fadeIn = true;
        fadeOut = false; // Asegura que solo se ejecute un fade a la vez
    }

    public void HideUI()
    {
        fadeIn = false; // Asegura que solo se ejecute un fade a la vez
        fadeOut = true;
    }

    private void Update()
    {
        if(fadeIn)
        {
            if (myCanvasUI.alpha < 1)
            {
                myCanvasUI.alpha += Time.deltaTime;
                if(myCanvasUI.alpha >= 1)
                {
                    fadeIn = false;
                }
            }
        }
        if (fadeOut)
        {
            if (myCanvasUI.alpha >= 0)
            {
                myCanvasUI.alpha -= Time.deltaTime;
                if (myCanvasUI.alpha == 0)
                {
                    fadeOut = false;
                }
            }
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
    public void StartLevel(string LevelName)
    {
        SceneManager.LoadScene(LevelName);
    }
}
