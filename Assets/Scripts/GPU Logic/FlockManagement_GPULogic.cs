using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("BehaviourTests")]

public class FlockManagement_GPULogic : MonoBehaviour
{
    [Header("Compute Shader")]
    [SerializeField] ComputeShader _cshader;
    int kernelHandle;

    //Data
    public static GPUBoid_Compute[] boidsBuffer;

    [Header("Prefab")]
    [SerializeField] Transform _boidsPrefab;
    Transform[] m_boidsTransform;

    [Header("Parameters")]
    public Transform target;
    [Tooltip("How many boids in this flock")] public int boidsCount;
    [Tooltip("How fast boids move")] public float moveSpeed;
    [Tooltip("How fast can boid turn")] public float rotationSpeed;
    [Tooltip("How far the boid considere another boid as a neighbour")] public float maxNeighbourDistance;
    [Tooltip("How far the boid wants to be from each others")] public float separationDistance;

    [Header("Rules")]
    public float weightTarget;
    public float weightCohesion;
    public float weightAlignement;
    public float weightSeparation;

    [Header("SpawnArea")]
    public Vector3 spawnArea;

    public enum Mode { CPU,GPU}
    public Mode mode;

    public static FlockManagement_GPULogic Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetUpComputeShader();

        CreateBoids();
    }

    void SetUpComputeShader()
    {
        kernelHandle = _cshader.FindKernel("CSMain");
        _cshader.SetFloat("_MoveSpeed", moveSpeed);
        _cshader.SetFloat("_RotationSpeed", rotationSpeed);
        _cshader.SetFloat("_NeighbourDistance", maxNeighbourDistance);
        _cshader.SetFloat("_SeparationDistance", separationDistance);
        _cshader.SetInt("_BoidsCount", boidsCount);
        _cshader.SetFloat("_WeightCohesion", weightCohesion);
        _cshader.SetFloat("_WeightAlignement", weightAlignement);
        _cshader.SetFloat("_WeightSeparation", weightSeparation);
        _cshader.SetFloat("_WeightTarget", weightTarget);
    }

    void CreateBoids()
    {
        m_boidsTransform = new Transform[boidsCount];
        boidsBuffer = new GPUBoid_Compute[boidsCount];
        for (int i = 0; i < boidsCount; i++)
        {
            m_boidsTransform[i] = Instantiate(_boidsPrefab, new Vector3(Random.Range(-0.5f,0.5f) * spawnArea.x, Random.Range(-0.5f, 0.5f) * spawnArea.y, Random.Range(-0.5f, 0.5f)*spawnArea.z),Quaternion.identity, null);
            m_boidsTransform[i].name = "Boid_" + i.ToString("0000");
            boidsBuffer[i] = new GPUBoid_Compute();
            boidsBuffer[i].position = m_boidsTransform[i].position;
            boidsBuffer[i].direction = Vector3.forward;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Update constants (debug)
        //_cshader.SetFloat("_MoveSpeed", moveSpeed);
        //_cshader.SetFloat("_RotationSpeed", rotationSpeed);
        //_cshader.SetFloat("_NeighbourDistance", maxNeighbourDistance);
        //_cshader.SetFloat("_SeparationDistance", separationDistance);
        //_cshader.SetInt("_BoidsCount", boidsCount);
        //_cshader.SetFloat("_WeightCohesion", weightCohesion);
        //_cshader.SetFloat("_WeightAlignement", weightAlignement);
        //_cshader.SetFloat("_WeightSeparation", weightSeparation);
        //_cshader.SetFloat("_WeightTarget", weightTarget);
        //Update variables
        _cshader.SetFloat("_DeltaTime", Time.deltaTime);
        _cshader.SetVector("_Target", target.position);

        if (mode == Mode.GPU)
        {
            ComputeBuffer buffer = new ComputeBuffer(boidsCount, sizeof(float) * 3 * 2); //Stride is the size of the 2 float3 arrays of the struct
            buffer.SetData(boidsBuffer);

            //Send the boids to the compute shader to compute
            _cshader.SetBuffer(kernelHandle, "boidsBuffer", buffer);
            _cshader.Dispatch(kernelHandle, boidsCount, 1, 1);

            //Get the data back
            buffer.GetData(boidsBuffer);
            buffer.Release();
        }
        else
        {
            FlockBehaviourTest.CPUTest();
        }

        for (int i = 0; i < boidsCount; i++)
        {
            m_boidsTransform[i].localPosition = boidsBuffer[i].position;
            m_boidsTransform[i].forward = boidsBuffer[i].direction;
        }
    }

    public struct GPUBoid_Compute
    {
        public Vector3 position;
        public Vector3 direction;
    }
}
