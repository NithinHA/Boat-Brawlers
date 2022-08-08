using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehaviour : MonoBehaviour
{
    private void Start()
    {
        Destroy(this.gameObject, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.Tags.RAFT))
        {
            RaftController_Custom.Instance.AddInstantaneousForce(transform.position, 1);
            Destroy(this.gameObject);
        }
    }
}
