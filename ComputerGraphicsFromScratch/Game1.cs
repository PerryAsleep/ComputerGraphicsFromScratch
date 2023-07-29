using System.Diagnostics;
using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;

namespace ComputerGraphicsFromScratch;

internal sealed class Game1 : Game
{
	private const int W = 1920;
	private const int H = 1080;

	private GraphicsDeviceManager Graphics;
	private SpriteBatch SpriteBatch;
	//private readonly RayTracer RayTracer;
	private readonly Rasterizer Rasterizer;

	private readonly ImGuiRenderer ImGuiRenderer;
	private readonly ApplicationMouseState MouseState = new();

	private readonly Camera Camera;

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

		Camera = new Camera(new Vector3(-3, 1, 2));

		//RayTracer = new RayTracer(GraphicsDevice, W, H);
		Rasterizer = new Rasterizer(GraphicsDevice, W, H, Camera);

		Content.RootDirectory = "Content";
		IsMouseVisible = true;

		ImGuiRenderer = new ImGuiRenderer(this);
		ImGuiRenderer.RebuildFontAtlas();
	}

	protected override void LoadContent()
	{
		SpriteBatch = new SpriteBatch(GraphicsDevice);
	}

	protected override void Update(GameTime gameTime)
	{
		if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			Exit();

		var inFocus = IsApplicationFocused();

		ImGuiRenderer.UpdateInput(gameTime);
		ImGuiRenderer.BeforeLayout();

		// Process Mouse Input.
		var state = Mouse.GetState();
		MouseState.Update(state, inFocus);

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

		DrawGui();

		ImGuiRenderer.AfterLayout();

		base.Draw(gameTime);
	}

	private void DrawGui()
	{
		if (ImGui.Begin("Controls"))
		{
			ImGui.Text("Camera");

			var pos = new System.Numerics.Vector3(Camera.Position.X, Camera.Position.Y, Camera.Position.Z);
			if (ImGui.DragFloat3("Position", ref pos, 0.1f))
				Camera.Position = new Vector3(pos.X, pos.Y, pos.Z);

			var yaw = Camera.Yaw;
			if (ImGui.DragFloat("Yaw", ref yaw, 0.1f, 0.0f, 360.0f))
				Camera.Yaw = yaw;

			var pitch = Camera.Pitch;
			if (ImGui.DragFloat("Pitch", ref pitch, 0.1f, -90.0f, 90.0f))
				Camera.Pitch = pitch;

			var roll = Camera.Roll;
			if (ImGui.DragFloat("Roll", ref roll, 0.1f, -90.0f, 90.0f))
				Camera.Roll = roll;

			ImGui.End();
		}
	}


	#region Application Focus

	public static bool IsApplicationFocused()
	{
		var activatedHandle = GetForegroundWindow();
		if (activatedHandle == IntPtr.Zero)
			return false;

		GetWindowThreadProcessId(activatedHandle, out var activeProcId);
		return activeProcId == Process.GetCurrentProcess().Id;
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

	#endregion Application Focus
}
