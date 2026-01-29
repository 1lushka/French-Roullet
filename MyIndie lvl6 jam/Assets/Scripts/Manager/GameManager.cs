using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private KnifeManager knifeManager;
    [SerializeField] private GameObject barrier;
    [SerializeField] private TextMeshPro waveText;
    [SerializeField] private AudioSource audioSource;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] barrierOpenSounds;
    [SerializeField] private AudioClip[] barrierCloseSounds;
    [SerializeField] private AudioClip[] roundStartSounds;

    [Header("Timings")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float barrierMoveDistance = 2f;
    [SerializeField] private float barrierMoveDuration = 0.5f;
    [SerializeField] private float delayBeforeAttack = 1f;

    [Header("Debug")]
    [SerializeField] private bool soloTestMode = true; // ← ВКЛ для теста

    // =====================
    // STATE
    // =====================

    private bool canAttack = false;
    private bool isBarrierDown = true;
    private bool roundInProgress = false;

    private int roundCount = 0;

    private Vector3 barrierOriginalPosition;

    private PlayerController localPlayer;

    // =====================
    // READY
    // =====================

    private HashSet<uint> readyPlayers = new HashSet<uint>();

    // =====================
    // INIT
    // =====================

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (barrier != null)
            barrierOriginalPosition = barrier.transform.position;

        UpdateWaveText();

        if (isServer)
            StartCoroutine(GameLoop());
    }

    // =====================
    // PLAYER REGISTER
    // =====================

    public void RegisterLocalPlayer(PlayerController player)
    {
        localPlayer = player;
    }

    // =====================
    // READY SYSTEM
    // =====================

    [Command(requiresAuthority = false)]
    public void CmdPlayerReady(NetworkConnectionToClient sender = null)
    {
        if (!isServer) return;
        if (roundInProgress) return;
        if (sender == null) return;

        uint id = sender.identity.netId;

        if (!readyPlayers.Contains(id))
            readyPlayers.Add(id);

        int needPlayers = soloTestMode ? 1 : 2;

        if (readyPlayers.Count >= needPlayers)
        {
            readyPlayers.Clear();
            RpcAllowAttack();
        }
    }

    [ClientRpc]
    private void RpcAllowAttack()
    {
        canAttack = true;
    }

    // =====================
    // UI BUTTON
    // =====================

    public void StartRound()
    {
        if (!isBarrierDown) return;
        if (roundInProgress) return;

        // Hand animation
        if (localPlayer != null)
            localPlayer.PlayHandAnimation();

        CmdPlayerReady();
    }

    // =====================
    // GAME LOOP (SERVER)
    // =====================

    private IEnumerator GameLoop()
    {
        while (true)
        {
            roundCount++;

            RpcUpdateWave(roundCount);

            if (roundCount >= 10)
            {
                RpcLoadWinScene();
                yield break;
            }

            yield return ShowBarrier();

            yield return new WaitUntil(() => canAttack);

            roundInProgress = true;
            canAttack = false;

            yield return new WaitForSeconds(delayBeforeAttack);

            yield return HideBarrier();

            RpcThrowKnives();

            yield return new WaitForSeconds(respawnDelay);

            RpcResetKnives();

            roundInProgress = false;
        }
    }

    // =====================
    // RPC EVENTS
    // =====================

    [ClientRpc]
    private void RpcThrowKnives()
    {
        knifeManager.ThrowAll();
    }

    [ClientRpc]
    private void RpcResetKnives()
    {
        knifeManager.ResetKnives();
    }

    [ClientRpc]
    private void RpcUpdateWave(int round)
    {
        roundCount = round;
        UpdateWaveText();
    }

    [ClientRpc]
    private void RpcLoadWinScene()
    {
        SceneManager.LoadScene("Win scene");
    }

    // =====================
    // BARRIER
    // =====================

    private IEnumerator ShowBarrier()
    {
        isBarrierDown = true;

        PlayRandomSound(barrierCloseSounds);

        Tween t = barrier.transform
            .DOMoveY(barrierOriginalPosition.y, barrierMoveDuration)
            .SetEase(Ease.InQuad);

        yield return t.WaitForCompletion();
    }

    private IEnumerator HideBarrier()
    {
        isBarrierDown = false;

        PlayRandomSound(barrierOpenSounds);

        Tween t = barrier.transform
            .DOMoveY(barrierOriginalPosition.y + barrierMoveDistance, barrierMoveDuration)
            .SetEase(Ease.OutQuad);

        yield return t.WaitForCompletion();
    }

    // =====================
    // UI
    // =====================

    private void UpdateWaveText()
    {
        if (waveText == null) return;

        waveText.text = $"ROUND: {roundCount}";

        waveText.DOFade(1f, 0.3f).From(0f);

        waveText.transform
            .DOPunchScale(Vector3.one * 0.1f, 0.3f, 6, 0.5f);
    }

    // =====================
    // AUDIO
    // =====================

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (audioSource == null) return;
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        audioSource.PlayOneShot(clip);
    }
}
