using UnityEngine;
using UnityEngine.Events;

public class Slider3D : MonoBehaviour
{
    [Header("Slider Settings")]
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 2f;
    [SerializeField] private float sensitivity = 1f; 

    [Header("Value Settings")]
    [SerializeField, Range(0f, 1f)] private float value = 0f; 
    public UnityEvent<float> onValueChanged;

    private Camera mainCamera;
    private bool isDragging = false;
    private float dragOffset;

    void Start()
    {
        mainCamera = Camera.main;
        UpdatePositionFromValue();
        onValueChanged?.Invoke(value);
    }

    void OnMouseDown()
    {
        isDragging = true;

        Plane plane = new Plane(Vector3.forward, transform.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            dragOffset = transform.position.y - hitPoint.y;
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Plane plane = new Plane(Vector3.forward, transform.position);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            float targetY = hitPoint.y + dragOffset;

            targetY = Mathf.Clamp(targetY, minY, maxY);

            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

            float newValue = Mathf.InverseLerp(minY, maxY, targetY);

            if (!Mathf.Approximately(newValue, value))
            {
                value = newValue;
                onValueChanged?.Invoke(value);
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    private void UpdatePositionFromValue()
    {
        float yPos = Mathf.Lerp(minY, maxY, value);
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
    }

    public void SetValue(float newValue)
    {
        value = Mathf.Clamp01(newValue);
        UpdatePositionFromValue();
        onValueChanged?.Invoke(value);
    }

    public float GetValue() => value;
}
