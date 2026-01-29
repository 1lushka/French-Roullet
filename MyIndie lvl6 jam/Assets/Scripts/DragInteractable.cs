using UnityEngine;
using DG.Tweening;
using Mirror;

public abstract class DragInteractable : NetworkBehaviour
{
    [Header("Setup")]
    [SerializeField] protected Camera cam;
    [SerializeField] protected string targetTag = "Untagged";

    [Header("X Clamp (drag)")]
    [SerializeField] protected float xMin = -5f, xMax = 5f;

    [Header("Hover")]
    [SerializeField] protected float hoverHeight = 1f;
    [SerializeField] protected float liftDuration = 0.2f;
    [SerializeField] protected Ease liftEase = Ease.OutQuad;

    [Header("Drop (external)")]
    [SerializeField] protected float externalHideOffsetY = 8f;
    [SerializeField] protected float externalDropDuration = 0.45f;
    [SerializeField] protected Ease externalDropEase = Ease.OutQuad;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip[] stackClip;
    [SerializeField] protected AudioClip[] takeClip;

    protected Transform draggedObject;
    protected float zOffset;
    protected float originalY;
    protected Vector3 originalLocalEuler;
    protected float _baseY;

    protected Tween liftTween;
    protected Tween animTween;

    protected virtual void Start()
    {
        if (cam == null)
            cam = Camera.main;

        _baseY = transform.position.y;
    }

    protected virtual void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetMouseButtonDown(0))
            TryPickObject();

        if (Input.GetMouseButtonUp(0))
            ReleaseObject();

        if (draggedObject != null && Input.GetMouseButton(0))
            DragObject();
    }

    private void TryPickObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag(targetTag))
            {
                draggedObject = hit.collider.transform;
                zOffset = cam.WorldToScreenPoint(draggedObject.position).z;

                originalY = draggedObject.position.y;
                originalLocalEuler = draggedObject.localEulerAngles;

                PlayRandomSoundFrom(takeClip);

                DOTween.Kill(draggedObject, false);

                liftTween?.Kill();
                liftTween = draggedObject
                    .DOMoveY(originalY + hoverHeight, liftDuration)
                    .SetEase(liftEase)
                    .SetLink(draggedObject.gameObject, LinkBehaviour.KillOnDestroy);
                    //.OnUpdate(() => ObjectMover.MoveTo(draggedObject, draggedObject.position));

                // вызываем абстрактный метод для разных типов объектов (щит/нож)
                StartHoldingAnimation(draggedObject);
            }
        }
    }

    private void ReleaseObject()
    {
        if (draggedObject == null) return;

        liftTween?.Kill();
        animTween?.Kill();

        draggedObject
            .DOMoveY(originalY, liftDuration)
            .SetEase(Ease.InQuad)
            //.OnUpdate(() => ObjectMover.MoveTo(draggedObject, draggedObject.position))
            .SetLink(draggedObject.gameObject, LinkBehaviour.KillOnDestroy)
            //.OnUpdate(() => ObjectMover.MoveTo(draggedObject, draggedObject.position));
            .OnComplete(() => ObjectMover.MoveTo(draggedObject, new Vector3(draggedObject.position.x, originalY, draggedObject.position.z)));
                              

        draggedObject
            .DOLocalRotate(originalLocalEuler, liftDuration)
            .SetEase(Ease.InOutSine)
            .SetLink(draggedObject.gameObject, LinkBehaviour.KillOnDestroy)
            .OnComplete(() => draggedObject = null);

        StartCoroutine(PlayRandomSound(stackClip));
        //ObjectMover.MoveTo(draggedObject, draggedObject.position);
        
    }

    protected virtual void DragObject()
    {
        Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zOffset);
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

        float clampedX = Mathf.Clamp(worldPos.x, xMin, xMax);
        Vector3 targetPos = new Vector3(clampedX, draggedObject.position.y, draggedObject.position.z);

        ObjectMover.MoveTo(draggedObject, targetPos);
    }

    public virtual void HideAbove()
    {
        var tr = transform;
        tr.DOKill();
        tr.position = new Vector3(tr.position.x, _baseY + externalHideOffsetY, tr.position.z);
    }

    protected void PlayRandomSoundFrom(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }

    protected System.Collections.IEnumerator PlayRandomSound(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0) yield break;
        yield return new WaitForSeconds(liftDuration);
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }

    private void OnDisable()
    {
        animTween?.Kill();
        liftTween?.Kill();
        if (draggedObject) DOTween.Kill(draggedObject, false);
    }

    protected abstract void StartHoldingAnimation(Transform target);
}
