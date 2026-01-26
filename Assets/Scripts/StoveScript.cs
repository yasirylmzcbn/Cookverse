using UnityEngine;

public class StoveScript : MonoBehaviour, IInteractable
{
    public enum BurnerSide { Left, Right }

    [Header("Burner Visuals (Optional)")]
    [SerializeField] private GameObject leftBurnerOnVisual;
    [SerializeField] private GameObject rightBurnerOnVisual;

    [Header("Burner Renderers (Optional)")]
    [SerializeField] private Renderer leftBurnerRenderer;
    [SerializeField] private Renderer rightBurnerRenderer;
    [SerializeField] private Color burnerOffColor = Color.black;
    [SerializeField] private Color burnerOnColor = Color.red;
    [Tooltip("Material color property name. '_BaseColor' (URP/HDRP) or '_Color' (Built-in).")]
    [SerializeField] private string colorProperty = "_BaseColor";

    [Header("Burner Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float onThreshold = 0.15f;

    [SerializeField, Range(0f, 1f)] private float leftBurnerLevel;
    [SerializeField, Range(0f, 1f)] private float rightBurnerLevel;

    SwitchCamera switchCamera;
    void Start()
    {
        switchCamera = FindFirstObjectByType<SwitchCamera>();
        ApplyVisuals();
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

    public float GetBurnerLevel(BurnerSide side)
    {
        return side == BurnerSide.Left ? leftBurnerLevel : rightBurnerLevel;
    }

    public bool IsBurnerOn(BurnerSide side)
    {
        return GetBurnerLevel(side) >= onThreshold;
    }

    public void SetBurnerLevel(BurnerSide side, float level01)
    {
        level01 = Mathf.Clamp01(level01);
        if (side == BurnerSide.Left)
            leftBurnerLevel = level01;
        else
            rightBurnerLevel = level01;

        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        if (leftBurnerOnVisual != null)
            leftBurnerOnVisual.SetActive(IsBurnerOn(BurnerSide.Left));

        if (rightBurnerOnVisual != null)
            rightBurnerOnVisual.SetActive(IsBurnerOn(BurnerSide.Right));

        ApplyBurnerColor(leftBurnerRenderer, leftBurnerLevel);
        ApplyBurnerColor(rightBurnerRenderer, rightBurnerLevel);
    }

    private void ApplyBurnerColor(Renderer rend, float level01)
    {
        if (rend == null) return;

        // Smooth a bit so the first part of rotation stays dark.
        float t = Mathf.InverseLerp(onThreshold, 1f, Mathf.Clamp01(level01));
        Color c = Color.Lerp(burnerOffColor, burnerOnColor, t);

        var mat = rend.material;
        if (mat == null) return;

        if (!string.IsNullOrWhiteSpace(colorProperty) && mat.HasProperty(colorProperty))
            mat.SetColor(colorProperty, c);
        else if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", c);
        else if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", c);
    }
}
