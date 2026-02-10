using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpellUIScript : MonoBehaviour
{
    [Header("Spell UI Images")]
    public Image spell1Image;
    public Image spell2Image;
    public Image spell3Image;
    public Image spell4Image;

    [Header("Transparency Settings")]
    [Range(0f, 1f)]
    public float transparencyAmount = 0.5f; // 50% transparent
    public float transparencyDuration = 20f; // how long it stays transparent, hard-coded to 20 to match the spell cooldown in PlayerController.cs for now

    private Color originalColor;

    void Start()
    {
        originalColor = spell1Image.color;
    }

    void Update()
    {
        // check keyboard input
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            StartCoroutine(FlashTransparency(spell1Image));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            StartCoroutine(FlashTransparency(spell2Image));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            StartCoroutine(FlashTransparency(spell3Image));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            StartCoroutine(FlashTransparency(spell4Image));
        }
    }

    IEnumerator FlashTransparency(Image spellImage)
    {
        Color currentColor = spellImage.color;

        Color transparentColor = currentColor;
        transparentColor.a = transparencyAmount;
        spellImage.color = transparentColor;

        yield return new WaitForSeconds(transparencyDuration);

        transparentColor.a = originalColor.a;
        spellImage.color = transparentColor;
    }
}
