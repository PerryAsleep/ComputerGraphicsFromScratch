using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace ComputerGraphicsFromScratch;

internal sealed class Rasterizer
{
	private const int NumTextures = 2;
	private int TextureIndex;
	private Texture2D[] Textures;
	private uint[] TextureData;
	private uint[] ClearData;

	private int CanvasW;
	private int CanvasH;

	private readonly GraphicsDevice Graphics;
	private readonly Camera Camera;
	private readonly IReadOnlyList<Instance> Instances;

	private float[] DepthBuffer;

	#region Initialization

	public Rasterizer(
		GraphicsDevice graphicsDevice,
		int w,
		int h,
		Camera camera,
		IReadOnlyList<Instance> instances)
	{
		Graphics = graphicsDevice;
		UpdateViewport(w, h);

		Camera = camera;
		Instances = instances;
	}

	public void UpdateViewport(int w, int h)
	{
		CanvasW = w;
		CanvasH = h;
		Textures = new Texture2D[NumTextures];
		for (var i = 0; i < NumTextures; i++)
		{
			Textures[i] = new Texture2D(Graphics, CanvasW, CanvasH, false, SurfaceFormat.Color);
		}

		TextureData = new uint[CanvasW * CanvasH];
		ClearData = new uint[CanvasW * CanvasH];
		DepthBuffer = new float[CanvasW * CanvasH];
	}

	#endregion Initialization

	#region Canvas

	private static uint ToRgba(Color color)
	{
		return color.R
		       | ((uint)color.G << 8)
		       | ((uint)color.B << 16)
		       | ((uint)color.A << 24);
	}

	private void PutPixel(int canvasX, int canvasY, Color color)
	{
		var x = canvasX + (CanvasW >> 1);
		var y = (CanvasH >> 1) - canvasY - 1;

		if (x < 0 || x >= CanvasW || y < 0 || y >= CanvasH)
		{
			return;
		}

		TextureData[y * CanvasW + x] = ToRgba(color);
	}

	private Point<int> ViewportToCanvas(Point<float> p2d)
	{
		return new Point<int>((int)(p2d.X * CanvasW / Camera.GetProjectionPlaneW()),
			(int)(p2d.Y * CanvasH / Camera.GetProjectionPlaneH()));
	}

	private Point<int> ViewportToCanvas(Point<int> p2d)
	{
		return new Point<int>((int)(p2d.X * CanvasW / Camera.GetProjectionPlaneW()),
			(int)(p2d.Y * CanvasH / Camera.GetProjectionPlaneH()));
	}

	#endregion Canvas

	#region Color

	private static Color MultiplyColor(Color color, float factor)
	{
		factor = Math.Clamp(factor, 0.0f, 1.0f);
		return new Color((byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor));
	}

	#endregion Color

	#region Rasterization

	private List<int> SortedVertexIndexes(IReadOnlyList<int> vertexIndexes, IReadOnlyList<Point<int>> projectedVertices)
	{
		var indexes = new List<int> { 0, 1, 2 };
		if (projectedVertices[vertexIndexes[indexes[1]]].Y < projectedVertices[vertexIndexes[indexes[0]]].Y)
			(indexes[0], indexes[1]) = (indexes[1], indexes[0]);
		if (projectedVertices[vertexIndexes[indexes[2]]].Y < projectedVertices[vertexIndexes[indexes[0]]].Y)
			(indexes[0], indexes[2]) = (indexes[2], indexes[0]);
		if (projectedVertices[vertexIndexes[indexes[2]]].Y < projectedVertices[vertexIndexes[indexes[1]]].Y)
			(indexes[1], indexes[2]) = (indexes[2], indexes[1]);
		return indexes;
	}

	private Vector3 ComputeTriangleNormal(Vector3 v0, Vector3 v1, Vector3 v2)
	{
		var v0v1 = v1 + -1 * v0;
		var v0v2 = v2 + -1 * v0;
		return Vector3.Cross(v0v1, v0v2);
	}

	/// <summary>
	/// Generic interpolation function.
	/// </summary>
	/// <remarks>
	/// This function dynamically allocates a List every time it is called.
	/// This seems terrible but for now I am following along with the book.
	/// </remarks>
	private static List<float> Interpolate(int i0, float d0, int i1, float d1)
	{
		var values = new List<float>();
		if (i0 == i1)
		{
			values.Add(d0);
			return values;
		}

		var a = (d1 - d0) / (i1 - i0);
		var d = d0;
		for (var i = i0; i <= i1; i++)
		{
			values.Add(d);
			d += a;
		}

		return values;
	}

