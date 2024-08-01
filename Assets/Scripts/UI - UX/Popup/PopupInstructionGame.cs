using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PopupInstructionGame : MonoBehaviour

{
    public GameObject popup; // Assign the Popup GameObject here
    public Button acceptButton; // Assign the Accept Button here
    private CanvasGroup canvasGroup;

    void Start()
    {
        if (popup != null)
        {
            canvasGroup = popup.GetComponent<CanvasGroup>();
            popup.SetActive(false);
            StartCoroutine(ShowPopupAfterDelay());
            acceptButton.onClick.AddListener(HidePopup);
        }
        else
        {
            Debug.LogError("Popup GameObject not assigned in the inspector.");
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
        canvasGroup.alpha = 1; // Ensure alpha is set to 1
        StartCoroutine(HidePopupAfterDelay());
    }

    IEnumerator HidePopupAfterDelay()
    {
        yield return new WaitForSeconds(10f); // Wait 10 seconds in real time
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float duration = 0.5f;
        float currentTime = 0f;

        while (currentTime <= duration)
        {
            currentTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, currentTime / duration);
            yield return null;
        }
        canvasGroup.alpha = 0; // Ensure alpha is set to 0
        popup.SetActive(false);
    }

    void HidePopup()
    {
        StopAllCoroutines(); // Stop all running coroutines
        StartCoroutine(FadeOut());
        acceptButton.onClick.RemoveListener(HidePopup);
    }
}