using UnityEngine;
using TMPro;
using UnityEngine.UI;

// I am not sure what else to do with the ammo count without a gun implemented

public class AmmoScript : MonoBehaviour
{
    public Potato_Shooter potatoShooter;
    public TextMeshProUGUI ammoText;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        potatoShooter = FindFirstObjectByType<Potato_Shooter>();
    }

    // Update is called once per frame
    void Update()
    {
        ammoText.text = potatoShooter.ammo + " / " + potatoShooter.initialAmmo;
    }
}
