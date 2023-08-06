using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ComputerGraphicsFromScratch;

internal readonly struct Triangle
{
	public readonly int VertexIndex0;
	public readonly int VertexIndex1;
	public readonly int VertexIndex2;
	public readonly Color Color;
	public readonly List<int> VertexIndices;
	public readonly List<Vector3> Normals;

	public Triangle(int vertexIndex0, int vertexIndex1, int vertexIndex2, Color color, List<Vector3> normals)
	{
		VertexIndices = new List<int> { vertexIndex0, vertexIndex1, vertexIndex2 };
		VertexIndex0 = vertexIndex0;
		VertexIndex1 = vertexIndex1;
		VertexIndex2 = vertexIndex2;
		Color = color;
		Normals = normals;
	}
}
