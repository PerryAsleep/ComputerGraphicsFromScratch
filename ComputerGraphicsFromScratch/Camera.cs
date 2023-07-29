using Microsoft.Xna.Framework;
using static ComputerGraphicsFromScratch.Utils;

namespace ComputerGraphicsFromScratch;

internal class Camera
{
	public Vector3 Position;
	private float _yaw;
	private float _pitch;
	private float _roll;

	public Camera(Vector3 position)
	{
		Position = position;
		UpdateOrientation();
	}

	public Matrix Orientation { get; private set; }

	public float Yaw
	{
		get => _yaw;
		set
		{
			_yaw = value;
			UpdateOrientation();
		}
	}

	public float Pitch
	{
		get => _pitch;
		set
		{
			_pitch = value;
			UpdateOrientation();
		}
	}

	public float Roll
	{
		get => _roll;
		set
		{
			_roll = value;
			UpdateOrientation();
		}
	}

	private void UpdateOrientation()
	{
		Orientation = Matrix.CreateFromYawPitchRoll(DegreesToRadians(Yaw), DegreesToRadians(Pitch), DegreesToRadians(Roll));
	}
}
