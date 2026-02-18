using UnityEngine;

public class MinimapCameraLimits : MonoBehaviour
{
    
    public GameObject player;
    public float yLimit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, yLimit, player.transform.position.z);
    }
}
