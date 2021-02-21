using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform player;
    public float camSpeed;

    private void Start()
    {
        transform.position = player.position;
    }
    private void FixedUpdate()
    {
        if(player != null)
        {
            transform.position = Vector2.Lerp(transform.position, player.position, camSpeed);
        }
    }
}
