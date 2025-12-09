using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
public struct ValuesPopulator : IJobParallelFor
{
    [ReadOnly] public ChunkInfo Info;
    int3 indexToPos(int i)
    {
        int layerSize = (Info.ChunkDimensions.x + 1) * (Info.ChunkDimensions.y + 1);

        int z = i / layerSize;
        int remaining = i % layerSize;

        int y = remaining / (Info.ChunkDimensions.x + 1);
        int x = remaining % (Info.ChunkDimensions.x + 1);

        return new int3(x, y, z);
    }

    int posToIndex(int3 pos)
    {
        return pos.x + Info.ChunkDimensions.x * (pos.y + Info.ChunkDimensions.y * pos.z);
    }

    public NativeArray<float> OutputValues;

    float FractalPerlinNoise(float3 pos, int numOctaves, float persistence, float lacunarity) 
    {
        float noiseOut = 0.0f;
        float frequency = 1.0f;
        float amplitude = 1.0f;
        float amplitudeSum = 0.0f;
        for (int i = 0; i < numOctaves; i++) 
        {
            amplitudeSum += amplitude;
            noiseOut += noise.cnoise(pos * frequency) * amplitude;
            frequency *= lacunarity;
            amplitude *= persistence;
        }
        return noiseOut / (amplitudeSum);
    }

    public void Execute(int i)
    {
        int3 pos = indexToPos(i) + (int3)Info.PositionOffset;



        OutputValues[i] = FractalPerlinNoise((float3)(pos + new int3(125678)) / 46.5f, 8, 0.6f, 2.0f) * 32.0f - pos.y;
        //OutputValues[i] = 16 - pos.y;
    }
}