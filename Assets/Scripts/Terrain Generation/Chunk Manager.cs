using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
	[Header("Settings")]
	public Material worldMaterial;
	public int MaxGeneratingChunks = 12;
	public Transform Reference;
	public bool RandomSeed = true;
	public NoiseParamaters NoiseSettings;
	
	public static ChunkManager instance;
	[Space]
	[Header("Global References")]
	public NoiseParamaters GlobalNoiseSettings;
	public int numGenerating = 0;
	public Dictionary<Vector3, TerrainChunk> AllChunks = new Dictionary<Vector3, TerrainChunk>();
    bool isGenerating = false;
	private IEnumerator GenerateChunks(int radius) 
	{
		isGenerating = true;
		float x = 0.0f, y = 0.0f;

		float dist = 0.0f, xDir = 0.0f, yDir = 0.0f;
		float distTravel = 1.0f;
		int timeTraveled = 0;
		xDir = 1.0f;
		while (x < radius && y < radius) 
		{
			while (dist < distTravel)
			{
				dist++;
                for (int j= -1; j <= 5; j++)
                {
					Vector3 actuallPos = new Vector3((x+Mathf.Round((Reference.position.x)/32)) * 32, (j+Mathf.Round((Reference.position.y)/32)) * 32, (y + Mathf.Round((Reference.position.z) / 32)) * 32);
					if (AllChunks.ContainsKey(actuallPos))
						continue;
					if (numGenerating > MaxGeneratingChunks)
						yield return new WaitUntil(() => { return numGenerating < MaxGeneratingChunks; });
                    GameObject obj = new GameObject(x + " " + y);
                    obj.transform.position = actuallPos;
                    TerrainChunk chunk = obj.AddComponent<TerrainChunk>();
                    chunk.Depth = 32;
                    chunk.Height = 32;
                    chunk.Width = 32;
					AllChunks.Add(actuallPos, chunk);
					chunk.StartGenerating();
                }
				x += xDir;
				y += yDir;

			}
			timeTraveled++;
			dist = 0;
			var temp = xDir;
			xDir = -yDir;
			yDir = temp;
			if (timeTraveled == 2) 
			{
				distTravel++;
				timeTraveled = 0;
			}
		}
		isGenerating = false;
	}

	private void Start()
	{
		instance = this;
		if (RandomSeed)
			NoiseSettings.Seed = UnityEngine.Random.Range(float.MinValue, float.MaxValue);
		StartCoroutine(GenerateChunks(10));
		//GenerateChunks(10);

    }

	private void Update()
	{
		if (!isGenerating)
			StartCoroutine(GenerateChunks(10));
			//GenerateChunks(10);

    }
}
