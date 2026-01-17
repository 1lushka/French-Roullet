using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RopesHealthManager : NetworkBehaviour
{
    [Header("Ropes")]
    [SerializeField] private TapePiece[] ropes;

    // Events (server only)
    public event Action<int, TapePiece> OnFirstThresholdHit;
    public event Action<TapePiece> OnFirstHit2;
    public event Action<TapePiece> OnFirstHit1;
    public event Action<TapePiece> OnFirstHit0;

    // Tracking previous health per rope
    private readonly Dictionary<TapePiece, int> _lastHealth = new();

    // Global fired thresholds, shared for whole session
    private static readonly HashSet<int> _firedThresholdsGlobal = new HashSet<int>();

    private static readonly int[] _thresholds = { 2, 1, 0 };

    // --------------------------------------------------------------------
    //                      SERVER LIFECYCLE
    // --------------------------------------------------------------------
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (ropes == null) return;

        foreach (var rope in ropes)
        {
            if (rope == null) continue;

            if (!_lastHealth.ContainsKey(rope))
                _lastHealth[rope] = rope.Health;

            // Subscribe ONLY on server
            rope.HealthChanged += HandleHealthChangedServer;
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (ropes == null) return;

        foreach (var rope in ropes)
        {
            if (rope == null) continue;
            rope.HealthChanged -= HandleHealthChangedServer;
        }
    }

    // --------------------------------------------------------------------
    //                    SERVER HEALTH CHANGE HANDLER
    // --------------------------------------------------------------------
    [Server]
    private void HandleHealthChangedServer(TapePiece rope, int newHealth)
    {
        if (!_lastHealth.TryGetValue(rope, out var prev))
            prev = newHealth;

        foreach (var t in _thresholds)
        {
            // already fired globally?
            if (_firedThresholdsGlobal.Contains(t))
                continue;

            // Detect threshold crossing
            if (prev > t && newHealth <= t)
            {
                _firedThresholdsGlobal.Add(t);

                OnFirstThresholdHit?.Invoke(t, rope);

                switch (t)
                {
                    case 2: OnFirstHit2?.Invoke(rope); break;
                    case 1: OnFirstHit1?.Invoke(rope); break;
                    case 0: OnFirstHit0?.Invoke(rope); break;
                }

                break; // Only the lowest threshold can be crossed at once
            }
        }

        _lastHealth[rope] = newHealth;
    }

    // --------------------------------------------------------------------
    //                      RESET BETWEEN LEVELS IF NEEDED
    // --------------------------------------------------------------------
    public static void ResetGlobalThresholds()
    {
        _firedThresholdsGlobal.Clear();
    }

    // --------------------------------------------------------------------
    //                      DEBUG (SERVER)
    // --------------------------------------------------------------------
    void Start()
    {
        if (isServer) // Debug only from server
            HookRopesHealthDebug(this);
    }

    public static void HookRopesHealthDebug(RopesHealthManager mgr)
    {
        if (mgr == null)
        {
            Debug.LogWarning("[RopesHealth] Manager not found.");
            return;
        }

        mgr.OnFirstThresholdHit += (t, rope) =>
            Debug.Log($"[RopesHealth] GLOBAL threshold {t} hit by '{rope?.name}'");

        mgr.OnFirstHit2 += rope =>
            Debug.Log($"[RopesHealth] FIRST 2 hit by '{rope?.name}'");

        mgr.OnFirstHit1 += rope =>
            Debug.Log($"[RopesHealth] FIRST 1 hit by '{rope?.name}'");

        mgr.OnFirstHit0 += rope =>
            Debug.Log($"[RopesHealth] FIRST 0 hit by '{rope?.name}'");

    }
}
