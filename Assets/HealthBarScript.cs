using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarScript : MonoBehaviour
{
    
    public int maxHealth, currHealth;
    public Slider healthBarSlider;
    public TextMeshProUGUI healthText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = currHealth;
        healthText.text = currHealth + " / " + maxHealth;
    }
}
