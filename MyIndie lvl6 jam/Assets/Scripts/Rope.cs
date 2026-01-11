using UnityEngine;
using UnityEngine.Events;

public class Rope : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onHoverStart;
    public UnityEvent onHoverEnd;
    public UnityEvent onPull;

    private bool isHovered = false;

    // Эти методы вызываются Raycaster'ом
    public void SetHovered(bool hovered)
    {
        if (hovered && !isHovered)
        {
            isHovered = true;
            onHoverStart?.Invoke();
        }
        else if (!hovered && isHovered)
        {
            isHovered = false;
            onHoverEnd?.Invoke();
        }
    }

    public void Pull()
    {
        if (isHovered)
            onPull?.Invoke();
    }
}
