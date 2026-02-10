using UnityEngine;
using UnityEngine.UI;

public class SpellUIScript : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;

    [Header("Spell UI Images")]
    public Image spell1Image;
    public Image spell2Image;
    public Image spell3Image;
    public Image spell4Image;

    [Header("Transparency Settings")]
    [Range(0f, 1f)]
    public float transparencyAmount = 0.5f; // 50% transparent

    private Color originalColor;

    void Start()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        if (spell1Image != null)
        {
            originalColor = spell1Image.color;
        }
    }

    void Update()
    {
        UpdateSpellTransparency(spell1Image, 1);
        UpdateSpellTransparency(spell2Image, 2);
        UpdateSpellTransparency(spell3Image, 3);
        UpdateSpellTransparency(spell4Image, 4);
    }

    private void UpdateSpellTransparency(Image spellImage, int spellNumber)
    {

        bool isOnCooldown = false;

        // read the actual cooldown status from PlayerController
        switch (spellNumber)
        {
            case 1: isOnCooldown = playerController.IsSpell1OnCooldown; break;
            /* these spells are not implemented yet.
            case 2: isOnCooldown = playerController.IsSpell2OnCooldown; break;
            case 3: isOnCooldown = playerController.IsSpell3OnCooldown; break;
            case 4: isOnCooldown = playerController.IsSpell4OnCooldown; break;
            */
        }

        if (isOnCooldown)
        {
            // spell on cooldown, so transparent
            Color transparentColor = originalColor;
            transparentColor.a = transparencyAmount;
            spellImage.color = transparentColor;
        }
        else
        {
            // spell is ready, so opaque
            Color readyColor = originalColor;
            readyColor.a = 1f;
            spellImage.color = readyColor;
        }
    }
}