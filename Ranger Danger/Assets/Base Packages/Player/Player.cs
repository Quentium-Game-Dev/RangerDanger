using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Speed Variables
    public float speed = 1.75f;
    public float sprintSpeed = 2.5f;
    private float normalSpeed;
    public float accelerationRate = 0.1f;
    public float decelerationRate = 0.25f;

    //Stamina Variables
    public float agility = 0.1f;
    public float exhaustionRate = 0.25f;
    public float availableStamina = 4.0f;
    public float minimumStamina;
    public float maximumStamina = 4.0f;

    private float timer = 0f;
    public float exhaustionTime = 3f;
   
    // Players current position in the game world
    private Vector2 currentPosition;
   
    // Calls player controller
    private PlayerControls playerControls;
    // Variables for storing the players inputs 
    private float leftRightInput;   // Left or Right
    private float upDownInput;      // Up or Down
    public float isSprinting;       // Sprint


    public enum playerState
    {
        Idle,
        Walk,
        Sprint,
        Recovering,
        Exhausted
    }

    public playerState state;

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
        normalSpeed = speed;
        availableStamina = maximumStamina;
        minimumStamina = 0.1f;

    }

    void Update()
    {
        ReadInputs();

        GetPlayerState();

        currentPosition = transform.position;
        
    }

    private void FixedUpdate()
    {
        //if the player is holding shift, not tired and has stamina they can sprint
        if (state == playerState.Sprint)
        {
            speed = Mathf.Lerp(speed, sprintSpeed, accelerationRate * Time.fixedDeltaTime);     //Over time increase sprinting speed
            availableStamina -= exhaustionRate * Time.fixedDeltaTime;     //Over time decrease stamina.
        }

        //    if (isExhausted == true && isSprinting >= 0.5f)
        //   {
        //      availableStamina = Mathf.Lerp(availableStamina, maximumStamina, exhaustionRate);

        //  }


        if (state == playerState.Recovering || state == playerState.Walk || state == playerState.Idle)
        {
            availableStamina = Mathf.Lerp(availableStamina, maximumStamina, agility * Time.fixedDeltaTime);
        }

        //If the player is exhausted and they are still sprinting
        if (state == playerState.Exhausted || state == playerState.Walk)
        {
            speed = Mathf.Lerp(speed, normalSpeed, decelerationRate * Time.fixedDeltaTime);
        }
        if (state == playerState.Idle)
        {
            speed = Mathf.Lerp(speed, 0, decelerationRate * Time.fixedDeltaTime);
        }

        currentPosition.x += leftRightInput * speed * Time.fixedDeltaTime;

        currentPosition.y += upDownInput * speed * Time.fixedDeltaTime;

        transform.position = currentPosition;
    }


    public void Sprinting()
    {
           
    }


    public void GetPlayerState()
    {
        if (isSprinting >= 0.5f && availableStamina <= minimumStamina) // holding shift and lacking stamina
        {
            timer += Time.fixedDeltaTime;
            state = playerState.Exhausted;
            //minimumStamina = 0.25f * maximumStamina;
        }


        if (isSprinting < 0.5f && availableStamina <= minimumStamina && timer > exhaustionTime) // not holding shift and lacking stamina
        {
            state = playerState.Recovering;
            timer = 0;
        }
        
        if (isSprinting >= 0.5f && state != playerState.Exhausted) //Holding down Shift
            state = playerState.Sprint;

        if (isSprinting < 0.5f && state != playerState.Exhausted)  //Not holding shift
            state = playerState.Walk;

        if (leftRightInput == 0 && upDownInput == 0 && timer == 0)  //doing nothing
            state = playerState.Idle;

        //print(state);
    }

    public void ReadInputs()
    {
        leftRightInput = playerControls.NormalMovement.LeftRight.ReadValue<float>();

        upDownInput = playerControls.NormalMovement.UpDown.ReadValue<float>();

        isSprinting = playerControls.NormalMovement.Sprint.ReadValue<float>();
    }
}


