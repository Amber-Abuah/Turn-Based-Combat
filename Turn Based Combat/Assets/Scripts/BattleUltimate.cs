using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUltimate : MonoBehaviour
{
    int currentUltimatePoints;
    [SerializeField] int maxUltimatePoints;

    BattleSystemUI battleSystemUI;

    public bool CanUltimate { get { return currentUltimatePoints == maxUltimatePoints; } }

    private void Start()
    {
        currentUltimatePoints = 0;
        battleSystemUI = GetComponent<BattleSystemUI>();
        battleSystemUI.UpdateUltimateBar(0);
    }

    public void AddUltimatePoints(int ultimatePoints)
    {
        currentUltimatePoints = Mathf.Clamp(currentUltimatePoints + ultimatePoints, 0, maxUltimatePoints);
        battleSystemUI.UpdateUltimateBar((float)currentUltimatePoints/ maxUltimatePoints);
    }

    public void ClearPoints()
    {
        currentUltimatePoints = 0;
        battleSystemUI.UpdateUltimateBar((float)currentUltimatePoints / maxUltimatePoints);
    }
}
