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

    float SmoothedFractalPerlinNoise(float3 pos, int numOctaves, float persistence, float lacunarity, float offset, float smoothing) 
    {
        float3 pos1 = pos + new float3(offset, 0, 0);
        float3 pos2 = pos + new float3(0, offset, 0);
        float3 pos3 = pos + new float3(0, 0, offset);
        float3 pos4 = pos + new float3(-offset, 0, 0);
        float3 pos5 = pos + new float3(0, -offset, 0);
        float3 pos6 = pos + new float3(0, 0, -offset);

        float val = FractalPerlinNoise(pos, numOctaves, persistence, lacunarity) * smoothing;
        float val1 = FractalPerlinNoise(pos + pos1, numOctaves, persistence, lacunarity);
        val1 += FractalPerlinNoise(pos + pos2, numOctaves, persistence, lacunarity);
        val1 += FractalPerlinNoise(pos + pos3, numOctaves, persistence, lacunarity);
        val1 += FractalPerlinNoise(pos + pos4, numOctaves, persistence, lacunarity);
        val1 += FractalPerlinNoise(pos + pos5, numOctaves, persistence, lacunarity);
        val1 += FractalPerlinNoise(pos + pos6, numOctaves, persistence, lacunarity);

        val1 /= 6;
        val1 *= 1 / smoothing;

        return val + val1;
    }

    public void Execute(int i)
    {
        int3 pos = indexToPos(i) + (int3)Info.PositionOffset;

        float height = SmoothedFractalPerlinNoise((float3)(pos + new int3(125678)) / 46.5f, 20, 0.3f, 2.0f, 0.1f, 0.8f) * 32.0f;
        height += math.pow(FractalPerlinNoise((float3)(pos + new int3(120487)) / 46.5f, 20, 0.3f, 2.0f), 3) * 64.0f;

        OutputValues[i] = height - pos.y;
        //OutputValues[i] = 16 - pos.y;
    }
}