using UnityEngine;

public class MinimapCameraLimits : MonoBehaviour
{
    public static MinimapCameraLimits Instance { get; private set; }

    public GameObject player;
    public float yLimit;

    private void Awake()
    {
        // Ensure only one minimap camera exists across scene loads
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate MinimapCamera detected. Keeping '{Instance.gameObject.name}', destroying '{gameObject.name}'.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        transform.position = new Vector3(player.transform.position.x, yLimit, player.transform.position.z);
    }
}