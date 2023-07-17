using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace ComputerGraphicsFromScratch;

internal sealed class RayTracer
{
	private const int NumTextures = 2;
	private int TextureIndex;
	private readonly Texture2D[] Textures;
	private readonly uint[] TextureData;

	private const float Epsilon = 0.01f;
	private const int RecursionDepth = 3;

	private readonly int CanvasW;
	private readonly int CanvasH;
	private readonly float ViewportW;
	private readonly float ViewportH;
	private readonly float ViewportZ = 1.0f;

	private readonly Color BackgroundColor = Color.Black;
	private readonly Vector3 CameraPosition = Vector3.Zero;

	private readonly List<Sphere> Spheres;
	private readonly List<Light> Lights;

	public RayTracer(GraphicsDevice graphicsDevice, int w, int h)
	{
		CanvasW = w;
		CanvasH = h;

		Spheres = new List<Sphere>
		{
			new(new Vector3(0, -1, 3), 1, Color.Red, 500, 0.2f),
			new(new Vector3(2, 0, 4), 1, Color.Blue, 500, 0.4f),
			new(new Vector3(-2, 0, 4), 1, Color.Green, 10, 0.3f),
			new(new Vector3(0, -5001, 0), 5000, Color.Yellow, 1000, 0.5f),
		};

		Lights = new List<Light>
		{
			new(Light.LightType.Ambient, 0.2f, Vector3.Zero),
			new(Light.LightType.Point, 0.6f, new Vector3(2.0f, 1.0f, 0.0f)),
			new(Light.LightType.Directional, 0.2f, new Vector3(1.0f, 4.0f, 4.0f)),
		};

		ViewportW = (float)w / h;
		ViewportH = 1.0f;

		// Set up the textures.
		Textures = new Texture2D[NumTextures];
		for (var i = 0; i < NumTextures; i++)
		{
			Textures[i] = new Texture2D(graphicsDevice, w, h, false, SurfaceFormat.Color);
		}

		TextureData = new uint[CanvasW * CanvasH];
	}

	public void Update(GameTime _)
	{
		var texture = Textures[TextureIndex];

		var halfCanvasW = CanvasW >> 1;
		var halfCanvasH = CanvasH >> 1;

		for (var x = -halfCanvasW; x < halfCanvasW; x++)
		{
			for (var y = -halfCanvasH; y < halfCanvasH; y++)
			{
				var direction = CanvasToViewport(x, y);
				var color = TraceRay(CameraPosition, direction, 1, float.PositiveInfinity, RecursionDepth);
				PutPixel(x, y, color);
			}
		}

		texture.SetData(TextureData);
	}

	private void PutPixel(int canvasX, int canvasY, Color color)
	{
		var x = canvasX + (CanvasW >> 1);
		var y = (CanvasH >> 1) - canvasY - 1;
		TextureData[y * CanvasW + x] = ToRgba(color);
	}

	private Vector3 CanvasToViewport(int canvasX, int canvasY)
	{
		return new Vector3(canvasX * ViewportW / CanvasW, canvasY * ViewportH / CanvasH, ViewportZ);
	}

	private Vector3 ReflectRay(Vector3 v1, Vector3 v2)
	{
		return v2 * (2.0f * Vector3.Dot(v1, v2)) - v1;
	}

	private static (float, float) IntersectRaySphere(Vector3 origin, Vector3 direction, Sphere sphere)
	{
		var oc = origin - sphere.Center;

		var k1 = Vector3.Dot(direction, direction);
		var k2 = 2.0f * Vector3.Dot(oc, direction);
		var k3 = Vector3.Dot(oc, oc) - (float)(sphere.Radius * sphere.Radius);

		var discriminant = k2 * k2 - 4.0f * k1 * k3;
		if (discriminant < 0.0f)
			return (float.PositiveInfinity, float.PositiveInfinity);

		var t1 = (-k2 + (float)Math.Sqrt(discriminant)) / (2.0f * k1);
		var t2 = (-k2 - (float)Math.Sqrt(discriminant)) / (2.0f * k1);

		return (t1, t2);
	}

