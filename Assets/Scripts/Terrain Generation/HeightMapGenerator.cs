using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
public struct HeightMapGenerator : IJobParallelFor 
{
	[ReadOnly] public ChunkInfo Info;
    [ReadOnly] public NoiseParamaters NoiseSettings;
    public NativeArray<float> HeightMaps;

	float2 getPos(int index) 
	{
		return new float2(index / (Info.ChunkDimensions.z + 1), index % (Info.ChunkDimensions.z + 1));
	}

	public void Execute(int i) 
	{
		float2 pos = getPos(i) + (int2)Info.PositionOffset.xz;
		pos += NoiseSettings.Offset.xz;
		HeightMaps[i] = Noise.SmoothedFractalPerlinNoise((new float3(pos.x, NoiseSettings.Seed, pos.y) + NoiseSettings.GeneralNoiseSettings.Offset) * NoiseSettings.GeneralNoiseSettings.Frequency, NoiseSettings.GeneralNoiseSettings.Octaves, NoiseSettings.GeneralNoiseSettings.Persistence, NoiseSettings.GeneralNoiseSettings.Lacunarity, NoiseSettings.GeneralNoiseSettings.SmoothingOffset, NoiseSettings.GeneralNoiseSettings.Smoothing) * 10.0f;
		HeightMaps[i] += math.pow(Noise.FractalRigidNoise((new float3(pos.x, NoiseSettings.Seed, pos.y) + NoiseSettings.Offset) / 250.0f, 3, 0.3f, 2.0f), 3) * 160.0f;
    }
}
