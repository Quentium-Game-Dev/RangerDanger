using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Speed & Sprint Values
    public float speed;
    public float sprintSpeed;
    public float sprintDuration;
    public float sprintTransition;
     
    // Calls player controller
    private PlayerControls playerControls;
   
    // Players current position in the game world
    private Vector2 currentPosition;

    // Variables for storing the players inputs (-1 or 1)
    private float leftRightInput;
    private float upDownInput;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }
  
    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Start()
    {
    
    }

    void Update()
    {
        currentPosition = transform.position;

        leftRightInput = playerControls.NormalMovement.LeftRight.ReadValue<float>();

        upDownInput = playerControls.NormalMovement.UpDown.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        currentPosition.x += leftRightInput * speed * Time.fixedDeltaTime;

        currentPosition.y += upDownInput * speed * Time.fixedDeltaTime;

        transform.position = currentPosition;
    }
}