	private static (List<float>, List<float>) EdgeInterpolate(int y0, float v0, int y1, float v1, int y2, float v2)
	{
		var v01 = Interpolate(y0, v0, y1, v1);
		var v12 = Interpolate(y1, v1, y2, v2);
		var v02 = Interpolate(y0, v0, y2, v2);
		v01.RemoveAt(v01.Count - 1);
		var v012 = new List<float>();
		v012.AddRange(v01);
		v012.AddRange(v12);
		return (v02, v012);
	}

	private Point<int> ProjectVertex(Vector3 v)
	{
		var ppz = Camera.GetProjectionPlaneDistance();
		return ViewportToCanvas(new Point<float>(v.X * ppz / v.Z, v.Y * ppz / v.Z));
	}

	private void DrawLine(Point<int> p0, Point<int> p1, Color color)
	{
		var dx = p1.X - p0.X;
		var dy = p1.Y - p0.Y;

		if (Math.Abs(dx) > Math.Abs(dy))
		{
			// The line is more horizontal. Make sure it's left to right.
			if (dx < 0)
				(p0, p1) = (p1, p0);

			// Compute the Y values and draw.
			var ys = Interpolate(p0.X, p0.Y, p1.X, p1.Y);
			for (var x = p0.X; x <= p1.X; x++)
			{
				PutPixel(x, (int)ys[(x - p0.X) | 0], color);
			}
		}
		else
		{
			// The line is more vertical. Make sure it's bottom to top.
			if (dy < 0)
				(p0, p1) = (p1, p0);

			// Compute the X values and draw.
			var xs = Interpolate(p0.Y, p0.X, p1.Y, p1.X);
			for (var y = p0.Y; y <= p1.Y; y++)
			{
				PutPixel((int)xs[(y - p0.Y) | 0], y, color);
			}
		}
	}

	private void DrawWireFrameTriangle(Point<int> p0, Point<int> p1, Point<int> p2, Color color)
	{
		DrawLine(p0, p1, color);
		DrawLine(p1, p2, color);
		DrawLine(p0, p2, color);
	}

	private void DrawFilledTriangle(Point<int> p0, Point<int> p1, Point<int> p2, Color color)
	{
		// Sort the points from bottom to top.
		if (p1.Y < p0.Y)
			(p0, p1) = (p1, p0);
		if (p2.Y < p0.Y)
			(p0, p2) = (p2, p0);
		if (p2.Y < p1.Y)
			(p1, p2) = (p2, p1);

		// Compute X coordinates of the edges.
		var x01 = Interpolate(p0.Y, p0.X, p1.Y, p1.X);
		var x12 = Interpolate(p1.Y, p1.X, p2.Y, p2.X);
		var x02 = Interpolate(p0.Y, p0.X, p2.Y, p2.X);

		// Merge the two short sides.
		if (x01.Count > 0)
			x01.RemoveAt(x01.Count - 1);
		x01.AddRange(x12);

		// Determine which is left and which is right.
		List<float> xLeft, xRight;
		var m = (x02.Count >> 1) | 0;
		if (x02[m] < x01[m])
		{
			xLeft = x02;
			xRight = x01;
		}
		else
		{
			xLeft = x01;
			xRight = x02;
		}

		// Draw horizontal segments.
		for (var y = p0.Y; y <= p2.Y; y++)
		{
			for (var x = (int)xLeft[y - p0.Y]; x <= (int)xRight[y - p0.Y]; x++)
			{
				PutPixel(x, y, color);
			}
		}
	}

	private void DrawShadedTriangle(Point<int> p0, Point<int> p1, Point<int> p2, Color color)
	{
		// Sort the points from bottom to top.
		if (p1.Y < p0.Y)
			(p0, p1) = (p1, p0);
		if (p2.Y < p0.Y)
			(p0, p2) = (p2, p0);
		if (p2.Y < p1.Y)
			(p1, p2) = (p2, p1);

		// Compute X coordinates and H values of the edges.
		var x01 = Interpolate(p0.Y, p0.X, p1.Y, p1.X);
		var h01 = Interpolate(p0.Y, p0.H, p1.Y, p1.H);

		var x12 = Interpolate(p1.Y, p1.X, p2.Y, p2.X);
		var h12 = Interpolate(p1.Y, p1.H, p2.Y, p2.H);

		var x02 = Interpolate(p0.Y, p0.X, p2.Y, p2.X);
		var h02 = Interpolate(p0.Y, p0.H, p2.Y, p2.H);

		// Merge the two short sides.
		if (x01.Count > 0)
			x01.RemoveAt(x01.Count - 1);
		x01.AddRange(x12);

		if (h01.Count > 0)
			h01.RemoveAt(h01.Count - 1);
		h01.AddRange(h12);

		// Determine which is left and which is right.
		List<float> xLeft, xRight, hLeft, hRight;
		var m = (x02.Count >> 1) | 0;
		if (x02[m] < x01[m])
		{
			xLeft = x02;
			xRight = x01;
			hLeft = h02;
			hRight = h01;
		}
		else
		{
			xLeft = x01;
			xRight = x02;
			hLeft = h01;
			hRight = h02;
		}

		// Draw horizontal segments.
		for (var y = p0.Y; y <= p2.Y; y++)
		{
			var xl = (int)xLeft[y - p0.Y] | 0;
			var xr = (int)xRight[y - p0.Y] | 0;
			var hSegment = Interpolate(xl, hLeft[y - p0.Y], xr, hRight[y - p0.Y]);

			for (var x = xl; x <= xr; x++)
			{
				PutPixel(x, y, MultiplyColor(color, hSegment[x - xl]));
			}
		}
	}

