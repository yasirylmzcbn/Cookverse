using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class Potato_Shooter : MonoBehaviour
{

    public GameObject Bullet;
    public Transform Shoot_Pos;
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
        Vector3 direction = (Shoot_Pos.position - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Instantiate(Bullet, Shoot_Pos.position, rotation);
    }
}