	private (Sphere, float) ClosestIntersection(Vector3 origin, Vector3 direction, float minT, float maxT)
	{
		var closestT = float.PositiveInfinity;
		Sphere closestSphere = null;

		for (var sphereIndex = 0; sphereIndex < Spheres.Count; sphereIndex++)
		{
			var ts = IntersectRaySphere(origin, direction, Spheres[sphereIndex]);
			if (ts.Item1 < closestT && minT < ts.Item1 && ts.Item1 < maxT)
			{
				closestT = ts.Item1;
				closestSphere = Spheres[sphereIndex];
			}

			if (ts.Item2 < closestT && minT < ts.Item2 && ts.Item2 < maxT)
			{
				closestT = ts.Item2;
				closestSphere = Spheres[sphereIndex];
			}
		}

		if (closestSphere != null)
			return (closestSphere, closestT);
		return (null, 0.0f);
	}

	private Color TraceRay(Vector3 origin, Vector3 direction, float minT, float maxT, int depth)
	{
		var (closestSphere, closestT) = ClosestIntersection(origin, direction, minT, maxT);

		if (closestSphere == null)
			return BackgroundColor;

		var point = origin + direction * closestT;
		var normal = point - closestSphere.Center;
		normal.Normalize();

		var view = direction * -1.0f;
		var lighting = ComputeLighting(point, normal, view, closestSphere.Specular);
		var localColor = MultiplyColor(closestSphere.Color, lighting);

		if (closestSphere.Reflective <= 0.0f || depth <= 0)
			return localColor;

		var reflectedRay = ReflectRay(view, normal);
		var reflectedColor = TraceRay(point, reflectedRay, Epsilon, float.PositiveInfinity, depth - 1);

		return AddColor(MultiplyColor(localColor, 1.0f - closestSphere.Reflective),
			MultiplyColor(reflectedColor, closestSphere.Reflective));
	}

	private float ComputeLighting(Vector3 point, Vector3 normal, Vector3 view, int specular)
	{
		var intensity = 0.0f;

		var normalLen = normal.Length();
		var viewLen = view.Length();

		for (var lightIndex = 0; lightIndex < Lights.Count; lightIndex++)
		{
			var light = Lights[lightIndex];
			if (light.Type == Light.LightType.Ambient)
			{
				intensity += light.Intensity;
			}
			else
			{
				Vector3 lightVector;
				var tMax = 1.0f;
				if (light.Type == Light.LightType.Point)
				{
					lightVector = light.Position - point;
				}
				else
				{
					lightVector = light.Position;
					tMax = float.PositiveInfinity;
				}

				// Shadow check
				var (blocker, _) = ClosestIntersection(point, lightVector, Epsilon, tMax);
				if (blocker != null)
					continue;

				// Diffuse reflection
				var nDotL = Vector3.Dot(normal, lightVector);
				if (nDotL > 0)
				{
					intensity += light.Intensity * nDotL / (normalLen * lightVector.Length());
				}

				// Specular reflection
				if (specular >= 0)
				{
					var vecR = ReflectRay(lightVector, normal);
					var rDotV = Vector3.Dot(vecR, view);
					if (rDotV > 0.0f)
					{
						intensity += light.Intensity * (float)Math.Pow(rDotV / (vecR.Length() * viewLen), specular);
					}
				}
			}
		}

		return intensity;
	}

	private static Color MultiplyColor(Color color, float factor)
	{
		factor = Math.Clamp(factor, 0.0f, 1.0f);
		return new Color((byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor));
	}

	private static Color AddColor(Color c1, Color c2)
	{
		var r = (byte)Math.Min(byte.MaxValue, c1.R + c2.R);
		var g = (byte)Math.Min(byte.MaxValue, c1.G + c2.G);
		var b = (byte)Math.Min(byte.MaxValue, c1.B + c2.B);
		return new Color(r, g, b);
	}

	private static uint ToRgba(Color color)
	{
		return color.R
		       | ((uint)color.G << 8)
		       | ((uint)color.B << 16)
		       | ((uint)color.A << 24);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(Textures[TextureIndex], new Vector2(0, 0), null, Color.White);
		TextureIndex = (TextureIndex + 1) % NumTextures;
	}
}
