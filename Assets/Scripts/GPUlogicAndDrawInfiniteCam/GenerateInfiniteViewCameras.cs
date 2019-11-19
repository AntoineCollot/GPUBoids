using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateInfiniteViewCameras : MonoBehaviour
{
    public int additionnalLayers = 1;
    public float layerSize = 250;

    public Camera infiniteCamerarPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Camera mainCam = Camera.main;

        for (int x = -additionnalLayers; x < additionnalLayers + 1; x++)
        {
            for (int y = -additionnalLayers; y < additionnalLayers + 1; y++)
            {
                for (int z = -additionnalLayers; z < additionnalLayers + 1; z++)
                {
                    Camera infiniteCamera = Instantiate(infiniteCamerarPrefab, mainCam.transform);
                    infiniteCamera.transform.localPosition = new Vector3(x, y, z) * layerSize;
                    infiniteCamera.transform.localRotation = Quaternion.identity;
                    infiniteCamera.depth = z;
                    infiniteCamera.name = string.Format("Infinite Camera {0},{1},{2}", x, y, z);
                }
            }
        }
    }
}
