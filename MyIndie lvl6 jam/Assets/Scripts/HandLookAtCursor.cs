using UnityEngine;

public class HandLookAtCursor : MonoBehaviour
{
    [Header("Основные настройки")]
    public Camera mainCamera;   
    public Transform hand;        
    public float distance = 10f;   

    [Header("Оффсет поворота")]
    public Vector3 rotationOffset; 

    void Update()
    {
        if (mainCamera == null || hand == null)
            return;

        Vector3 mousePos = Input.mousePosition;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));

        Vector3 direction = worldPos - hand.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);

            hand.rotation = lookRot * Quaternion.Euler(rotationOffset);
        }
    }
}
