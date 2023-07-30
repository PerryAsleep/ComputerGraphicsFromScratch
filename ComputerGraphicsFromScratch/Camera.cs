using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using static ComputerGraphicsFromScratch.Utils;

namespace ComputerGraphicsFromScratch;

internal class Camera
{
	public Vector3 Position;
	private float _yaw;
	private float _pitch;
	private float _roll;
	private float _hFov = 90;
	private float _focalLength;

	private int ViewportW;
	private int ViewportH;
	private List<Plane> ClippingPlanes;

	public Camera(Vector3 position, int viewportW, int viewportH)
	{
		Position = position;
		UpdateViewport(viewportW, viewportH);
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

	public float Fov
	{
		get => _hFov;
		set
		{
			_hFov = value;
			UpdateFocalLength();
		}
	}

	public void UpdateViewport(int viewportW, int viewportH)
	{
		ViewportW = viewportW;
		ViewportH = viewportH;
		UpdateFocalLength();
	}

	private void UpdateFocalLength()
	{
		var projectionW = (float)ViewportW / ViewportH;
		_focalLength = (float)(projectionW / (2.0f * Math.Tan(0.5f * DegreesToRadians(_hFov))));
		UpdateClippingPlanes();
	}

	private void UpdateClippingPlanes()
	{
		var projectionW = (float)ViewportW / ViewportH;

		var hNormal = new Vector2(_focalLength, 0.5f * projectionW);
		hNormal.Normalize();

		var vNormal = new Vector2(_focalLength, 0.5f);
		vNormal.Normalize();

		ClippingPlanes = new List<Plane>
		{
			new(new Vector3(0.0f, 0.0f, 1.0f), -1.0f), // Near
			new(new Vector3(hNormal.X, 0.0f, hNormal.Y), 0.0f), // Left
			new(new Vector3(-hNormal.X, 0.0f, hNormal.Y), 0.0f), // Right
			new(new Vector3(0.0f, -vNormal.X, vNormal.Y), 0.0f), // Top
			new(new Vector3(0.0f, vNormal.X, vNormal.Y), 0.0f), // Bottom
		};
	}

	public float GetProjectionPlaneW()
	{
		return (float)ViewportW / ViewportH;
	}

	public float GetProjectionPlaneH()
	{
		return 1.0f;
	}

	public IReadOnlyList<Plane> GetClippingPlanes()
	{
		return ClippingPlanes;
	}

	public float GetProjectionPlaneDistance()
	{
		return _focalLength;
	}

	private void UpdateOrientation()
	{
		Orientation = Matrix.CreateFromYawPitchRoll(DegreesToRadians(Yaw), DegreesToRadians(Pitch), DegreesToRadians(Roll));
	}

	public Matrix GetViewMatrix()
	{
		return Matrix.Transpose(Orientation) * Matrix.CreateTranslation(Position * -1.0f);
	}
}
