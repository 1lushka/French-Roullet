using UnityEngine;
using Mirror;

public class RopesAttachOnDeath : NetworkBehaviour
{
    [Header("Менеджер здоровья верёвок")]
    [SerializeField] private RopesHealthManager manager;

    [Header("Куда прикреплять")]
    [SerializeField] private Transform leftAnchor;
    [SerializeField] private Transform rightAnchor;

    [Header("Как найти верёвки")]
    [SerializeField] private Transform ropesRoot;
    [SerializeField] private TapePiece[] ropes;

    [Header("Настройки прикрепления")]
    [SerializeField] private bool worldPositionStays = true;
    [SerializeField] private bool equalGoesRight = false;

    private bool _done;

    [SerializeField] private Vector2 postZeroDelayRange = new Vector2(0.4f, 1.2f);
    [SerializeField] private float leftMoveSpeed = 2.5f;

    [Header("Анимация смерти")]
    [SerializeField] private Animator deathAnimator;
    [SerializeField] private string deathTrigger = "Death";

    [Header("Аудио")]
    [SerializeField] private AudioSource DeathAudioSource;
    [SerializeField] private AudioClip giliotina;
    [SerializeField] private AudioClip headCut;

    private bool _moveLeftActive;

    // ------------------------------------------------------------
    //                     ЗВУКОВОЙ РЕЕСТР
    // ------------------------------------------------------------
    private enum SoundID : byte
    {
        HeadCut = 1,
        Giliotina = 2
    }

    private AudioClip GetClipByID(SoundID id)
    {
        switch (id)
        {
            case SoundID.HeadCut: return headCut;
            case SoundID.Giliotina: return giliotina;
            default: return null;
        }
    }

    // ------------------------------------------------------------
    private void Awake()
    {
        if (manager == null)
            manager = FindFirstObjectByType<RopesHealthManager>();

        if (ropes == null || ropes.Length == 0)
        {
            if (ropesRoot != null)
                ropes = ropesRoot.GetComponentsInChildren<TapePiece>(false);
            else
                ropes = FindObjectsByType<TapePiece>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }
    }

    private void OnEnable()
    {
        if (manager != null)
            manager.OnFirstHit0 += HandleFirstZeroServer;
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.OnFirstHit0 -= HandleFirstZeroServer;
    }

    // ------------------------------------------------------------
    //            СРАБОТАЛ НУЛЕВОЙ ПОРОГ (ТОЛЬКО СЕРВЕР)
    // ------------------------------------------------------------
    [Server]
    private void HandleFirstZeroServer(TapePiece destroyed)
    {
        if (!isServer) return;
        if (_done || destroyed == null || leftAnchor == null || rightAnchor == null) return;

        _done = true;

        float pivotX = destroyed.transform.position.x;

        foreach (var r in ropes)
        {
            if (r == null || r == destroyed) continue;

            float x = r.transform.position.x;

            bool goRight = x > pivotX || (Mathf.Approximately(x, pivotX) && equalGoesRight);
            var targetParent = goRight ? rightAnchor : leftAnchor;

            r.transform.SetParent(targetParent, worldPositionStays);
        }

        StartCoroutine(PostZeroSequenceServer(destroyed));
    }

    // ------------------------------------------------------------
    //                ПОСЛЕДОВАТЕЛЬНОСТЬ ПОСЛЕ НУЛЯ
    // ------------------------------------------------------------
    [Server]
    private System.Collections.IEnumerator PostZeroSequenceServer(TapePiece destroyed)
    {
        // RPC: звук 1
        RpcPlayOneShot(SoundID.HeadCut);

        float delay = Random.Range(postZeroDelayRange.x, postZeroDelayRange.y);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // Удаляем уничтоженную верёвку
        if (destroyed != null)
            NetworkServer.Destroy(destroyed.gameObject);

        // Начинаем движение
        _moveLeftActive = true;

        // RPC: звук 2
        RpcPlayOneShot(SoundID.Giliotina);

        // RPC: анимация смерти
        if (!string.IsNullOrEmpty(deathTrigger))
            RpcSetAnimatorTrigger(deathTrigger);
    }

    // ------------------------------------------------------------
    //                   RPC АУДИО / АНИМАЦИИ
    // ------------------------------------------------------------
    [ClientRpc]
    private void RpcPlayOneShot(SoundID clipID)
    {
        var clip = GetClipByID(clipID);
        if (DeathAudioSource != null && clip != null)
            DeathAudioSource.PlayOneShot(clip);
    }

    [ClientRpc]
    private void RpcSetAnimatorTrigger(string trigger)
    {
        if (deathAnimator != null)
            deathAnimator.SetTrigger(trigger);
    }

    // ------------------------------------------------------------
    //              ДВИЖЕНИЕ ВЛЕВО (ТОЛЬКО СЕРВЕР)
    // ------------------------------------------------------------
    private void Update()
    {
        if (!isServer) return;

        if (_moveLeftActive && leftAnchor != null && leftMoveSpeed > 0f)
            leftAnchor.Translate(Vector3.left * leftMoveSpeed * Time.deltaTime, Space.World);
    }
}
