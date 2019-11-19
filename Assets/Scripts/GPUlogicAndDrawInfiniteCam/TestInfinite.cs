using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInfinite : MonoBehaviour
{
    public float speed;
    public Vector3 dir;
    public float layerSize;

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        pos += dir * Time.deltaTime * speed;
        pos.x %= layerSize;
        pos.y %= layerSize;
        pos.z %= layerSize;
        transform.position = pos;
    }
}
