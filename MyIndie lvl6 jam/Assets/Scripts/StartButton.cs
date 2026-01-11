using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class StartButton : MonoBehaviour
{
    [SerializeField] private UnityEvent onClick;
    [SerializeField] private float pressDepth = 0.1f;
    [SerializeField] private float pressDuration = 0.15f;

    private bool isPressed = false;
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void OnMouseDown()
    {
        if (isPressed) return;
        isPressed = true;

        transform.DOLocalMoveY(originalPosition.y - pressDepth, pressDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                onClick?.Invoke();
                transform.DOLocalMoveY(originalPosition.y, pressDuration).SetEase(Ease.InQuad);
            });
    }
}
