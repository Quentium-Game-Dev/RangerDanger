using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Player State Variables
    public enum playerState
    {
        Idle,
        Walk,
        Sprint,
        Recovering,
        Exhausted
    }
    public playerState state;

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
    //Stamina Timers
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

    //Debug
    SpriteRenderer playerSprite;

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
        //Debug
        playerSprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        ReadInputs();

        GetPlayerState();

        currentPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Sprinting();

        currentPosition.x += leftRightInput * speed * Time.fixedDeltaTime;

        currentPosition.y += upDownInput * speed * Time.fixedDeltaTime;

        transform.position = currentPosition;
    }

    ///  <summary>  ///
    ///   METHODS   ///
    /// </summary>  ///

    public void Sprinting()
    {
        //If the player is holding shift, not tired and has stamina they can sprint
        if (state == playerState.Sprint)
        {
            speed = Mathf.Lerp(speed, sprintSpeed, accelerationRate * Time.fixedDeltaTime);     //Over time increase sprinting speed
            availableStamina -= exhaustionRate * Time.fixedDeltaTime;     //Over time decrease stamina.
        }

        //If the player is not making any stamina exhausting movements
        if (state == playerState.Recovering || state == playerState.Walk || state == playerState.Idle)
        {
            availableStamina = Mathf.Lerp(availableStamina, maximumStamina, agility * Time.fixedDeltaTime);
        }

        //If the player is exhausted and they are still sprinting
        if (state == playerState.Exhausted || state == playerState.Walk)
        {
            speed = Mathf.Lerp(speed, normalSpeed, decelerationRate * Time.fixedDeltaTime);
        }

        //If the player stops moving/looses input
        if (state == playerState.Idle)
        {
            speed = Mathf.Lerp(speed, 0, decelerationRate * Time.fixedDeltaTime);
        }
    }

    public void GetPlayerState()
    {
        //If the player is holding shift and lacking stamina
        if (isSprinting >= 0.5f && availableStamina <= minimumStamina) 
        {
            timer += Time.fixedDeltaTime;
            state = playerState.Exhausted;
            //Debug
            DebugState();
        }

        //If the player is not holding shift and their stamina is less than the max and they are no longer exhausted
        if (isSprinting < 0.5f && availableStamina <= minimumStamina && timer > exhaustionTime) 
        {
            state = playerState.Recovering;
            timer = 0;
            //Debug
            DebugState();
        }

        //If the player is holding down shift and not exhausted 
        if (isSprinting >= 0.5f && state != playerState.Exhausted) 
        {
            state = playerState.Sprint;
            //Debug
            DebugState();
        }
        
        //If the player is not holding shift and not exhausted
        if (isSprinting < 0.5f && state != playerState.Exhausted)
        {
            state = playerState.Walk;
            //Debug
            DebugState();
        }
            
        //If the player is doing nothing
        if (leftRightInput == 0 && upDownInput == 0 && timer == 0)  //doing nothing
        {
            state = playerState.Idle;
            //Debug
            DebugState();
        }    
    }

    public void ReadInputs()
    {
        leftRightInput = playerControls.NormalMovement.LeftRight.ReadValue<float>();

        upDownInput = playerControls.NormalMovement.UpDown.ReadValue<float>();

        isSprinting = playerControls.NormalMovement.Sprint.ReadValue<float>();
    }

    public void DebugState()
    {
        if (state == playerState.Idle)
            playerSprite.sprite = Resources.Load<Sprite>(state.ToString());
        if (state == playerState.Walk)
            playerSprite.sprite = Resources.Load<Sprite>(state.ToString());
        if (state == playerState.Sprint)
            playerSprite.sprite = Resources.Load<Sprite>(state.ToString());
        if(state == playerState.Recovering)
            playerSprite.sprite = Resources.Load<Sprite>(state.ToString());
        if(state == playerState.Exhausted)
            playerSprite.sprite = Resources.Load<Sprite>(state.ToString());
    }
}


