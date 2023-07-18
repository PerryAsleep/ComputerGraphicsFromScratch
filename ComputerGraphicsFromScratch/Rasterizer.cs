using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComputerGraphicsFromScratch;

internal sealed class Rasterizer
{
	private const int NumTextures = 2;
	private int TextureIndex;
	private readonly Texture2D[] Textures;
	private readonly uint[] TextureData;
	private readonly uint[] ClearData;

	private readonly int CanvasW;
	private readonly int CanvasH;

	private struct Point
	{
		public readonly int X;
		public readonly int Y;

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	public Rasterizer(GraphicsDevice graphicsDevice, int w, int h)
	{
		CanvasW = w;
		CanvasH = h;

		// Set up the textures.
		Textures = new Texture2D[NumTextures];
		for (var i = 0; i < NumTextures; i++)
		{
			Textures[i] = new Texture2D(graphicsDevice, w, h, false, SurfaceFormat.Color);
		}

		TextureData = new uint[CanvasW * CanvasH];
		ClearData = new uint[CanvasW * CanvasH];
	}

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

	#endregion Canvas

	#region Rasterization

	/// <summary>
	/// Generic interpolation function.
	/// </summary>
	/// <remarks>
	/// This function dynamically allocates a List every time it is called.
	/// This seems terrible but for now I am following along with the book.
	/// </remarks>
	private IReadOnlyList<int> Interpolate(int i0, float d0, int i1, float d1)
	{
		var values = new List<int>();
		if (i0 == i1)
		{
			values.Add((int)d0);
			return values;
		}

		var a = (d1 - d0) / (i1 - i0);
		var d = d0;
		for (var i = i0; i <= i1; i++)
		{
			values.Add((int)d);
			d += a;
		}

		return values;
	}

	private void DrawLine(Point p0, Point p1, Color color)
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
				PutPixel(x, ys[(x - p0.X) | 0], color);
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
				PutPixel(xs[(y - p0.Y) | 0], y, color);
			}
		}
	}

	#endregion Rasterization

	public void Update(GameTime _)
	{
		var texture = Textures[TextureIndex];
		Array.Copy(ClearData, TextureData, CanvasW * CanvasH);

		DrawLine(new Point(-200, -100), new Point(240, 120), Color.White);
		DrawLine(new Point(-50, -200), new Point(60, 240), Color.White);

		texture.SetData(TextureData);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(Textures[TextureIndex], new Vector2(0, 0), null, Color.White);
		TextureIndex = (TextureIndex + 1) % NumTextures;
	}
}
