using UnityEngine;

public class DifficultySetter : MonoBehaviour, IInteractable
{
    [SerializeField] private DifficultyUI difficultyUI;

    public bool Interact()
    {
        if (difficultyUI != null)
        {
            if (difficultyUI.IsVisible())
            {
                return false;
            }
            difficultyUI.SetMenuVisible(true);
            return true;
        }
        return false;
    }

    private void OnGUI()
    {
        if (Time.timeScale == 0f) return;

        if (PlayerController.Instance == null || PlayerController.Instance.InteractorSource == null) return;

        if (difficultyUI != null && difficultyUI.IsVisible()) return;

        bool canInteract = false;
        Ray r = new Ray(PlayerController.Instance.InteractorSource.position, PlayerController.Instance.InteractorSource.forward);
        if (Physics.Raycast(r, out RaycastHit hit, PlayerController.Instance.InteractDistance))
        {
            IInteractable interactable = null;
            hit.collider.gameObject.TryGetComponent(out interactable);

            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable == null && hit.rigidbody != null)
                interactable = hit.rigidbody.GetComponentInParent<IInteractable>();

            if (interactable != null && interactable as Component != null && (interactable as Component).gameObject == gameObject)
            {
                canInteract = true;
            }
        }

        if (canInteract)
        {
            Color originalColor = GUI.color;
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 36,
                fontStyle = FontStyle.Bold
            };

            GUI.Label(new Rect(0, Screen.height * 0.7f, Screen.width, 100f), "Press E to change the difficulty", style);
            GUI.color = originalColor;
        }
    }
}