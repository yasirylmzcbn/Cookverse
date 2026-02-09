using UnityEngine;
using System.Collections;

public class Potato_Shooter : MonoBehaviour
{

    public GameObject Bullet;
    public Transform Shoot_Pos;
    public int initialAmmo;
    public float shootCooldownDuration;
    public float reloadDuration = 2f;
    private int ammo;
    private Coroutine reloadCoroutine;
    private Coroutine shootCooldownCoroutine;
    enum WeaponState
    {
        Ready,
        Cooldown, //async op to wait for the cooldown and then set it to ready or empty
        Reloading,
        Empty
    }
    private WeaponState state;
    private void Awake()
    {
        ammo = initialAmmo;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Shoot()
    {
        if (state == WeaponState.Cooldown || state == WeaponState.Reloading)
        {
            return;
        }
        if (ammo <= 0)
        {
            state = WeaponState.Empty;
        }
        if (state == WeaponState.Empty)
        {
            state = WeaponState.Empty;
            // Not enough ammo, make a click sound in final game and add coroutine to change color in grayboxing
            return;

        }
        Vector3 direction = (Shoot_Pos.position - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Instantiate(Bullet, Shoot_Pos.position, rotation);
        ammo -= 1;
    }

    public void TryReload()
    {
        if (state == WeaponState.Reloading)
        {
            return;
        }

        if (reloadCoroutine != null)
        {
            return;
        }

        reloadCoroutine = StartCoroutine(ReloadRoutine());
        state = WeaponState.Reloading;
    }

    IEnumerator ReloadRoutine()
    {
        //animation
        yield return new WaitForSeconds(reloadDuration);
        ammo = initialAmmo;
        state = WeaponState.Ready;
        reloadCoroutine = null;
    }

    public void WaitCooldown()
    {
        if (state == WeaponState.Cooldown)
        {
            return;
        }

        if (shootCooldownCoroutine != null)
        {
            return;
        }

        shootCooldownCoroutine = StartCoroutine(WaitCooldownRoutine());
        state = WeaponState.Cooldown;
    }

    IEnumerator WaitCooldownRoutine()
    {
        yield return new WaitForSeconds(shootCooldownDuration);
        state = WeaponState.Ready;
        shootCooldownCoroutine = null;
    }
}
