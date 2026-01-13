using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
public unsafe struct MeshGenerator : IJob
{
	[ReadOnly] public ChunkInfo Info;
	[ReadOnly] public NativeArray<float> InputValues;
	[ReadOnly] public NativeArray<int> EdgeTable;
	[ReadOnly] public NativeArray<int> TriTable;

	public NativeList<float3> OutputVerticies;
	public NativeList<int> OutputTriangles;
	public NativeList<float3> OutputNormals;
	//public NativeList<float4> OutputTangents;
	//public NativeList<float2> OutputUVs;
	public Bounds MeshBounds;

	int strideX;
	int strideXY;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	int posToIndex(int3 p) => p.z * strideXY + p.y * strideX + p.x;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static float3 VertexInterpolate(float3 p1, float3 p2, float v1, float v2, float threshold)
	{
		float t = (threshold - v1) / (v2 - v1);
		return math.lerp(p1, p2, t);
	}

	int AddVertex(float3 pos) 
	{
		for (int i = 0; i < OutputVerticies.Length; i++)
		{
			bool3 isEqual = OutputVerticies[i] == pos;
			if (isEqual.x && isEqual.y && isEqual.z) 
			{
				return i;
			}
		}
		MeshBounds.Encapsulate(pos);
		OutputVerticies.Add(pos);
		OutputNormals.Add(float3.zero);

		return OutputVerticies.Length - 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void generateCube(int3 position) 
	{

		int3 pos = position;
		int3 pos1 = new int3(pos.x + 1, pos.y, pos.z);
		int3 pos2 = new int3(pos.x + 1, pos.y + 1, pos.z);
		int3 pos3 = new int3(pos.x, pos.y + 1, pos.z);
		int3 pos4 = new int3(pos.x, pos.y, pos.z + 1);
		int3 pos5 = new int3(pos.x + 1, pos.y, pos.z + 1);
		int3 pos6 = new int3(pos.x + 1, pos.y + 1, pos.z + 1);
		int3 pos7 = new int3(pos.x, pos.y + 1, pos.z + 1);


		int baseIndex = pos.z * strideXY + pos.y * strideX + pos.x;

		int i0 = baseIndex;
		int i1 = baseIndex + 1;
		int i3 = baseIndex + strideX;
		int i2 = i3 + 1;

		int i4 = baseIndex + strideXY;
		int i5 = i4 + 1;
		int i7 = i4 + strideX;
		int i6 = i7 + 1;

		int cubeIndex = 0;
		if (InputValues[i0] < Info.Threshold) cubeIndex |= 1;
		if (InputValues[i1] < Info.Threshold) cubeIndex |= 2;
		if (InputValues[i2] < Info.Threshold) cubeIndex |= 4;
		if (InputValues[i3] < Info.Threshold) cubeIndex |= 8;
		if (InputValues[i4] < Info.Threshold) cubeIndex |= 16;
		if (InputValues[i5] < Info.Threshold) cubeIndex |= 32;
		if (InputValues[i6] < Info.Threshold) cubeIndex |= 64;
		if (InputValues[i7] < Info.Threshold) cubeIndex |= 128;
		if (cubeIndex == 0 || cubeIndex == 255)
			return;
		int edges = EdgeTable[cubeIndex];
		if (edges == 0)
			return;

		float3* edgeVertex = stackalloc float3[12];

		if ((edges & 1) != 0)
			edgeVertex[0] = VertexInterpolate(pos, pos1, InputValues[i0], InputValues[i1], Info.Threshold);
		if ((edges & 2) != 0)
			edgeVertex[1] = VertexInterpolate(pos1, pos2, InputValues[i1], InputValues[i2], Info.Threshold);
		if ((edges & 4) != 0)
			edgeVertex[2] = VertexInterpolate(pos2, pos3, InputValues[i2], InputValues[i3], Info.Threshold);
		if ((edges & 8) != 0)
			edgeVertex[3] = VertexInterpolate(pos3, pos, InputValues[i3], InputValues[i0], Info.Threshold);
		if ((edges & 16) != 0)
			edgeVertex[4] = VertexInterpolate(pos4, pos5, InputValues[i4], InputValues[i5], Info.Threshold);
		if ((edges & 32) != 0)
			edgeVertex[5] = VertexInterpolate(pos5, pos6, InputValues[i5], InputValues[i6], Info.Threshold);
		if ((edges & 64) != 0)
			edgeVertex[6] = VertexInterpolate(pos6, pos7, InputValues[i6], InputValues[i7], Info.Threshold);
		if ((edges & 128) != 0)
			edgeVertex[7] = VertexInterpolate(pos7, pos4, InputValues[i7], InputValues[i4], Info.Threshold);
		if ((edges & 256) != 0)
			edgeVertex[8] = VertexInterpolate(pos, pos4, InputValues[i0], InputValues[i4], Info.Threshold);
		if ((edges & 512) != 0)
			edgeVertex[9] = VertexInterpolate(pos1, pos5, InputValues[i1], InputValues[i5], Info.Threshold);
		if ((edges & 1024) != 0)
			edgeVertex[10] = VertexInterpolate(pos2, pos6, InputValues[i2], InputValues[i6], Info.Threshold);
		if ((edges & 2048) != 0)
			edgeVertex[11] = VertexInterpolate(pos3, pos7, InputValues[i3], InputValues[i7], Info.Threshold);


		for (int i = 0; TriTable[cubeIndex * 16 + i] != -1; i += 3)
		{
			baseIndex = cubeIndex * 16 + i;
			int a = TriTable[baseIndex];
			int b = TriTable[baseIndex + 1];
			int c = TriTable[baseIndex + 2];


			int iA = AddVertex(edgeVertex[a]);
			int iB = AddVertex(edgeVertex[b]);
			int iC = AddVertex(edgeVertex[c]);

			OutputTriangles.Add(iA);
			OutputTriangles.Add(iB);
			OutputTriangles.Add(iC);

			// triangle normal
			float3 normal = math.normalize(math.cross(edgeVertex[b] - edgeVertex[a], edgeVertex[c] - edgeVertex[a]));

			// accumulate
			OutputNormals[iA] += normal;
			OutputNormals[iB] += normal;
			OutputNormals[iC] += normal;

			baseIndex = OutputVerticies.Length;

		}

	}

	public void Execute()
	{
		strideX = Info.ChunkDimensions.x + 1;
		strideXY = strideX * (Info.ChunkDimensions.y + 1);

		for (int i = 0; i < Info.ChunkDimensions.x; i++)
		{
			for (int j = 0; j < Info.ChunkDimensions.y; j++)
			{
				for (int k = 0; k < Info.ChunkDimensions.z; k++)
				{
					generateCube(new int3(i, j, k));
				}
			}
		}

		for (int i = 0; i < OutputNormals.Length; i++)
		{
			OutputNormals[i] = math.normalize(OutputNormals[i]);
		}
	}
}

/*
struct MeshGenerator : IJobParallelFor 
{
	[ReadOnly] public ChunkInfo Info;
	[ReadOnly] public NativeArray<float> InputValues;

	public NativeArray<float3> OutputVerticies;
	public NativeArray<int> OutputTriangles;

	int3 indexToPos(int i)
	{
		int layerSize = Info.ChunkDimensions.x * Info.ChunkDimensions.y;

		int z = i / layerSize;
		int remaining = i % layerSize;

		int y = remaining / Info.ChunkDimensions.x;
		int x = remaining % Info.ChunkDimensions.x;

		return new int3(x, y, z);
	}

	int posToIndex(int3 pos)
	{
		return pos.x + Info.ChunkDimensions.x * (pos.y + Info.ChunkDimensions.y * pos.z);
	}

	int posToIndex(int x, int y, int z)
	{
		return x + Info.ChunkDimensions.x * (y + Info.ChunkDimensions.y * z);
	}

	float3 VertexInterpolate(float3 p1, float3 p2, float v1, float v2, float threshold)
	{
		float t = (threshold - v1) / (v2 - v1);
		return p1 + t * (p2 - p1);
	}

	public void Execute(int i) 
	{
		NativeArray<int3> pos = new NativeArray<int3>(8, Allocator.Temp);
		pos[0] = indexToPos(i);
		pos[1] = new int3(pos[0].x + 1, pos[0].y, pos[0].z);
		pos[2] = new int3(pos[0].x + 1, pos[0].y + 1, pos[0].z);
		pos[3] = new int3(pos[0].x, pos[0].y + 1, pos[0].z);
		pos[4] = new int3(pos[0].x, pos[0].y, pos[0].z + 1);
		pos[5] = new int3(pos[0].x + 1, pos[0].y, pos[0].z + 1);
		pos[6] = new int3(pos[0].x + 1, pos[0].y + 1, pos[0].z + 1);
		pos[7] = new int3(pos[0].x, pos[0].y + 1, pos[0].z + 1);


		int cubeIndex = 0;
		if (InputValues[posToIndex(pos[0])] < Info.Threshold) cubeIndex |= 1;
		if (InputValues[posToIndex(pos[1])] < Info.Threshold) cubeIndex |= 2;
		if (InputValues[posToIndex(pos[2])] < Info.Threshold) cubeIndex |= 4;
		if (InputValues[posToIndex(pos[3])] < Info.Threshold) cubeIndex |= 8;
		if (InputValues[posToIndex(pos[4])] < Info.Threshold) cubeIndex |= 16;
		if (InputValues[posToIndex(pos[5])] < Info.Threshold) cubeIndex |= 32;
		if (InputValues[posToIndex(pos[6])] < Info.Threshold) cubeIndex |= 64;
		if (InputValues[posToIndex(pos[7])] < Info.Threshold) cubeIndex |= 128;

		int edges = Info.edgeTable[cubeIndex];
		if (edges == 0)
			return;

		NativeArray<float3> edgeVertex = new NativeArray<float3>(12, Allocator.Temp);

		if ((edges & 1) != 0)
			edgeVertex[0] = VertexInterpolate(pos[0], pos[1], InputValues[posToIndex(pos[0])], InputValues[posToIndex(pos[1])], Info.Threshold);
		if ((edges & 2) != 0)
			edgeVertex[1] = VertexInterpolate(pos[1], pos[2], InputValues[posToIndex(pos[1])], InputValues[posToIndex(pos[2])], Info.Threshold);
		if ((edges & 4) != 0)
			edgeVertex[2] = VertexInterpolate(pos[2], pos[3], InputValues[posToIndex(pos[2])], InputValues[posToIndex(pos[3])], Info.Threshold);
		if ((edges & 8) != 0)
			edgeVertex[3] = VertexInterpolate(pos[3], pos[0], InputValues[posToIndex(pos[3])], InputValues[posToIndex(pos[0])], Info.Threshold);
		if ((edges & 16) != 0)
			edgeVertex[4] = VertexInterpolate(pos[4], pos[5], InputValues[posToIndex(pos[4])], InputValues[posToIndex(pos[5])], Info.Threshold);
		if ((edges & 32) != 0)
			edgeVertex[5] = VertexInterpolate(pos[5], pos[6], InputValues[posToIndex(pos[5])], InputValues[posToIndex(pos[6])], Info.Threshold);
		if ((edges & 64) != 0)
			edgeVertex[6] = VertexInterpolate(pos[6], pos[7], InputValues[posToIndex(pos[6])], InputValues[posToIndex(pos[7])], Info.Threshold);
		if ((edges & 128) != 0)
			edgeVertex[7] = VertexInterpolate(pos[7], pos[4], InputValues[posToIndex(pos[7])], InputValues[posToIndex(pos[4])], Info.Threshold);
		if ((edges & 256) != 0)
			edgeVertex[8] = VertexInterpolate(pos[0], pos[4], InputValues[posToIndex(pos[0])], InputValues[posToIndex(pos[4])], Info.Threshold);
		if ((edges & 512) != 0)
			edgeVertex[9] = VertexInterpolate(pos[1], pos[5], InputValues[posToIndex(pos[1])], InputValues[posToIndex(pos[5])], Info.Threshold);
		if ((edges & 1024) != 0)
			edgeVertex[10] = VertexInterpolate(pos[2], pos[6], InputValues[posToIndex(pos[2])], InputValues[posToIndex(pos[6])], Info.Threshold);
		if ((edges & 2048) != 0)
			edgeVertex[11] = VertexInterpolate(pos[3], pos[7], InputValues[posToIndex(pos[3])], InputValues[posToIndex(pos[7])], Info.Threshold);

		int outputVertexIndex = 0;
		int outputTriIndex = 0;
		
		for (int j = 0; Info.triTable[cubeIndex * 16 + j] != -1; j += 3)
		{
			int a = Info.triTable[cubeIndex * 16 + j];
			int b = Info.triTable[cubeIndex * 16 + j + 1];
			int c = Info.triTable[cubeIndex * 16 + j + 2];



			OutputVerticies[i + outputVertexIndex] = (edgeVertex[a]);
			outputVertexIndex++;
			OutputVerticies[i + outputVertexIndex] = (edgeVertex[b]);
			outputVertexIndex++;
			OutputVerticies[i + outputVertexIndex] = (edgeVertex[c]);
			outputVertexIndex++;

			OutputTriangles[i + outputTriIndex] = i + outputVertexIndex - 2;
			outputTriIndex++;
			OutputTriangles[i + outputTriIndex] = i + outputVertexIndex - 1;
			outputTriIndex++;
			OutputTriangles[i + outputTriIndex] = i + outputVertexIndex;
			outputTriIndex++;
		}

		for (int j = outputTriIndex; j < 15; j++)
		{
			OutputTriangles[i + j] = -1;
		}
	}
}
*/