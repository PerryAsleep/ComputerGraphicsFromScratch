using Microsoft.Xna.Framework;

namespace ComputerGraphicsFromScratch;

internal class Instance
{
	private readonly Model Model;
	public Vector3 Position;
	public Matrix Orientation;
	public float Scale;

	private Matrix Transform;

	public Instance(Model model)
	{
		Model = model;
		Position = Vector3.Zero;
		Orientation = Matrix.Identity;
		Scale = 1.0f;
		UpdateTransform();
	}

	public Instance(Model model, Vector3 position)
	{
		Model = model;
		Position = position;
		Orientation = Matrix.Identity;
		Scale = 1.0f;
		UpdateTransform();
	}

	public Instance(Model model, Vector3 position, Matrix orientation, float scale)
	{
		Model = model;
		Position = position;
		Orientation = orientation;
		Scale = scale;
		UpdateTransform();
	}

	private void UpdateTransform()
	{
		Transform = (Matrix.CreateScale(Scale) * Orientation) * Matrix.CreateTranslation(Position);
	}

	public Model GetModel()
	{
		return Model;
	}

	public Matrix GetTransform()
	{
		return Transform;
	}
}
