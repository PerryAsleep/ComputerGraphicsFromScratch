using System.Diagnostics;
using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;
using System.Collections.Generic;

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
	private readonly List<Instance> Instances;

	public Game1()
	{
		Graphics = new GraphicsDeviceManager(this);
		Graphics.GraphicsProfile = GraphicsProfile.HiDef;

		Window.AllowUserResizing = true;
		Window.ClientSizeChanged += OnResize;
		Graphics.SynchronizeWithVerticalRetrace = true;

		Graphics.PreferredBackBufferHeight = H;
		Graphics.PreferredBackBufferWidth = W;
		Graphics.IsFullScreen = false;
		Graphics.ApplyChanges();

		Camera = new Camera(new Vector3(-3, 1, -2), GetViewportWidth(), GetViewportHeight());

		Content.RootDirectory = "Content";
		IsMouseVisible = true;

		ImGuiRenderer = new ImGuiRenderer(this);
		ImGuiRenderer.RebuildFontAtlas();

		// Create cube instances.
		var cube = CreateCube();
		Instances = new List<Instance>
		{
			new(cube, new Vector3(-1.5f, 0.0f, 7.0f), 0.75f),
			new(cube, new Vector3(1.25f, 2.5f, 7.5f), 1.0f),
		};

		//RayTracer = new RayTracer(GraphicsDevice, W, H);
		Rasterizer = new Rasterizer(GraphicsDevice, W, H, Camera, Instances);
	}

	private static Model CreateCube()
	{
		var vertices = new List<Vector3>
		{
			new(1, 1, 1),
			new(-1, 1, 1),
			new(-1, -1, 1),
			new(1, -1, 1),
			new(1, 1, -1),
			new(-1, 1, -1),
			new(-1, -1, -1),
			new(1, -1, -1),
		};

		var triangles = new List<Triangle>
		{
			new(0, 1, 2, Color.Red),
			new(0, 2, 3, Color.Red),
			new(4, 0, 3, Color.Green),
			new(4, 3, 7, Color.Green),
			new(5, 4, 7, Color.Blue),
			new(5, 7, 6, Color.Blue),
			new(1, 5, 6, Color.Yellow),
			new(1, 6, 2, Color.Yellow),
			new(4, 5, 1, Color.Purple),
			new(4, 1, 0, Color.Purple),
			new(2, 6, 7, Color.Cyan),
			new(2, 7, 3, Color.Cyan),
		};

		return new Model(vertices, triangles, new Vector3(0, 0, 0), (float)Math.Sqrt(3));
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

			var fov = Camera.Fov;
			if (ImGui.DragFloat("FOV", ref fov, 0.1f))
				Camera.Fov = fov;

			var yaw = Camera.Yaw;
			if (ImGui.DragFloat("Yaw", ref yaw, 0.1f, 0.0f, 360.0f))
				Camera.Yaw = yaw;

			var pitch = Camera.Pitch;
			if (ImGui.DragFloat("Pitch", ref pitch, 0.1f, -90.0f, 90.0f))
				Camera.Pitch = pitch;

			var roll = Camera.Roll;
			if (ImGui.DragFloat("Roll", ref roll, 0.1f, -90.0f, 90.0f))
				Camera.Roll = roll;

			var index = 0;
			foreach (var instance in Instances)
			{
				ImGui.Separator();
				ImGui.Text($"Cube {index + 1}");
				ImGui.Text($"Num Triangles Rendered: {instance.DebugGetNumRenderedTriangles()}");

				var instancePos = new System.Numerics.Vector3(instance.Position.X, instance.Position.Y, instance.Position.Z);
				if (ImGui.DragFloat3($"Position##{index}", ref instancePos, 0.1f))
					instance.Position = new Vector3(instancePos.X, instancePos.Y, instancePos.Z);

				var instanceScale = instance.Scale;
				if (ImGui.DragFloat($"Scale##{index}", ref instanceScale, 0.01f, 0.01f, 10.0f))
					instance.Scale = instanceScale;

				var instanceYaw = instance.Yaw;
				if (ImGui.DragFloat($"Yaw##{index}", ref instanceYaw, 0.1f, 0.0f, 360.0f))
					instance.Yaw = instanceYaw;

				var instancePitch = instance.Pitch;
				if (ImGui.DragFloat($"Pitch##{index}", ref instancePitch, 0.1f, -90.0f, 90.0f))
					instance.Pitch = instancePitch;

				var instanceRoll = instance.Roll;
				if (ImGui.DragFloat($"Roll##{index}", ref instanceRoll, 0.1f, -90.0f, 90.0f))
					instance.Roll = instanceRoll;

				ImGui.Checkbox($"Draw Triangle Outlines##{index}", ref instance.DebugDrawOutlines);

				if (ImGui.Button($"Reset##{index}"))
				{
					instance.Position = Vector3.Zero;
					instance.Scale = 1.0f;
					instance.Yaw = 0.0f;
					instance.Pitch = 0.0f;
					instance.Roll = 0.0f;
				}

				index++;
			}

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

	#region Window Resizing

	public void OnResize(object sender, EventArgs e)
	{
		var form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(Window.Handle);
		if (form == null)
			return;
		var w = GetViewportWidth();
		var h = GetViewportHeight();
		Camera.UpdateViewport(w, h);
		Rasterizer.UpdateViewport(w, h);
	}

	public int GetViewportWidth()
	{
		return Graphics.GraphicsDevice.Viewport.Width;
	}

	public int GetViewportHeight()
	{
		return Graphics.GraphicsDevice.Viewport.Height;
	}

	#endregion Window Resizing
}
