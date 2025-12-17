using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainChunk : MonoBehaviour
{
	public int Width;
	public int Height;
	public int Depth;

	public bool isGenerating = false;
	public bool isDoneGenerating = false;
	public IEnumerator Generate() 
	{

		float startTime = Time.realtimeSinceStartup;

		isGenerating = true;
		ChunkManager.instance.NumGenerating++;
		
        NativeArray<float> outputValues = new NativeArray<float>((Depth + 1)*(Width + 1)*(Height + 1), Allocator.Persistent);

		ChunkInfo info = new ChunkInfo(new int3(Depth, Height, Width), new float3(transform.position.x, transform.position.y, transform.position.z), 0f);

		NativeArray<float> heightMap;

		if (ChunkManager.instance.HeightMaps.ContainsKey(transform.position)) 
		{
			heightMap = ChunkManager.instance.HeightMaps[transform.position];
		}
		else 
		{
			heightMap = new NativeArray<float>((Depth + 1) * (Width + 1), Allocator.Persistent);


            var heightMapGenerator = new HeightMapGenerator()
			{
				NoiseSettings = ChunkManager.instance.NoiseSettings,
				Info = info,
				HeightMaps = heightMap,
			};

			JobHandle heightMapJobHandle = heightMapGenerator.Schedule((Depth + 1) * (Width + 1), Depth + 1);
            yield return new WaitUntil(() => { return heightMapJobHandle.IsCompleted; });
			heightMapJobHandle.Complete();

			ChunkManager.instance.HeightMaps.Add(transform.position, heightMap);
        }

		var valuesGenerator = new ValuesPopulator()
		{
			Info = info,
			OutputValues = outputValues,
			HeightMaps = heightMap,
			NoiseSettings = ChunkManager.instance.NoiseSettings
		};

		JobHandle valueJobHandle = valuesGenerator.Schedule((Depth + 1) * (Width + 1) * (Height + 1), (Depth + 1) * (Width + 1));


        NativeList<float3> outputVerticies = new NativeList<float3>(Depth * Width * Height * 12, Allocator.Persistent);
		NativeList<int> outputTriangles = new NativeList<int>(Depth * Width * Height * 3, Allocator.Persistent);

		var meshGenerator = new MeshGenerator()
		{
			Info = info,
			InputValues = outputValues,
			OutputTriangles = outputTriangles,
			OutputVerticies = outputVerticies,
			TriTable = ChunkManager.triTable,
			EdgeTable = ChunkManager.edgeTable,
		};
		//meshGenerator.Run();
		

		JobHandle meshJobHandle = meshGenerator.Schedule(valueJobHandle);

		yield return new WaitUntil(() => { return meshJobHandle.IsCompleted; });
		meshJobHandle.Complete();
		outputValues.Dispose();
		float3[] verts = outputVerticies.AsArray().ToArray();
		outputVerticies.Dispose();
		List<int> triangles = new List<int>(outputTriangles.AsArray());
		outputTriangles.Dispose();
		Mesh mesh = new Mesh();
		Vector3[] vertsVectors = new Vector3[verts.Length];
		for (int i = 0; i < verts.Length; i++)
		{
			vertsVectors[i] = new Vector3(verts[i].x, verts[i].y, verts[i].z);
		}
		
		mesh.vertices = vertsVectors;


		//triangles.RemoveAll((int val) => { return val == -1; });

		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshRenderer>().material = ChunkManager.instance.worldMaterial;



		isGenerating = false;
		isDoneGenerating = true;
        ChunkManager.instance.NumGenerating--;

		float endTime = Time.realtimeSinceStartup;
		float deltaTime = endTime - startTime;
		ChunkManager.instance.GenerationTimes.Add(deltaTime);
    }
    public void StartGenerating()
    {
		StartCoroutine(Generate());
    }
    private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		if (isGenerating)
			Gizmos.color = Color.green;
		if (isDoneGenerating)
			Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(new Vector3(Width, Height, Depth) / 2 + transform.position, new Vector3(Width, Height, Depth));
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