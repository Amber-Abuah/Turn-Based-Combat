using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavController : MonoBehaviour
{
    NavMeshAgent nav;
    Animator anim;

    [SerializeField] float dampTime = .7f;

    private void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        anim.SetFloat("Speed", nav.velocity.magnitude, dampTime, Time.deltaTime);
    }

    public float MoveToTarget(Vector3 position)
    {
        nav.SetDestination(position);

        return (transform.position - position).magnitude;
    }

    public void EnableNavMesh()
    {
        nav.enabled = true;
    }

    public void DisableNavMesh()
    {
        nav.enabled = false;
    }

}
