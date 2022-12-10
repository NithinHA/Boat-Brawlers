using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : MonoBehaviour
{
    public float Weight = 1f;

    private void Start()
    {
        Destroy(this.gameObject, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.Tags.RAFT))
        {
            RaftController_Custom.Instance.AddInstantaneousForce(transform.position, Weight);
            Destroy(this.gameObject);
        }
    }
}
