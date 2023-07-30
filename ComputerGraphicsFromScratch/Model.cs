using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ComputerGraphicsFromScratch;

internal class Model
{
	private readonly List<Vector3> Vertices;
	private readonly List<Triangle> Triangles;
	private readonly Vector3 BoundsCenter;
	private readonly float BoundsRadius;

	public Model(List<Vector3> vertices, List<Triangle> triangles, Vector3 boundsCenter, float boundsRadius)
	{
		Vertices = vertices;
		Triangles = triangles;
		BoundsCenter = boundsCenter;
		BoundsRadius = boundsRadius;
	}

	public IReadOnlyList<Vector3> GetVertices()
	{
		return Vertices;
	}

	public IReadOnlyList<Triangle> GetTriangles()
	{
		return Triangles;
	}

	public Vector3 GetBoundsCenter()
	{
		return BoundsCenter;
	}

	public float GetBoundsRadius()
	{
		return BoundsRadius;
	}
}
