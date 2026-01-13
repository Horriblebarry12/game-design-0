using Unity.Burst;
using Unity.Mathematics;

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
		public float Frequency;
		public float Amplitude;
		public int Octaves;
		public float3 Offset;
		public float Persistence;
		public float Lacunarity;
		public float3 SmoothingOffset;
		public float Smoothing;
	}

	[System.Serializable]
	public struct RigidNoise
	{
		public float Scale;
		public float Frequency;
		public float Amplitude;
		public int Octaves;
		public float3 Offset;
		public float Persistence;
		public float Lacunarity;
		public float Power;
	}
}