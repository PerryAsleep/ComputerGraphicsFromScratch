using Microsoft.Xna.Framework;

namespace ComputerGraphicsFromScratch;

internal sealed class Sphere
{
	public Vector3 Center { get; set; }
	public double Radius { get; set; }
	public Color Color { get; set; }
	public int Specular { get; set; }

	public Sphere(Vector3 center, double radius, Color color, int specular)
	{
		Center = center;
		Radius = radius;
		Color = color;
		Specular = specular;
	}
}
