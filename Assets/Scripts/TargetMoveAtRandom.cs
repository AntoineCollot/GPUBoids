using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMoveAtRandom : MonoBehaviour
{
    public float playAreaSize;
    public float moveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MoveAround());
    }

    IEnumerator MoveAround()
    {
        while(true)
        {
            Vector3 target = new Vector3(Random.Range(0,playAreaSize), Random.Range(0, playAreaSize), Random.Range(0, playAreaSize));
            Vector3 toTarget = target - transform.position;
            while (toTarget.magnitude> 10)
            {
                toTarget = target - transform.position;
                transform.Translate(toTarget.normalized * moveSpeed * Time.deltaTime, Space.World);

                yield return null;
            }
        }
    }
}
