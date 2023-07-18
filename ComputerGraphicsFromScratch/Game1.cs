using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ComputerGraphicsFromScratch;

internal sealed class Game1 : Game
{
	private const int W = 640;
	private const int H = 480;

	private GraphicsDeviceManager Graphics;
	private SpriteBatch SpriteBatch;
	private readonly RayTracer RayTracer;
	private readonly Rasterizer Rasterizer;

	public Game1()
	{
		Graphics = new GraphicsDeviceManager(this);
		Graphics.GraphicsProfile = GraphicsProfile.HiDef;

		Window.AllowUserResizing = false;
		Graphics.SynchronizeWithVerticalRetrace = true;

		Graphics.PreferredBackBufferHeight = H;
		Graphics.PreferredBackBufferWidth = W;
		Graphics.IsFullScreen = false;
		Graphics.ApplyChanges();

		RayTracer = new RayTracer(GraphicsDevice, W, H);
		Rasterizer = new Rasterizer(GraphicsDevice, W, H);

		Content.RootDirectory = "Content";
		IsMouseVisible = true;
	}

	protected override void LoadContent()
	{
		SpriteBatch = new SpriteBatch(GraphicsDevice);
	}

	protected override void Update(GameTime gameTime)
	{
		if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			Exit();

		//RayTracer.Update(gameTime);
		Rasterizer.Update(gameTime);

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);

		SpriteBatch.Begin();
		//RayTracer.Draw(SpriteBatch);
		Rasterizer.Draw(SpriteBatch);
		SpriteBatch.End();

		base.Draw(gameTime);
	}
}
