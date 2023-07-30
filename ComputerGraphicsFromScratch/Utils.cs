using System;
using System.Collections.Generic;

namespace ComputerGraphicsFromScratch;

internal sealed class Utils
{
	public static float DegreesToRadians(float degrees)
	{
		return degrees * 0.01745329251994329576923690768489f;
	}

	public static float RadiansToDegrees(float radians)
	{
		return radians * 57.295779513082320876798154814105f;
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		(a, b) = (b, a);
	}

	public void Shuffle<T>(List<T> values)
	{
		var r = new Random();
		for (var i = values.Count - 1; i > 0; --i)
		{
			var rand = (int)(r.NextDouble() * (i + 1));
			(values[i], values[rand]) = (values[rand], values[i]);
		}
	}
}
