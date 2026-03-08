using UnityEngine;
using DG.Tweening;

public class ShieldController : DragInteractable
{
    [Header("Wobble Settings")]
    [SerializeField] private float wobbleAngle = 7f;
    [SerializeField] private float wobblePeriod = 0.8f;
    [SerializeField] private Ease wobbleEase = Ease.InOutSine;

    [Header("Segments")]
    [SerializeField] private int shieldSegmentSize = 3;
    [SerializeField] private float segmentWidth = 1f;
    [SerializeField] private float ropeLeftX = -5f;
    [SerializeField] private int totalSegments = 10;

    protected override void StartHoldingAnimation(Transform target)
    {
        float half = Mathf.Max(0.01f, wobblePeriod * 0.5f);
        var baseEuler = originalLocalEuler;

        animTween?.Kill();
        animTween = DOTween.Sequence()
            .Append(target.DOLocalRotate(new Vector3(baseEuler.x + wobbleAngle, baseEuler.y, baseEuler.z), half).SetEase(wobbleEase))
            .Append(target.DOLocalRotate(new Vector3(baseEuler.x - wobbleAngle, baseEuler.y, baseEuler.z), half).SetEase(wobbleEase))
            .Append(target.DOLocalRotate(new Vector3(baseEuler.x, baseEuler.y, baseEuler.z), half).SetEase(wobbleEase))
            .SetLoops(-1, LoopType.Restart)
            //.OnUpdate(() => //ObjectMover.MoveTo(target, target.position))
            .SetLink(target.gameObject, LinkBehaviour.KillOnDestroy);
    }

    protected override void DragObject()
    {
        Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zOffset);
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

        float snappedX = SnapToSegments(worldPos.x);

        Vector3 target = new Vector3(snappedX, draggedObject.position.y, draggedObject.position.z);

        ObjectMover.MoveTo(draggedObject, target);
    }

    private float SnapToSegments(float mouseX)
    {
        float localX = mouseX - ropeLeftX;
        float segmentFloat = localX / segmentWidth;

        float snappedSegment;

        if (shieldSegmentSize % 2 == 1)
        {
            // нечётный щит → центр сегмента
            snappedSegment = Mathf.Round(segmentFloat);
        }
        else
        {
            // чётный → между сегментами
            snappedSegment = Mathf.Round(segmentFloat - 0.5f) + 0.5f;
        }

        float half = shieldSegmentSize * 0.5f;

        snappedSegment = Mathf.Clamp(
            snappedSegment,
            half,
            totalSegments - half
        );

        return ropeLeftX + snappedSegment * segmentWidth;
    }
}
