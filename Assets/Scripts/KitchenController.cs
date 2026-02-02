using UnityEngine;

public class KitchenController : MonoBehaviour, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private SwitchCamera switchCamera;
    void Start()
    {
        
    }

    bool IInteractable.Interact()
    {
        if (switchCamera != null)
        {
            switchCamera.SwitchToKitchenCamera(SwitchCamera.KitchenCameras.Stove);
            return true;
        }
        return false;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
