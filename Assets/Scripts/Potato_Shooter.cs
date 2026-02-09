using UnityEngine;

public class Potato_Shooter : MonoBehaviour
{

    public GameObject Bullet;
    public Transform Shoot_Pos;

    [Header("Aiming")]
    [Tooltip("Optional override. If empty, uses the currently active camera from SwitchCamera, else Camera.main.")]
    [SerializeField] private Transform aimReference;

    [Tooltip("If true, aim is computed by raycasting from the camera forward and shooting towards the hit point. Helps in third-person.")]
    [SerializeField] private bool useCameraRaycastAim = true;

    [SerializeField] private float aimMaxDistance = 200f;

    [Tooltip("Layers the aim ray can hit.")]
    [SerializeField] private LayerMask aimLayers = ~0;

    private SwitchCamera _switchCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _switchCamera = FindFirstObjectByType<SwitchCamera>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Shoot()
    {
        if (Bullet == null || Shoot_Pos == null) return;

        Transform aim = GetAimTransform();
        Vector3 direction = GetAimDirection(aim);

        if (direction.sqrMagnitude < 0.0001f)
            direction = transform.forward;

        Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Instantiate(Bullet, Shoot_Pos.position, rotation);
    }

    private Transform GetAimTransform()
    {
        if (aimReference != null)
            return aimReference;

        if (_switchCamera == null)
            _switchCamera = FindFirstObjectByType<SwitchCamera>();

        if (_switchCamera != null)
        {
            if (_switchCamera.kitchenCamera != null && _switchCamera.kitchenCamera.activeInHierarchy)
                return _switchCamera.kitchenCamera.transform;

            if (_switchCamera.thirdPersonCamera != null && _switchCamera.thirdPersonCamera.activeInHierarchy)
                return _switchCamera.thirdPersonCamera.transform;

            if (_switchCamera.firstPersonCamera != null && _switchCamera.firstPersonCamera.activeInHierarchy)
                return _switchCamera.firstPersonCamera.transform;
        }

        return Camera.main != null ? Camera.main.transform : transform;
    }

    private Vector3 GetAimDirection(Transform aim)
    {
        if (!useCameraRaycastAim || aim == null)
            return aim != null ? aim.forward : transform.forward;

        Ray ray = new Ray(aim.position, aim.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, aimMaxDistance, aimLayers, QueryTriggerInteraction.Ignore))
        {
            Vector3 toHit = hit.point - Shoot_Pos.position;
            return toHit.sqrMagnitude > 0.0001f ? toHit.normalized : aim.forward;
        }

        return aim.forward;
    }
}
