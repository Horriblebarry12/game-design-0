using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
public struct ValuesPopulator : IJobParallelFor
{
	[ReadOnly] public ChunkInfo Info;
	[ReadOnly] public NoiseParamaters NoiseSettings;
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


	public NativeArray<float> OutputValues;



	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Execute(int i)
	{
		float3 pos = indexToPos(i) + (int3)Info.PositionOffset;
		var y = pos.y;
		//pos /= 2;
		pos.y = 120874;

		//float height = SmoothedFractalPerlinNoise((pos + new float3(125678.5f)) / 46.5f, 2, 0.3f, 2.0f, 0.1f, 0.8f) * 10.0f;
		float height = math.pow(Noise.FractalRigidNoise((pos + new float3(125678.5f)) / 250.0f, 3, 0.3f, 2.0f), 3) * 160.0f;

		OutputValues[i] = height - y;
		//OutputValues[i] = 16 - pos.y;
	}
}
[System.Serializable]
[BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast)]
public struct NoiseParamaters 
{
	public float Seed;
	public float3 Offset;
	public float Scale;

	public GeneralNoise GeneralNoiseSettings;
	public RigidNoise RigidNoiseSettings;
	[System.Serializable]
	public struct GeneralNoise 
	{
		public float Scale;
		public float Amplitude;
		public int Octaves;
		public float3 Offset;
		public float Persistence;
		public float Lacunarity;
	}

	[System.Serializable]
	public struct RigidNoise 
	{
		public float Scale;
		public float Amplitude;
		public int Octaves;
		public float3 Offset;
		public float Persistence;
		public float Lacunarity;
		public float Power;
	}
}