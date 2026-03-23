using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string sceneToLoad;
    [Tooltip("Scene name with the kitchen.")]
    [SerializeField] private string cookingSceneName = "MainScene";

    [Tooltip("Scene name with the combat.")]
    [SerializeField] private string combatSceneName = "EnemyScene";
    void Start()
    {

    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Portal triggered by: " + other.name);
        if (other.CompareTag("Player"))
        {
            // Collider may be on a child of the player, so resolve from parent.
            Potato_Shooter shooter = other.GetComponentInParent<Potato_Shooter>();
            if (shooter == null)
            {
                Transform root = other.transform.root;
                if (root != null)
                    shooter = root.GetComponentInChildren<Potato_Shooter>(true); // include inactive
            }

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
