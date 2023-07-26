using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ComputerGraphicsFromScratch;

internal class Model
{
	private readonly List<Vector3> Vertices;
	private readonly List<Triangle> Triangles;

	public Model(List<Vector3> vertices, List<Triangle> triangles)
	{
		Vertices = vertices;
		Triangles = triangles;
	}

	public IReadOnlyList<Vector3> GetVertices()
	{
		return Vertices;
	}

	public IReadOnlyList<Triangle> GetTriangles()
	{
		return Triangles;
	}
}
