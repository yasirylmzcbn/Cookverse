using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    private PlayerController _playerController;
    private Potato_Shooter _potatoShooter;

    [Header("Health UI")]
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Ammo UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Quests UI")]
    [SerializeField] private TextMeshProUGUI questText;

    private readonly List<string> _quests = new List<string>();

    void Start()
    {
        _playerController = playerController;

        if (_playerController != null)
            _potatoShooter = _playerController.GetComponentInChildren<Potato_Shooter>();

        _quests.Add("End Waddle Quackdonald's entire career");
        _quests.Add("Collect exotic meats from Marinara Trench");
        _quests.Add("Find the star");
    }

    void Update()
    {
        if (_playerController != null)
        {
            if (healthBarSlider != null)
            {
                healthBarSlider.maxValue = _playerController.maxHealth;
                healthBarSlider.value = _playerController.currentHealth;
            }

            if (healthText != null)
                healthText.text = _playerController.currentHealth + " / " + _playerController.maxHealth;
        }

        if (_potatoShooter != null && ammoText != null)
        {
            ammoText.text = _potatoShooter.ammo + " / " + _potatoShooter.initialAmmo;

            // more transparent text during reload
            var c = ammoText.color;
            c.a = (_potatoShooter.ammo <= 0 || _potatoShooter.IsReloading) ? 0.3f : 1f;
            ammoText.color = c;
        }

        if (questText != null)
            questText.text = "> " + string.Join("\n> ", _quests);
    }
}
