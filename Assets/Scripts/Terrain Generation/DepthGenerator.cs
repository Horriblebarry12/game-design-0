using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
public struct DepthGenerator : IJobParallelFor
{
	[ReadOnly] public ChunkInfo Info;
	[ReadOnly] public NoiseParamaters NoiseSettings;
	[ReadOnly] public NativeArray<float> HeightMaps;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	int3 indexToPos(int i)
	{
		int layerSize = (Info.ChunkDimensions.x + 1) * (Info.ChunkDimensions.y + 1);

		int z = i / layerSize;
		int remaining = i % layerSize;

		int y = remaining / (Info.ChunkDimensions.x + 1);
		int x = remaining % (Info.ChunkDimensions.x + 1);

		return new int3(x, y, z);
	}

	int get2DIndex(float2 pos) 
	{

		return (int)(pos.x * (Info.ChunkDimensions.z + 1) + pos.y);
    }

    public NativeArray<float> OutputDepths;



	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Execute(int i)
	{
		float3 pos = indexToPos(i);
		float3 offsetPos = pos + (int3)Info.PositionOffset;
        //float height = SmoothedFractalPerlinNoise((pos + new float3(125678.5f)) / 46.5f, 2, 0.3f, 2.0f, 0.1f, 0.8f) * 10.0f;
        //float height = math.pow(Noise.FractalRigidNoise((pos + new float3(125678.5f)) / 250.0f, 3, 0.3f, 2.0f), 3) * 160.0f;

        OutputDepths[i] = HeightMaps[get2DIndex(pos.xz)] - offsetPos.y;
		//OutputValues[i] = 16 - pos.y;
	}
}
