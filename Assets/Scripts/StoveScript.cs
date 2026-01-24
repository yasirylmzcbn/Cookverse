using UnityEngine;

public class StoveScript : MonoBehaviour, IInteractable
{
    SwitchCamera switchCamera;
    void Start()
    {
        switchCamera = FindFirstObjectByType<SwitchCamera>();
    }
    bool IInteractable.Interact()
    {
        if (switchCamera != null)
        {
            switchCamera.SwitchToStoveCamera();
            return true;
        }
        return false;
    }
}
