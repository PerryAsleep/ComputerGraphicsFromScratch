using System;
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

	public void Update(GameTime _)
	{
		var texture = Textures[TextureIndex];
		Array.Copy(ClearData, TextureData, CanvasW * CanvasH);

		DrawLineBroken(-200, -100, 240, 120, Color.White);
		DrawLineBroken(-50, -200, 60, 240, Color.White);

		texture.SetData(TextureData);
	}

	private void DrawLineBroken(int x0, int y0, int x1, int y1, Color color)
	{
		var a = (y1 - y0) / (x1 - x0);
		var y = y0;
		for (var x = x0; x <= x1; x++)
		{
			PutPixel(x, y, color);
			y += a;
		}
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
