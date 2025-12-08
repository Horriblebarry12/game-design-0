using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
	public Material worldMaterial;

	public static ChunkManager instance;

	private void GenerateChunks(int radius) 
	{
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
                for (int z= -1; z < 1; z++)
                {
                    GameObject obj = new GameObject(x + " " + y);
                    obj.transform.position = new Vector3(x * 32, z * 32, y * 32);
                    TerrainChuck chunk = obj.AddComponent<TerrainChuck>();
                    chunk.Depth = 32;
                    chunk.Height = 32;
                    chunk.Width = 32;

                    StartCoroutine(chunk.Generate());
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

	}

	private void Start()
	{
		instance = this;

        GenerateChunks(10);
	}
}