	private void RenderTriangle(Triangle triangle, IReadOnlyList<Vector3> vertices, List<Point<int>> projectedVertices,
		bool drawOutlines)
	{
		// Sort by projected point Y.
		var indexes = SortedVertexIndexes(triangle.VertexIndices, projectedVertices);

		var i0 = indexes[0];
		var i1 = indexes[1];
		var i2 = indexes[2];

		var v0 = vertices[triangle.VertexIndices[i0]];
		var v1 = vertices[triangle.VertexIndices[i1]];
		var v2 = vertices[triangle.VertexIndices[i2]];

		// Compute triangle normal. Use the unsorted vertices, otherwise the winding of the points may change.
		var normal = ComputeTriangleNormal(
			vertices[triangle.VertexIndices[0]],
			vertices[triangle.VertexIndices[1]],
			vertices[triangle.VertexIndices[2]]);

		// Backface culling.
		var vertexToCamera =
			-1.0f * vertices[triangle.VertexIndices[0]]; // Should be Subtract(camera.position, vertices[triangle.indexes[0]])
		if (Vector3.Dot(vertexToCamera, normal) <= 0.0f)
		{
			return;
		}

		// Get attribute values (X, 1/Z) at the vertices.
		var p0 = projectedVertices[triangle.VertexIndices[i0]];
		var p1 = projectedVertices[triangle.VertexIndices[i1]];
		var p2 = projectedVertices[triangle.VertexIndices[i2]];

		// Compute attribute values at the edges.
		var (x02, x012) = EdgeInterpolate(p0.Y, p0.X, p1.Y, p1.X, p2.Y, p2.X);
		var (iz02, iz012) = EdgeInterpolate(p0.Y, 1.0f / v0.Z, p1.Y, 1.0f / v1.Z, p2.Y, 1.0f / v2.Z);

		// Determine which is left and which is right.
		var m = (x02.Count >> 1) | 0;
		List<float> xLeft, xRight, izLeft, izRight;
		if (x02[m] < x012[m])
		{
			(xLeft, xRight) = (x02, x012);
			(izLeft, izRight) = (iz02, iz012);
		}
		else
		{
			(xLeft, xRight) = (x012, x02);
			(izLeft, izRight) = (iz012, iz02);
		}

		// Draw horizontal segments.
		for (var y = p0.Y; y <= p2.Y; y++)
		{
			var xl = (int)xLeft[y - p0.Y];
			var xr = (int)xRight[y - p0.Y];

			// Interpolate attributes for this scanline.
			var zl = izLeft[y - p0.Y];
			var zr = izRight[y - p0.Y];
			var zScan = Interpolate(xl, zl, xr, zr);

			for (var x = xl; x <= xr; x++)
			{
				if (UpdateDepthBufferIfCloser(x, y, zScan[x - xl]))
				{
					PutPixel(x, y, triangle.Color);
				}
			}
		}

		if (drawOutlines)
		{
			var outlineColor = MultiplyColor(triangle.Color, 0.75f);
			DrawLine(p0, p1, outlineColor);
			DrawLine(p0, p2, outlineColor);
			DrawLine(p2, p1, outlineColor);
		}
	}

	#endregion Rasterization

	#region Clipping

