using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleChaseAi : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    public float turnSpeed = 10f;

    void Update()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep flat on ground

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);

        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}

