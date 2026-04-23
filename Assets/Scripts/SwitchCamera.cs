using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;

public class SwitchCamera : MonoBehaviour
{
    private static SwitchCamera _instance;

    public GameObject firstPersonCamera;
    public GameObject thirdPersonCamera;
    public GameObject kitchenCamera;
    public enum KitchenCameras { Stove, Oven, Sink, Fryer, Microwave, }
    private Dictionary<KitchenCameras, GameObject> cameras;
    public KitchenCameras currentKitchenCamera;
    bool kitchenCam = false;
    bool firstPerson = true;
    [SerializeField] private GameObject hotbarUIPanel;

    private static bool IsAlive(GameObject go) => go != null;

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (IsAlive(go))
            go.SetActive(active);
    }

    public bool IsInKitchenCamera => kitchenCam;
    public bool IsInkitchenCamera => kitchenCam && currentKitchenCamera == KitchenCameras.Stove;

    public static bool IsKitchenInteractionAllowed()
    {
        if (_instance == null)
            _instance = FindFirstObjectByType<SwitchCamera>();

        return _instance != null
               && _instance.kitchenCam
               && Cursor.visible
               && Cursor.lockState == CursorLockMode.None;
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    void Start()
    {
        SetActiveSafe(firstPersonCamera, true);
        SetActiveSafe(thirdPersonCamera, false);
        SetActiveSafe(kitchenCamera, false);
        currentKitchenCamera = KitchenCameras.Stove;
        cameras = new Dictionary<KitchenCameras, GameObject>()
        {
            { KitchenCameras.Stove, kitchenCamera },
            // TODO Add other kitchen cameras here when implemented
        };
        EnsureActiveAudioListener();
    }

    private bool TryGetKitchenCamera(KitchenCameras cam, out GameObject target)
    {
        target = null;

        if (cameras == null)
        {
            cameras = new Dictionary<KitchenCameras, GameObject>()
            {
                { KitchenCameras.Stove, kitchenCamera },
            };
        }

        if (!cameras.TryGetValue(cam, out target) || !IsAlive(target))
        {
            // Keep Stove in sync in case scene references changed.
            if (cam == KitchenCameras.Stove && IsAlive(kitchenCamera))
            {
                cameras[KitchenCameras.Stove] = kitchenCamera;
                target = kitchenCamera;
            }
        }

        return IsAlive(target);
    }

    void Update()
    {
        // if (Keyboard.current.vKey.wasPressedThisFrame)
        // {
        //     CycleCameraMode();
        // }
    }

    public void CycleCameraMode()
    {
        if (!kitchenCam)
        {
            if (firstPerson)
            {
                SetActiveSafe(firstPersonCamera, false);
                SetActiveSafe(thirdPersonCamera, true);
                firstPerson = false;
            }
            else
            {
                SetActiveSafe(thirdPersonCamera, false);
                SetActiveSafe(firstPersonCamera, true);
                firstPerson = true;
            }

            EnsureActiveAudioListener();
        }
    }

    public bool SwitchToKitchenCamera(KitchenCameras cam)
    {
        if (!TryGetKitchenCamera(cam, out GameObject targetKitchenCam))
            return false;

        if (PlayerController.Instance != null)
            PlayerController.Instance.SetBodyVisible(false);

        kitchenCam = true;
        SetActiveSafe(firstPersonCamera, false);
        SetActiveSafe(thirdPersonCamera, false);
        SetActiveSafe(targetKitchenCam, true);
        currentKitchenCamera = cam;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EnsureActiveAudioListener();
        return true;
    }

    public void ExitKitchenCamera()
    {
        Debug.Log("Exiting kitchen camera");

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetBodyVisible(true);
            PlayerController.Instance.EndInteraction();
        }

        kitchenCam = false;
        SetActiveSafe(kitchenCamera, false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SpellMenuUI spellMenu = FindFirstObjectByType<SpellMenuUI>(FindObjectsInactive.Include);
        if (spellMenu != null && spellMenu.menuOpen)
            spellMenu.SetMenuVisible(false);

        if (firstPerson)
        {
            SetActiveSafe(firstPersonCamera, true);

            CameraController fpController = firstPersonCamera != null
                ? firstPersonCamera.GetComponentInChildren<CameraController>(true)
                : null;

            if (fpController != null)
                fpController.enabled = true;
        }
        else
        {
            SetActiveSafe(thirdPersonCamera, true);
        }

        CameraMove tpController = thirdPersonCamera != null
            ? thirdPersonCamera.GetComponentInChildren<CameraMove>(true)
            : null;

        if (tpController != null)
            tpController.enabled = !firstPerson;

        EnsureActiveAudioListener();
    }

    private void EnsureActiveAudioListener()
    {
        Camera activeCamera = GetActiveGameplayCamera();
        if (activeCamera == null)
            return;

        AudioListener current = FindFirstObjectByType<AudioListener>(FindObjectsInactive.Include);
        if (current != null && current.gameObject == activeCamera.gameObject)
        {
            current.enabled = true;
            return;
        }

        AudioListener activeListener = activeCamera.GetComponent<AudioListener>();
        if (activeListener == null)
            activeListener = activeCamera.gameObject.AddComponent<AudioListener>();

        activeListener.enabled = true;

        AudioListener[] allListeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (AudioListener listener in allListeners)
        {
            if (listener != null && listener != activeListener)
                listener.enabled = false;
        }
    }

    private Camera GetActiveGameplayCamera()
    {
        if (kitchenCam)
        {
            Camera kitchenCamComponent = kitchenCamera != null ? kitchenCamera.GetComponentInChildren<Camera>(true) : null;
            if (kitchenCamComponent != null)
                return kitchenCamComponent;
        }

        if (firstPerson)
        {
            Camera fpCamera = firstPersonCamera != null ? firstPersonCamera.GetComponentInChildren<Camera>(true) : null;
            if (fpCamera != null)
                return fpCamera;
        }
        else
        {
            Camera tpCamera = thirdPersonCamera != null ? thirdPersonCamera.GetComponentInChildren<Camera>(true) : null;
            if (tpCamera != null)
                return tpCamera;
        }

        return Camera.main;
    }
}
