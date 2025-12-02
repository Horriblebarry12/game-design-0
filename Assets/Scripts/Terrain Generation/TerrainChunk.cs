using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public class TerrainChuck : MonoBehaviour
{
    public int Width;
    public int Height;
    public int Depth;
    public void Generate() 
    {
        NativeArray<float> outputValues = new NativeArray<float>(Depth*Width*Height, Allocator.TempJob);

        var job = new GenerateChunk()
        {
            ChunkDimensions = new float3(Depth, Width, Height),
            OutputValues = outputValues
        };

        JobHandle jobHandle = job.Schedule(Depth * Width * Height, 32);

        jobHandle.Complete();

        foreach (var value in outputValues)
        {
            Debug.Log(value);
        }
    }

    private void Start()
    {
        Generate();
    }
}

struct GenerateChunk : IJobParallelFor 
{
    [ReadOnly] public float3 ChunkDimensions;
    float3 indexToPos(int i) 
    {
        float3 ret;
        ret.z = i / (ChunkDimensions.x * ChunkDimensions.y);
        ret.y = (i - (ret.z * ChunkDimensions.x * ChunkDimensions.y)) / ChunkDimensions.y;
        ret.x = (i - (ret.z * ChunkDimensions.x * ChunkDimensions.y * ChunkDimensions.y)) / ChunkDimensions.x;
        return ret;
    }

    public NativeArray<float> OutputValues;

    public void Execute(int i) 
    {
        float3 pos = indexToPos(i);
        
        OutputValues[i] = noise.cnoise(pos);
    }
}

