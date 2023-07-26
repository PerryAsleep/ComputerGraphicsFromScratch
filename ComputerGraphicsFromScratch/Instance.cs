using Microsoft.Xna.Framework;

namespace ComputerGraphicsFromScratch;

internal class Instance
{
	private readonly Model Model;
	public Vector3 Position;

	public Instance(Model model)
	{
		Model = model;
	}

	public Instance(Model model, Vector3 position)
	{
		Model = model;
		Position = position;
	}

	public Model GetModel()
	{
		return Model;
	}
}
