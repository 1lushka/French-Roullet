using UnityEngine;
using DG.Tweening;

public class ShieldController : DragInteractable
{
    [Header("Wobble Settings")]
    [SerializeField] private float wobbleAngle = 7f;
    [SerializeField] private float wobblePeriod = 0.8f;
    [SerializeField] private Ease wobbleEase = Ease.InOutSine;

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
}
