using UnityEngine;

public class HoverOutline : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask mask = ~0;
    [SerializeField] private float maxDist = 100f;
    //[SerializeField] private string targetTag = "Untagged";

    private Outline currentOutline;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null) return;

        Outline next = null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, mask))
        {
            
                next = hit.collider.GetComponentInParent<Outline>();
            
        }

        // Если не изменилось — ничего не делаем
        if (currentOutline == next)
            return;

        // Выключаем старый
        if (currentOutline != null)
            currentOutline.enabled = false;

        currentOutline = next;

        // Включаем новый
        if (currentOutline != null)
            currentOutline.enabled = true;
    }

    private void OnDisable()
    {
        if (currentOutline != null)
            currentOutline.enabled = false;

        currentOutline = null;
    }
}
