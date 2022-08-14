using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] float speed;

    private void Update()
    {
        gameObject.transform.Rotate(new Vector3(0, 0, speed * Time.deltaTime));
    }
}