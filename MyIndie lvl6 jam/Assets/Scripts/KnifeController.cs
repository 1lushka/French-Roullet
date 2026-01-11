using UnityEngine;
using DG.Tweening;

public class KnifeController : DragInteractable
{
    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 180f;

    protected override void StartHoldingAnimation(Transform target)
    {
        //animTween?.Kill();
        //animTween = target
        //    .DOLocalRotate(new Vector3(0, 360, 0), 360f / spinSpeed, RotateMode.LocalAxisAdd)
        //    .SetEase(Ease.Linear)
        //    .SetLoops(-1, LoopType.Restart)
        //    .OnUpdate(() => ObjectMover.MoveTo(target, target.position))
        //    .SetLink(target.gameObject, LinkBehaviour.KillOnDestroy);
    }
}
