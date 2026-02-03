using UnityEngine;
using Mirror;
using System.Collections;

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

    [SerializeField] private Vector2 postZeroDelayRange = new Vector2(0.4f, 0.5f);
    [SerializeField] private float leftMoveSpeed = 2.5f;

    [Header("Анимация смерти")]
    [SerializeField] private Animator deathAnimator;
    [SerializeField] private string deathTrigger = "Death";

    [Header("Аудио")]
    [SerializeField] private AudioSource DeathAudioSource;
    [SerializeField] private AudioClip giliotina;
    [SerializeField] private AudioClip headCut;

    private bool _done;
    private bool _moveLeftActive;

    private enum SoundID : byte
    {
        HeadCut = 1,
        Giliotina = 2
    }

    private AudioClip GetClipByID(SoundID id)
    {
        return id switch
        {
            SoundID.HeadCut => headCut,
            SoundID.Giliotina => giliotina,
            _ => null
        };
    }

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

    // =============================
    // SERVER: Верёвка сломана
    // =============================

    [Server]
    private void HandleFirstZeroServer(TapePiece destroyed)
    {
        if (_done || destroyed == null) return;

        _done = true;

        float pivotX = destroyed.transform.position.x;

        foreach (var r in ropes)
        {
            if (r == null || r == destroyed) continue;

            bool goRight = r.transform.position.x > pivotX ||
                           (Mathf.Approximately(r.transform.position.x, pivotX) && equalGoesRight);

            var target = goRight ? rightAnchor : leftAnchor;

            r.transform.SetParent(target, worldPositionStays);
        }

        StartCoroutine(PostZeroSequenceServer(destroyed));
    }

    // =============================
    // SERVER: Казнь
    // =============================

    [Server]
    private IEnumerator PostZeroSequenceServer(TapePiece destroyed)
    {
        RpcPlayOneShot(SoundID.HeadCut);

        yield return new WaitForSeconds(Random.Range(postZeroDelayRange.x, postZeroDelayRange.y));

        if (destroyed != null)
            NetworkServer.Destroy(destroyed.gameObject);

        _moveLeftActive = true;

        RpcPlayOneShot(SoundID.Giliotina);

        if (!string.IsNullOrEmpty(deathTrigger))
            RpcSetAnimatorTrigger(deathTrigger);

        // ======= ПОБЕДА ПАЛАЧА =======
        if (GameManager.Instance != null)
            GameManager.Instance.PalachWin();
    }

    // =============================
    // RPC
    // =============================

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

    // =============================
    // SERVER MOVE
    // =============================

    private void Update()
    {
        if (!isServer) return;

        if (_moveLeftActive && leftAnchor != null)
        {
            leftAnchor.Translate(
                Vector3.left * leftMoveSpeed * Time.deltaTime,
                Space.World
            );
        }
    }
}
