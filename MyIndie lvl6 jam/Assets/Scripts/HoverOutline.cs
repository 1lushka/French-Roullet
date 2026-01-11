using UnityEngine;

public class HoverOutline : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] LayerMask mask = ~0;
    [SerializeField] float maxDist = 100f;
    [SerializeField] string targetTag = "Shield";

    Behaviour current;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (!cam) return;

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        Behaviour next = null;

        if (Physics.Raycast(ray, out var hit, maxDist, mask) && hit.collider.CompareTag(targetTag))
            next = FindOutlineOn(hit.collider.transform);

        if (current == next) return;

        if (current) current.enabled = false;
        current = next;
        if (current) current.enabled = true;
    }

    static Behaviour FindOutlineOn(Transform t)
    {
        // Ищем в объекте и родителях любой Behaviour, чей тип содержит "Outline"
        // (подходит для Outline, QuickOutline, кастомных Outline-компонентов)
        var list = t.GetComponentsInParent<Behaviour>(true);
        for (int i = 0; i < list.Length; i++)
        {
            var b = list[i];
            if (!b) continue;
            var n = b.GetType().Name;
            if (n == "Outline" || n == "QuickOutline" || n.Contains("Outline"))
                return b;
        }
        return null;
    }

    void OnDisable()
    {
        if (current) current.enabled = false;
        current = null;
    }
}
