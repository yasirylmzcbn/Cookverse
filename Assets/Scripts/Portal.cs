using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    private enum ReturnOffsetDirection
    {
        Forward,
        Backward,
    }

    public string sceneToLoad;
    [Tooltip("Scene name with the kitchen.")]
    [SerializeField] private string cookingSceneName = "MainScene";

    [Tooltip("Scene name with the combat.")]
    [SerializeField] private string combatSceneName = "EnemyScene";

    [Header("Return Spawn Offset")]
    [Tooltip("How far to offset the saved return position so the player does not spawn inside this portal trigger.")]
    [SerializeField, Min(0f)] private float returnSpawnOffset = 2f;

    [Tooltip("Which direction to offset relative to this portal's forward vector.")]
    [SerializeField] private ReturnOffsetDirection returnOffsetDirection = ReturnOffsetDirection.Forward;

    private Vector3 GetSafeReturnOffset()
    {
        Vector3 direction = returnOffsetDirection == ReturnOffsetDirection.Forward ? transform.forward : -transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.forward;

        return direction.normalized * returnSpawnOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Portal triggered by: " + other.name);
        if (other.CompareTag("Player"))
        {
            Transform playerTransform = null;

            // Collider may be on a child of the player, so resolve from parent.
            PlayerController playerController = other.GetComponentInParent<PlayerController>();
            if (playerController != null)
            {
                if (sceneToLoad == cookingSceneName)
                    playerController.DisableCombat();
                else if (sceneToLoad == combatSceneName)
                    playerController.EnableCombat();

            }
            Potato_Shooter shooter = other.GetComponentInParent<Potato_Shooter>();
            if (shooter == null)
            {
                Transform root = other.transform.root;
                if (root != null)
                    shooter = root.GetComponentInChildren<Potato_Shooter>(true); // include inactive
            }

            playerTransform = shooter != null ? shooter.transform.root : other.transform.root;
            if (playerTransform == null)
                playerTransform = other.transform;

            Scene activeScene = SceneManager.GetActiveScene();
            Vector3 safeReturnPosition = playerTransform.position + GetSafeReturnOffset();
            PlayerController.RememberPositionForScene(activeScene.name, safeReturnPosition, playerTransform.rotation);

            if (shooter != null)
            {
                Debug.Log("Shooter enabled");
                if (!string.IsNullOrEmpty(sceneToLoad) && sceneToLoad == cookingSceneName)
                    shooter.gameObject.SetActive(false);
                else if (!string.IsNullOrEmpty(sceneToLoad) && sceneToLoad == combatSceneName)
                    shooter.gameObject.SetActive(true);
            }
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
