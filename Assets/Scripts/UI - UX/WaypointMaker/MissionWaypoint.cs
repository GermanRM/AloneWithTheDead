using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionWaypoint : MonoBehaviour
{

public Image img;
public Transform target;
public TMP_Text meter;

    private void Update()
    {
        float minX = img.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = img.GetPixelAdjustedRect(). height / 2;
        float maxY = Screen.height - minY;
        Vector2 pos = Camera.main.WorldToScreenPoint(target.position);
    
        if (Vector3.Dot((target.position - transform.position), transform.forward) < 0)
        // Target is ehind the player
        if(pos.x < Screen.width / 2)
        {
            pos.x = maxX;
        } else 
        {
            pos.x = minX;
        }

        pos.x = Math.Clamp(pos.x, minX, maxX);
        pos.y = Math.Clamp(pos.y, minY, maxY);

        img.transform.position = pos;
        meter.text = ((int)Vector3.Distance(target.position, transform.position)).ToString() + "m";

    }
}
