using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple boid class, doesn't contain a Rigidbody so performance isn't too bad.
/// </summary>
public class Boid : MonoBehaviour
{
    public Vector3 position;
    public Vector3 velocity;

    void FixedUpdate()
    {
        transform.position = position;
    }
}
