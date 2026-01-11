using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;
using Mirror;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private KnifeManager knifeManager;
    [SerializeField] private GameObject barrier;
    [SerializeField] private Animator ropeAnimator;
    [SerializeField] private Animator handAnimator;
    [SerializeField] private TextMeshPro waveText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] barrierOpenSounds;
    [SerializeField] private AudioClip[] barrierCloseSounds;
    [SerializeField] private AudioClip[] roundStartSounds;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float barrierMoveDistance = 2f;
    [SerializeField] private float barrierMoveDuration = 0.5f;
    [SerializeField] private float delayBeforeAttack = 1f;

    [SyncVar] private int playersReady = 0;

    private bool waitingForAttack = false;
    private bool canAttack = false;
    private bool isBarrierDown = true;
    private bool roundInProgress = false;
    private int roundCount = 0;
    private Vector3 barrierOriginalPosition;

    void Start()
    {
        if (barrier != null)
            barrierOriginalPosition = barrier.transform.position;

        UpdateWaveText();
        StartCoroutine(GameLoop());
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerReady()
    {
        if (roundInProgress) return;
        playersReady = Mathf.Min(playersReady + 1, 2);
        if (playersReady >= 2)
        {
            playersReady = 0;
            RpcAllowAttack();
        }
    }

    [ClientRpc]
    private void RpcAllowAttack()
    {
        canAttack = true;
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            roundCount++;
            UpdateWaveText();

            if (roundCount == 10)
            {
                SceneManager.LoadScene("Win scene");
            }

            if (barrier != null)
                ShowBarrier();
            yield return ShowBarrier();

            waitingForAttack = true;
            yield return new WaitUntil(() => canAttack);
            roundInProgress = true;

            waitingForAttack = false;
            canAttack = false;

            yield return new WaitForSeconds(delayBeforeAttack);

            if (barrier != null)
                HideBarrier();
            yield return HideBarrier();

            knifeManager.ThrowAll();

            yield return new WaitForSeconds(respawnDelay);

            knifeManager.ResetKnives();

            roundInProgress = false;
        }
    }

    public void StartRound()
    {
        if (!isBarrierDown) return;
        if (roundInProgress) return;

        CmdPlayerReady();
    }

    private IEnumerator ShowBarrier()
    {
        barrier.transform.DOMoveY(barrierOriginalPosition.y, barrierMoveDuration);
        isBarrierDown = true;
        PlayRandomSound(barrierCloseSounds);

        Tween t = barrier.transform.DOMoveY(barrierOriginalPosition.y, barrierMoveDuration)
            .SetEase(Ease.InQuad);

        PlayRandomSound(barrierCloseSounds);
        yield return t.WaitForCompletion();
    }

    private IEnumerator HideBarrier()
    {
        barrier.transform.DOMoveY(barrierOriginalPosition.y + barrierMoveDistance, barrierMoveDuration);
        isBarrierDown = false;
        PlayRandomSound(barrierOpenSounds);

        Tween t = barrier.transform
            .DOMoveY(barrierOriginalPosition.y + barrierMoveDistance, barrierMoveDuration)
            .SetEase(Ease.OutQuad);

        PlayRandomSound(barrierOpenSounds);
        yield return t.WaitForCompletion();
    }

    private void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = $"ROUND: {roundCount}";
            waveText.DOFade(1f, 0.3f).From(0f);
            waveText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 6, 0.5f);
        }
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}
