using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
public struct Noise 
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FractalPerlinNoise(float3 pos, int numOctaves, float persistence, float lacunarity)
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SmoothedFractalPerlinNoise(float3 pos, int numOctaves, float persistence, float lacunarity, float offset, float smoothing)
	{
		float3 pos1 = pos + new float3(offset, 0, 0);
		float3 pos2 = pos + new float3(0, offset, 0);
		float3 pos3 = pos + new float3(0, 0, offset);
		float3 pos4 = pos + new float3(-offset, 0, 0);
		float3 pos5 = pos + new float3(0, -offset, 0);
		float3 pos6 = pos + new float3(0, 0, -offset);

		float val = FractalPerlinNoise(pos, numOctaves, persistence, lacunarity) * smoothing;
		float val1 = FractalPerlinNoise(pos + pos1, numOctaves, persistence, lacunarity);
		//val1 += FractalPerlinNoise(pos + pos2, numOctaves, persistence, lacunarity);
		val1 += FractalPerlinNoise(pos + pos3, numOctaves, persistence, lacunarity);
		val1 += FractalPerlinNoise(pos + pos4, numOctaves, persistence, lacunarity);
		//val1 += FractalPerlinNoise(pos + pos5, numOctaves, persistence, lacunarity);
		val1 += FractalPerlinNoise(pos + pos6, numOctaves, persistence, lacunarity);

		val1 /= 4;
		val1 *= 1 / smoothing;

		return val + val1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FractalRigidNoise(float3 pos, int numOctaves, float persistence, float lacunarity)
	{
		float noiseOut = 0.0f;
		float frequency = 1.0f;
		float amplitude = 1.0f;
		float amplitudeSum = 0.0f;
		for (int i = 0; i < numOctaves; i++)
		{
			amplitudeSum += amplitude;
			noiseOut += (1 - math.abs(noise.cnoise(pos * frequency))) * amplitude;
			frequency *= lacunarity;
			amplitude *= persistence;
		}
		return noiseOut / (amplitudeSum);
	}
}
