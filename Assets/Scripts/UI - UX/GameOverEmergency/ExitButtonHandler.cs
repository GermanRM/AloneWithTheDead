
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButtonHandler : MonoBehaviour
{
    // Este método será llamado cuando se haga clic en el botón de salida
    public void OnExitButtonClick()
    {
        // Carga la escena "Main"
        SceneManager.LoadScene("Main Menu");
    }
}
