using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [Header("Popup Properties")]
    [SerializeField] private List<GameObject> popupList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FadeIn(GameObject go)
    {
        Animator animator = go.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("FadeIn");
        }
    }

     private void FadeOut(GameObject go)
    {
        Animator animator = go.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("FadeOut");
        }
    }
}
