using UnityEngine.InputSystem;
using UnityEngine;

public class SwitchCamera : MonoBehaviour
{
    public GameObject firstPersonCamera;
    public GameObject thirdPersonCamera;
    public GameObject stoveCamera;
    // TODO add more kitchen cameras once models are created
    public enum KitchenCameras { Stove, Oven, Sink, Fryer, Microwave, }
    public KitchenCameras currentKitchenCamera;
    bool kitchenCam = false;
    bool firstPerson = true;

    void Start()
    {
        firstPersonCamera.SetActive(true);
        thirdPersonCamera.SetActive(false);
        stoveCamera.SetActive(false);
        currentKitchenCamera = KitchenCameras.Stove;
    }

    void Update()
    {
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            CycleCameraMode();
        }
    }

    public void CycleCameraMode()
    {
        if (!kitchenCam)
        {
            if (firstPerson)
            {
                firstPersonCamera.SetActive(false);
                thirdPersonCamera.SetActive(true);
                firstPerson = false;
            }
            else
            {
                thirdPersonCamera.SetActive(false);
                firstPersonCamera.SetActive(true);
                firstPerson = true;
            }
        }
    }
    public void SwitchToStoveCamera()
    {
        kitchenCam = true;
        firstPersonCamera.SetActive(false);
        thirdPersonCamera.SetActive(false);
        stoveCamera.SetActive(true);
        currentKitchenCamera = KitchenCameras.Stove;
    }

    public void SwitchToOvenCamera()
    {
        kitchenCam = true;
        firstPersonCamera.SetActive(false);
        thirdPersonCamera.SetActive(false);
        stoveCamera.SetActive(false);
        // ovenCamera.SetActive(true);
        currentKitchenCamera = KitchenCameras.Oven;
    }

    public void ExitKitchenCamera()
    {
        kitchenCam = false;
        stoveCamera.SetActive(false);
        if (firstPerson)
        {
            firstPersonCamera.SetActive(true);
        }
        else
        {
            thirdPersonCamera.SetActive(true);
        }
    }
}
