using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    float initCamPosX = 0f;
    float initThisPosX = 0f;
    public float intensity = 1f;
    void Start()
    {
        initCamPosX = Camera.main.transform.position.x;
        initThisPosX = transform.position.x;
    }

    void Update()
    {
        transform.position = new Vector3((Camera.main.transform.position.x - initCamPosX) * intensity + initThisPosX, transform.position.y);
    }
}
