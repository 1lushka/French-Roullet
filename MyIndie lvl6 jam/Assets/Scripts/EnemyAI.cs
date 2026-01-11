using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private TapePiece[] tapePieces;
    [SerializeField] private Transform[] axes;

    private Vector3[] startPositions;

    void Awake()
    {
        if (axes != null && axes.Length > 0)
        {
            startPositions = new Vector3[axes.Length];
            for (int i = 0; i < axes.Length; i++)
            {
                if (axes[i] != null)
                    startPositions[i] = axes[i].position;
            }
        }
    }

    public void MakeMove()
    {
        if (tapePieces == null || tapePieces.Length == 0 || axes == null || axes.Length == 0)
            return;

        List<TapePiece> availablePieces = new List<TapePiece>(tapePieces);

        for (int i = 0; i < axes.Length; i++)
        {
            Transform axe = axes[i];
            if (axe == null) continue;
            if (availablePieces.Count == 0)
            {
                Debug.LogWarning("Не хватает TapePiece для всех топоров!");
                break;
            }

            int randIndex = Random.Range(0, availablePieces.Count);
            TapePiece chosenPiece = availablePieces[randIndex];
            availablePieces.RemoveAt(randIndex);

            float startY = startPositions != null && startPositions.Length > i ? startPositions[i].y : axe.position.y;
            float startZ = startPositions != null && startPositions.Length > i ? startPositions[i].z : axe.position.z;

            Vector3 targetPos = new Vector3(
                chosenPiece.transform.position.x,
                startY,
                startZ
            );

            ObjectMover.MoveTo(axe, targetPos);
        }
    }

    public void StartAttack()
    {
        foreach (Transform axe in axes)
        {
            if (axe == null) continue;

            Axe axeCtrl = axe.GetComponent<Axe>();
            if (axeCtrl != null)
                axeCtrl.Throw();
        }
    }
}