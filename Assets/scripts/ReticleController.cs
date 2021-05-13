using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleController : MonoBehaviour
{
    public bool isSpinning = true;

    private void FixedUpdate()
    {
        if (isSpinning)
        {
            transform.RotateAround(transform.position, Vector3.up, 1.0f);
        }
    }
}
