using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarScript : MonoBehaviour
{
    PlayerController playerController;
    public int maxHealth, currHealth;
    public Slider healthBarSlider;
    public TextMeshProUGUI healthText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        healthBarSlider.maxValue = playerController.maxHealth;
        healthBarSlider.value = playerController.currentHealth;
        healthText.text = playerController.currentHealth + " / " + playerController.maxHealth;
    }
}
