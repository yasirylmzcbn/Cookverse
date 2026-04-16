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
}