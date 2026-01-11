using UnityEngine;

public class RopeRaycaster : MonoBehaviour
{
    [SerializeField] Camera _camera;
    private Rope currentRope;

    void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Rope hitRope = null;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hitRope = hit.collider.GetComponent<Rope>();
        }

        if (hitRope != currentRope)
        {
            if (currentRope != null)
                currentRope.SetHovered(false);

            currentRope = hitRope;

            if (currentRope != null)
                currentRope.SetHovered(true);
        }

        if (Input.GetMouseButtonDown(0) && currentRope != null)
        {
            currentRope.Pull();
        }
    }
}
