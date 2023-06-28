using Microsoft.Xna.Framework;

namespace ComputerGraphicsFromScratch;

internal sealed class Light
{
	public enum LightType
	{
		Ambient,
		Point,
		Directional,
	};

	public LightType Type { get; set; }
	public float Intensity { get; set; }
	public Vector3 Position { get; set; }

	public Light(LightType type, float intensity, Vector3 position)
	{
		Type = type;
		Intensity = intensity;
		Position = position;
	}
}
