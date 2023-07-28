using Microsoft.Xna.Framework;

namespace ComputerGraphicsFromScratch;

internal class Camera
{
	public Vector3 Position;
	public Matrix Orientation;

	public Camera(Vector3 position, Matrix orientation)
	{
		Position = position;
		Orientation = orientation;
	}
}
