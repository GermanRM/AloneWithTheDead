using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
[Range(0, 100)] 
public float value; 
public float speed;
public float margin;
public float multiplier;
public GameObject player;
public RectTransform top, bottom, left, right, center;
    void Update()
    {
        value = player.GetComponent<PlayerMovement>().GetMoveVelocityMagnitude() * multiplier;

        float TopValue, BottomValue, LeftValue, RightValue;

        TopValue = Mathf.Lerp(top.position.y, center.position.y + margin + value, speed * Time.deltaTime);
        BottomValue = Mathf.Lerp(bottom.position.y, center.position.y - margin  - value, speed * Time.deltaTime);
        LeftValue = Mathf. Lerp(left.position.x, center.position.x - margin - value, speed * Time.deltaTime);
        RightValue = Mathf. Lerp(right.position.x, center.position.x + margin + value, speed * Time.deltaTime);
        
        top.position = new Vector2(top.position.x, TopValue);
        bottom.position = new Vector2(bottom.position.x, BottomValue);
        left.position = new Vector2(LeftValue, center.position.y); 
        right.position = new Vector2(RightValue, center.position.y);
    }


}
