using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManagement_GPULogicAndDraw : MonoBehaviour
{
    [Header("Compute Shader")]
    [SerializeField] ComputeShader _cshader;
    int kernelHandle;

    //Data
    public static GPUBoid_Compute[] boidsData;

    [Header("Drawing")]
    public Material boidMaterial;
    public Mesh boidMesh;
    [Range(1, 500)] public float boidSize = 100;
    ComputeBuffer m_boidLogicBuffer;
    ComputeBuffer m_boidDrawingBuffer;

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
    public float spawnRadius;
    private Bounds m_infiniteBounds = new Bounds(Vector3.zero, Vector3.one * 9999);

    public static FlockManagement_GPULogicAndDraw Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateBoids();

        SetUpComputeShader();
        SetUpDrawing();
    }

    void SetUpDrawing()
    {
        boidMaterial = new Material(boidMaterial);

        m_boidDrawingBuffer = new ComputeBuffer(
            1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments
        );

        m_boidDrawingBuffer.SetData(new uint[5] {
            boidMesh.GetIndexCount(0), (uint) boidsCount, 0, 0, 0
        });

        boidMaterial.SetBuffer("boidsBuffer", m_boidLogicBuffer);
        boidMaterial.SetFloat("_BoidSize",boidSize);
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

        m_boidLogicBuffer = new ComputeBuffer(boidsCount, sizeof(float) * 3 * 2); //Stride is the size of the 2 float3 arrays of the struct
        m_boidLogicBuffer.SetData(boidsData);

        //Send the boids to the compute shader to compute
        _cshader.SetBuffer(kernelHandle, "boidsBuffer", m_boidLogicBuffer);
    }

    void CreateBoids()
    {
        boidsData = new GPUBoid_Compute[boidsCount];
        for (int i = 0; i < boidsCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-1.0f,1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized * Random.Range(0,spawnRadius);
            boidsData[i] = new GPUBoid_Compute();
            boidsData[i].position = pos;
            boidsData[i].direction = Vector3.forward;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Update constants (debug)
        _cshader.SetFloat("_MoveSpeed", moveSpeed);
        _cshader.SetFloat("_RotationSpeed", rotationSpeed);
        _cshader.SetFloat("_NeighbourDistance", maxNeighbourDistance);
        _cshader.SetFloat("_SeparationDistance", separationDistance);
        _cshader.SetInt("_BoidsCount", boidsCount);
        _cshader.SetFloat("_WeightCohesion", weightCohesion);
        _cshader.SetFloat("_WeightAlignement", weightAlignement);
        _cshader.SetFloat("_WeightSeparation", weightSeparation);
        _cshader.SetFloat("_WeightTarget", weightTarget);
        //Update variables
        _cshader.SetFloat("_DeltaTime", Time.deltaTime);
        _cshader.SetVector("_Target", target.position);

        _cshader.Dispatch(kernelHandle, boidsCount/256+1, 1, 1);
    }
    
    //Draw the mesh
    void LateUpdate()
    {
        Graphics.DrawMeshInstancedIndirect(boidMesh, 0, boidMaterial, m_infiniteBounds, m_boidDrawingBuffer, 0);
    }

    private void OnDisable()
    {
        m_boidLogicBuffer.Release();
        m_boidDrawingBuffer.Release();
    }

    public struct GPUBoid_Compute
    {
        public Vector3 position;
        public Vector3 direction;
    }
}
