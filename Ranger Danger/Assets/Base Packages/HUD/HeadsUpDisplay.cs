using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
    public Slider staminaBar;
    public GameObject player;

    private float maxStamina;
    private float currentStamina;

    // Update is called once per frame
    void Update()
    {
        maxStamina = player.GetComponent<Player>().maximumStamina;
        currentStamina = player.GetComponent<Player>().availableStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = currentStamina;
    }
}