	private void ClipTriangle(Triangle triangle, Plane plane, List<Triangle> triangles, List<Vector3> vertices)
	{
		var a = vertices[triangle.VertexIndex0];
		var b = vertices[triangle.VertexIndex1];
		var c = vertices[triangle.VertexIndex2];

		var ad = Vector3.Dot(plane.Normal, a) + plane.D;
		var bd = Vector3.Dot(plane.Normal, b) + plane.D;
		var cd = Vector3.Dot(plane.Normal, c) + plane.D;

		var inCount = 0;
		if (ad > 0.0f)
			inCount++;
		if (bd > 0.0f)
			inCount++;
		if (cd > 0.0f)
			inCount++;

		if (inCount == 0)
		{
			return;
		}
		else if (inCount == 3)
		{
			// The triangle is fully in front of the plane.
			triangles.Add(triangle);
		}
		// TODO: Implement clipping of triangles which overlap clipping planes.
		// This requires not only generated new triangles, but new vertices.
		// This is missing from the sample code:
		// https://github.com/ggambetta/computer-graphics-from-scratch/issues/30
		//else if (inCount == 1)
		//{
		//	if (ad < 0.0f)
		//	{
		//		Utils.Swap(ref ad, ref bd);
		//		Utils.Swap(ref a, ref b);
		//	}
		//	if (ad < 0.0f)
		//	{
		//		Utils.Swap(ref ad, ref cd);
		//		Utils.Swap(ref a, ref c);
		//	}

		//	var rb = new Ray(a, b - a);
		//	var fb = rb.Intersects(plane);
		//	var rc = new Ray(a, c - a);
		//	var fc = rc.Intersects(plane);
		//	if (fb != null && fc != null)
		//	{
		//		var bp = a + (fb.Value * rb.Direction);
		//		var cp = a + (fc.Value * rc.Direction);
		//		triangles.Add(new Triangle());
		//	}
		//}
	}

	private Model TransformAndClip(IReadOnlyList<Plane> clippingPlanes, Model model, float scale, Matrix transform)
	{
		// Transform the bounding sphere and attempt to early discard.
		var center = Vector3.Transform(model.GetBoundsCenter(), transform);
		var radius = model.GetBoundsRadius() * scale;
		foreach (var clippingPlane in clippingPlanes)
		{
			var distance = Vector3.Dot(clippingPlane.Normal, center) + clippingPlane.D;
			if (distance < -radius)
			{
				return null;
			}
		}

		// Apply modelview transform.
		var vertices = new List<Vector3>();
		foreach (var vertex in model.GetVertices())
			vertices.Add(Transform(vertex, transform));

		// Clip the entire model against each successive plane.
		var triangles = new List<Triangle>();
		triangles.AddRange(model.GetTriangles());
		foreach (var clippingPlane in clippingPlanes)
		{
			var newTriangles = new List<Triangle>();
			foreach (var triangle in triangles)
			{
				ClipTriangle(triangle, clippingPlane, newTriangles, vertices);
			}

			triangles = newTriangles;
		}

		return new Model(vertices, triangles, center, model.GetBoundsRadius());
	}

	#endregion Clipping

	#region Depth Buffer

	private void ClearDepthBuffer()
	{
		Array.Clear(DepthBuffer);
	}

	private bool UpdateDepthBufferIfCloser(int canvasX, int canvasY, float inverseZ)
	{
		var x = canvasX + (CanvasW >> 1);
		var y = (CanvasH >> 1) - canvasY - 1;

		if (x < 0 || x >= CanvasW || y < 0 || y >= CanvasH)
		{
			return false;
		}

		var i = x + CanvasW * y;
		if (DepthBuffer[i] < inverseZ)
		{
			DepthBuffer[i] = inverseZ;
			return true;
		}

		return false;
	}

	#endregion Depth Buffer

	private Vector3 Transform(Vector3 vector, Matrix transform)
	{
		var v4 = Vector4.Transform(new Vector4(vector.X, vector.Y, vector.Z, 1.0f), transform);
		return new Vector3(v4.X, v4.Y, v4.Z);
	}

	#region Scene

	private void RenderModel(Model model, bool drawOutlines)
	{
		var vertices = model.GetVertices();
		var triangles = model.GetTriangles();
		var projectedVertices = new List<Point<int>>(vertices.Count);
		for (var i = 0; i < vertices.Count; i++)
		{
			projectedVertices.Add(ProjectVertex(vertices[i]));
		}

		for (var i = 0; i < triangles.Count; i++)
		{
			RenderTriangle(triangles[i], model.GetVertices(), projectedVertices, drawOutlines);
		}
	}

	private void RenderScene()
	{
		var viewMatrix = Camera.GetViewMatrix();
		foreach (var instance in Instances)
		{
			var transform = instance.GetTransform() * viewMatrix;
			var clipped = TransformAndClip(Camera.GetClippingPlanes(), instance.GetModel(), instance.Scale, transform);
			if (clipped != null)
			{
				instance.DebugSetNumRenderedTriangles(clipped.GetTriangles().Count);
				RenderModel(clipped, instance.DebugDrawOutlines);
			}
			else
			{
				instance.DebugSetNumRenderedTriangles(0);
			}
		}
	}

	#endregion Scene

	public void Update(GameTime _)
	{
		var texture = Textures[TextureIndex];
		Array.Copy(ClearData, TextureData, CanvasW * CanvasH);

		ClearDepthBuffer();
		RenderScene();

		texture.SetData(TextureData);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(Textures[TextureIndex], new Vector2(0, 0), null, Color.White);
		TextureIndex = (TextureIndex + 1) % NumTextures;
	}
}
