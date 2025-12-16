using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
public readonly unsafe struct ChunkInfo 
{
	

	[ReadOnly] public readonly int3 ChunkDimensions;
	[ReadOnly] public readonly float3 PositionOffset;
	[ReadOnly] public readonly float Threshold;

	public ChunkInfo(int3 chunkDimensions, float3 positionOffset, float threshold)
	{
		ChunkDimensions = chunkDimensions;
		PositionOffset = positionOffset;
		Threshold = threshold;
	}
}