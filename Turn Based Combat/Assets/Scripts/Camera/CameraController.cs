using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    CameraAnimator cameraAnimator;
    bool followPlayer;

    public static CameraController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cameraAnimator = GetComponentInChildren<CameraAnimator>();
    }

    public void FollowPlayer(Unit player)
    {
        followPlayer = true;
        StartCoroutine(FollowPlayerUnit(player));
    }

    public void StopFollowPlayer()
    {
        followPlayer = false;
    }

    public void CloseUp(Unit unit)
    {
        cameraAnimator.CloseUp();
        transform.position = unit.transform.position;
    }

    public void CloseUpZoomOut(Unit unit)
    {
        cameraAnimator.CloseUpZoomOut();
        transform.position = unit.transform.position;
    }

    public void ResetToBackShot()
    {
        cameraAnimator.Default();
        transform.position = Vector3.zero;
    }

    public void Ultimate()
    {
        cameraAnimator.Ultimate();
    }

    IEnumerator FollowPlayerUnit(Unit player)
    {
        cameraAnimator.Follow();

        while (followPlayer)
        {
            transform.position = player.transform.position;
            yield return null;
        }

        ResetToBackShot();
    }
}
