using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public GameObject objectToFollow;

    void Update()
    {
        if (objectToFollow)
            transform.position = objectToFollow.transform.position;
    }
}
