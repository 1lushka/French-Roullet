using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;
using Mirror;

public class TapePiece : NetworkBehaviour
{
    [Header("Health Settings")]
    [SyncVar(hook = nameof(OnHealthChanged))]
    [SerializeField] private int health = 3;

    private int maxHealth;

    [Header("Rope Pieces")]
    [SerializeField] private Transform[] ropePieces;

    [Header("Scale Settings")]
    [SerializeField] private float minScaleY = 0.3f;
    [SerializeField] private float maxScaleY = 1f;
    [SerializeField] private float scaleTweenDuration = 0.3f;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.3f;
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private float shakeRandomness = 90f;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 0.3f;

    private Tween shakeTween;
    private float lastDamageTime = -999f;
    private Vector3[] originalScales;

    public event Action<TapePiece, int> HealthChanged;
    public int Health => health;

    void Start()
    {
        maxHealth = health;

        originalScales = new Vector3[ropePieces.Length];
        for (int i = 0; i < ropePieces.Length; i++)
        {
            if (ropePieces[i] != null)
                originalScales[i] = ropePieces[i].localScale;
        }
    }

    // ---------------------------------------------------------------------
    //                       SERVER: apply real damage
    // ---------------------------------------------------------------------
    [Server]
    public void TakeDamage(int damage)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;

        int newHealth = Mathf.Max(health - damage, 0);
        health = newHealth; // Will sync to all clients → triggers hook
    }

    // ---------------------------------------------------------------------
    //         HOOK — вызывается на всех клиентах при изменении здоровья
    // ---------------------------------------------------------------------
    private void OnHealthChanged(int oldValue, int newValue)
    {
        // Вызов события для RopesHealthManager
        HealthChanged?.Invoke(this, newValue);

        // Визуал клиенты видят только местно
        Shake();
        UpdateRopeScales();

        if (newValue <= 0)
            Detach();
    }

    // ---------------------------------------------------------------------
    //                        CLIENT VISUALS ONLY
    // ---------------------------------------------------------------------
    private void Shake()
    {
        shakeTween?.Kill();
        shakeTween = transform.DOShakePosition(
            duration: shakeDuration,
            strength: shakeStrength,
            vibrato: shakeVibrato,
            randomness: shakeRandomness
        ).SetEase(Ease.OutQuad);
    }

    private void UpdateRopeScales()
    {
        float healthPercent = maxHealth > 0 ? (float)health / maxHealth : 0f;
        float targetScaleY = Mathf.Lerp(minScaleY, maxScaleY, healthPercent);

        for (int i = 0; i < ropePieces.Length; i++)
        {
            Transform rope = ropePieces[i];
            if (rope == null) continue;

            Vector3 original = originalScales[i];
            Vector3 targetScale = new Vector3(original.x, targetScaleY * original.y, original.z);

            rope.DOScale(targetScale, scaleTweenDuration)
                .SetEase(Ease.OutQuad);
        }
    }

    private void Detach()
    {
        shakeTween?.Kill();
        // Здесь логика отрыва, если нужна
        // Можно добавить RPC для спец. эффекта
    }
}
