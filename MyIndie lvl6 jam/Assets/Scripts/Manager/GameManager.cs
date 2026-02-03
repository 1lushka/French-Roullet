using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private KnifeManager knifeManager;
    [SerializeField] private GameObject barrier;
    [SerializeField] private TextMeshPro waveText;
    [SerializeField] private AudioSource audioSource;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] barrierOpenSounds;
    [SerializeField] private AudioClip[] barrierCloseSounds;

    [Header("Timings")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float barrierMoveDistance = 2f;
    [SerializeField] private float barrierMoveDuration = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool soloTestMode = true;

    [Header("Round Timer")]
    [SerializeField] private float roundTime = 30f;

    // =====================
    // STATE
    // =====================

    [SyncVar(hook = nameof(OnCanAttackChanged))]
    private bool canAttack; // серверный флаг, клиенты получают уведомление

    private bool isBarrierDown = true;
    private bool roundInProgress;
    private bool gameEnded;

    private int roundCount;
    private float currentTime;
    private Vector3 barrierOriginalPosition;
    private PlayerController localPlayer;

    private HashSet<uint> readyPlayers = new HashSet<uint>();

    // =====================
    // INIT
    // =====================

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (barrier != null) barrierOriginalPosition = barrier.transform.position;
        if (resultPanel != null) resultPanel.SetActive(false);
        UpdateWaveText();

        if (isServer) StartCoroutine(GameLoop());
    }

    // =====================
    // PLAYER REGISTER
    // =====================

    public void RegisterLocalPlayer(PlayerController player)
    {
        localPlayer = player;
    }

    // =====================
    // READY
    // =====================

    [Command(requiresAuthority = false)]
    public void CmdPlayerReady(NetworkConnectionToClient sender = null)
    {
        if (!isServer || roundInProgress || sender == null) return;

        uint id = sender.identity.netId;
        if (!readyPlayers.Contains(id))
            readyPlayers.Add(id);

        int needPlayers = soloTestMode ? 1 : 2;

        if (readyPlayers.Count >= needPlayers)
        {
            readyPlayers.Clear();
            canAttack = true; // SyncVar автоматически уведомит клиентов
        }
    }

    private void OnCanAttackChanged(bool oldValue, bool newValue)
    {
        if (newValue && localPlayer != null)
        {
            localPlayer.PlayHandAnimation();
        }
    }

    // =====================
    // UI BUTTON
    // =====================

    public void StartRound()
    {
        if (!isBarrierDown || roundInProgress || gameEnded) return;
        CmdPlayerReady();
    }

    // =====================
    // GAME LOOP (SERVER)
    // =====================

    private IEnumerator GameLoop()
    {
        while (!gameEnded)
        {
            roundCount++;
            RpcUpdateWave(roundCount);

            yield return ShowBarrier();

            // Ждём готовности всех игроков
            yield return new WaitUntil(() => canAttack);

            roundInProgress = true;
            canAttack = false; // сброс для следующего раунда

            currentTime = roundTime;
            yield return HideBarrier();

            RpcThrowKnives();

            yield return new WaitForSeconds(respawnDelay);

            RpcResetKnives();
            roundInProgress = false;

            if (roundCount >= 10)
            {
                PrisonerWin();
            }
        }
    }

    // =====================
    // WIN / LOSE
    // =====================

    [Server]
    public void PalachWin()
    {
        if (gameEnded) return;
        gameEnded = true;
        RpcShowResult("ПАЛАЧ ПОБЕДИЛ!");
    }

    [Server]
    public void PrisonerWin()
    {
        if (gameEnded) return;
        gameEnded = true;
        RpcShowResult("ЗАКЛЮЧЁННЫЙ ПОБЕДИЛ!");
    }

    [ClientRpc]
    private void RpcShowResult(string message)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null) resultText.text = message;
    }

    // =====================
    // RPC EVENTS
    // =====================

    [ClientRpc]
    private void RpcThrowKnives() => knifeManager.ThrowAll();

    [ClientRpc]
    private void RpcResetKnives() => knifeManager.ResetKnives();

    [ClientRpc]
    private void RpcUpdateWave(int round)
    {
        roundCount = round;
        UpdateWaveText();
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
        waveText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 6, 0.5f);
    }

    // =====================
    // AUDIO
    // =====================

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0) return;

        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
