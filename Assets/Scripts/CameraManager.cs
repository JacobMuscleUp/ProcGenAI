using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    float timer = 0.0f;
    bool flag = true;

    void LateUpdate()
    {
        var playerPos = Wolf.Player.transform.position;
        if (transform.position != new Vector3(playerPos[0], 30, playerPos[2]))
            timer = 0.0f;
        transform.position = Vector3.Lerp(transform.position, new Vector3(playerPos[0], 30, playerPos[2]), timer += Time.deltaTime);
        
        if (flag)
        {
            flag = false;
            transform.position = new Vector3(playerPos[0], 30, playerPos[2]);
            transform.LookAt(playerPos);
            timer = 1.0f;
        }

    }
}
