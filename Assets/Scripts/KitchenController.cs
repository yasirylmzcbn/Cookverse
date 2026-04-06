using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenController : MonoBehaviour, IInteractable
{
    [SerializeField] private SwitchCamera switchCamera;

    bool IInteractable.Interact()
    {
        if (switchCamera == null)
            switchCamera = FindFirstObjectByType<SwitchCamera>();

        if (switchCamera != null)
        {
            return switchCamera.SwitchToKitchenCamera(SwitchCamera.KitchenCameras.Stove);
        }
        return false;
    }

}
