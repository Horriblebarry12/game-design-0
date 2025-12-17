using Unity.Burst;
using Unity.Jobs;

[BurstCompile(FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
public struct HeightMapGenerator : IJobParallelFor 
{
	public void Execute(int i) 
	{
	
	}
}
