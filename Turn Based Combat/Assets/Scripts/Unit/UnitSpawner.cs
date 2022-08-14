using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] Transform worldCanvas;
    [SerializeField] GameObject hitEffectGameObject;
    [SerializeField] GameObject enemyUI;

    public static UnitSpawner Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public GameObject CreateEnemyUI(Transform uiPosition)
    {
        GameObject go = Instantiate(enemyUI) as GameObject;
        go.transform.SetParent(worldCanvas);
        go.transform.position = uiPosition.position;

        return go;
    }

    public void CreateHitEffect(Unit unit)
    {
        unit.SetUI(hitEffectGameObject, worldCanvas);
    }
}
