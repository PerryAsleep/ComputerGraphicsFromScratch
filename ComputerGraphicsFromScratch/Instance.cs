using Microsoft.Xna.Framework;
using static ComputerGraphicsFromScratch.Utils;

namespace ComputerGraphicsFromScratch;

internal class Instance
{
	private readonly Model Model;

	private Vector3 _position;
	private float _scale;
	private float _yaw;
	private float _pitch;
	private float _roll;

	private int DebugNumRenderedTriangles;

	private Matrix Transform;

	public Instance(Model model)
	{
		Model = model;
		Position = Vector3.Zero;
		Scale = 1.0f;
		UpdateTransform();
	}

	public Instance(Model model, Vector3 position)
	{
		Model = model;
		Position = position;
		Scale = 1.0f;
		UpdateTransform();
	}

	public Instance(Model model, Vector3 position, float scale)
	{
		Model = model;
		Position = position;
		Scale = scale;
		UpdateTransform();
	}

	public Vector3 Position
	{
		get => _position;
		set
		{
			_position = value;
			UpdateTransform();
		}
	}

	public float Scale
	{
		get => _scale;
		set
		{
			_scale = value;
			UpdateTransform();
		}
	}

	public float Yaw
	{
		get => _yaw;
		set
		{
			_yaw = value;
			UpdateTransform();
		}
	}

	public float Pitch
	{
		get => _pitch;
		set
		{
			_pitch = value;
			UpdateTransform();
		}
	}

	public float Roll
	{
		get => _roll;
		set
		{
			_roll = value;
			UpdateTransform();
		}
	}

	private void UpdateTransform()
	{
		var scale = Matrix.CreateScale(Scale);
		var orientation = Matrix.CreateFromYawPitchRoll(DegreesToRadians(Yaw), DegreesToRadians(Pitch), DegreesToRadians(Roll));
		var translation = Matrix.CreateTranslation(Position);
		Transform = scale * orientation * translation;
	}

	public Model GetModel()
	{
		return Model;
	}

	public Matrix GetTransform()
	{
		return Transform;
	}

	public void DebugSetNumRenderedTriangles(int numRenderedTriangles)
	{
		DebugNumRenderedTriangles = numRenderedTriangles;
	}

	public int DebugGetNumRenderedTriangles()
	{
		return DebugNumRenderedTriangles;
	}
}
