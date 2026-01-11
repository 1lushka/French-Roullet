using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class KnifeManager : NetworkBehaviour
{
    [SerializeField] private List<Axe> knives = new List<Axe>();
    [SerializeField] private bool autoCollect = true;

    private readonly List<Vector3> startPositions = new List<Vector3>();
    private readonly List<Quaternion> startRotations = new List<Quaternion>();

    private void Awake()
    {
        if (autoCollect)
        {
            knives.Clear();
            knives.AddRange(GetComponentsInChildren<Axe>(includeInactive: true));
        }

        SaveStartTransforms();
    }

    private void SaveStartTransforms()
    {
        startPositions.Clear();
        startRotations.Clear();

        foreach (var knife in knives)
        {
            if (knife == null) continue;
            startPositions.Add(knife.transform.position);
            startRotations.Add(knife.transform.rotation);
        }
    }

    public void ThrowAll()
    {
        if (isServer)
            RpcThrowAll();
        else if (isClient)
            CmdThrowAll();
        else
            ExecuteThrowAll();
    }

    [Command(requiresAuthority = false)]
    private void CmdThrowAll() => RpcThrowAll();

    [ClientRpc]
    private void RpcThrowAll() => ExecuteThrowAll();

    private void ExecuteThrowAll()
    {
        foreach (var knife in knives)
        {
            if (knife != null && knife.gameObject.activeInHierarchy)
                knife.Throw();
        }
    }

    public void ResetKnives()
    {
        for (int i = 0; i < knives.Count; i++)
        {
            var knife = knives[i];
            if (knife == null) continue;

            ObjectMover.MoveTo(knife.transform, startPositions[i]);
            knife.transform.rotation = startRotations[i];
        }
    }

    public void RegisterKnife(Axe knife)
    {
        if (!knives.Contains(knife))
        {
            knives.Add(knife);
            startPositions.Add(knife.transform.position);
            startRotations.Add(knife.transform.rotation);
        }
    }

    public void UnregisterKnife(Axe knife)
    {
        int index = knives.IndexOf(knife);
        if (index >= 0)
        {
            knives.RemoveAt(index);
            startPositions.RemoveAt(index);
            startRotations.RemoveAt(index);
        }
    }
}
