using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [Header("Popup Properties")]
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private Transform popupContainer;

    private void Update()
    {
        //This is to debug the funtionality. Delete it later
        if (Input.GetKeyUp(KeyCode.R))
        {
            CreatePopup("test", 3, 5);
        }
    }

    public void CreatePopup(string description, float liveTime, float secondsToDestroy)
    {
        GameObject go = Instantiate(popupPrefab, popupContainer);
        go.GetComponentInChildren<TMP_Text>().text = description;
        StartCoroutine(DestroyPopup(go, liveTime, secondsToDestroy));
    }

    public IEnumerator DestroyPopup(GameObject popup, float liveTime, float secondsToDestroy)
    {
        yield return new WaitForSeconds(liveTime);
        popup.GetComponent<Animator>().SetTrigger("FadeOutTrigger");
        yield return new WaitForSeconds(secondsToDestroy);
        Destroy(popup);
    }
}
