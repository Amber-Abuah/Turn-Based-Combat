using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraAnimator : MonoBehaviour
{
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void BattleEnd()
    {
        animator.SetTrigger("BattleEnd");
    }

    public void Follow()
    {
        animator.SetTrigger("Follow");
    }

    public void CloseUp()
    {
        animator.SetTrigger("CloseUp");
    }

    public void CloseUpZoomOut()
    {
        animator.SetTrigger("CloseUpZoomOut");
    }

    public void Ultimate()
    {
        animator.SetTrigger("Ultimate");
    }

    public void Default()
    {
        animator.SetTrigger("Default");
    }
}
