using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
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
				new GameObject(x + " " + y).transform.position = new Vector3(x, 0, y);

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
		GenerateChunks(10);
	}
}
