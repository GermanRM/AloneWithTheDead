using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PopupInstructionGame : MonoBehaviour
{
    public GameObject popup; // Asigna el objeto Popup aquí
    public Button acceptButton; // Asigna el botón Aceptar aquí
    private CanvasGroup canvasGroup;
    private Animator animator; // Añadir Animator

    void Start()
    {
        if (popup != null)
        {
            canvasGroup = popup.GetComponent<CanvasGroup>();
            animator = popup.GetComponent<Animator>(); // Obtener el Animator
            popup.SetActive(false);
            StartCoroutine(ShowPopupAfterDelay());
            acceptButton.onClick.AddListener(HidePopup);
        }
        else
        {
            Debug.LogError("Popup GameObject no asignado en el inspector.");
        }
    }

    IEnumerator ShowPopupAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        popup.SetActive(true);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float duration = 0.5f;
        float currentTime = 0f;
        
        while (currentTime <= duration)
        {
            currentTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, currentTime / duration);
            yield return null;
        }
        Time.timeScale = 0f; // Pausar el juego
        StartCoroutine(HidePopupAfterDelay());
    }

    IEnumerator HidePopupAfterDelay()
    {
        yield return new WaitForSecondsRealtime(10f); // Esperar 10 segundos en tiempo real
        animator.SetBool("IsFadingOut", true); // Activar el parámetro de fade out
    }

    IEnumerator FadeOut()
    {
        float duration = 0.5f;
        float currentTime = 0f;

        while (currentTime <= duration)
        {
            currentTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, currentTime / duration);
            yield return null;
        }
        popup.SetActive(false);
        Time.timeScale = 1f; // Reanudar el juego
        animator.SetBool("IsFadingOut", false); // Reiniciar el parámetro de fade out
    }

    void HidePopup()
    {
        StopAllCoroutines(); // Detener corutinas en curso
        StartCoroutine(FadeOut());
        Time.timeScale = 1f; // Reanudar el juego
        acceptButton.onClick.RemoveListener(HidePopup);
    }
}