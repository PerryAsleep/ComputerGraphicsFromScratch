using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;

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

	public static int Clamp(int value, int min, int max)
	{
		return Math.Min(Math.Max(min, value), max);
	}

	#region Color

	public static Color AddColor(Color color1, Color color2)
	{
		return new Color(
			(byte)Clamp(color1.R + color2.R, byte.MinValue, byte.MaxValue),
			(byte)Clamp(color1.G + color2.G, byte.MinValue, byte.MaxValue),
			(byte)Clamp(color1.B + color2.B, byte.MinValue, byte.MaxValue));
	}

	public static Color MultiplyColor(Color color, float factor)
	{
		return new Color(
			(byte)Clamp((int)(color.R * factor), byte.MinValue, byte.MaxValue),
			(byte)Clamp((int)(color.G * factor), byte.MinValue, byte.MaxValue),
			(byte)Clamp((int)(color.B * factor), byte.MinValue, byte.MaxValue));
	}

	#endregion Color

	#region Model Generation

	public static Model CreateCube()
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
			new(0, 1, 2, Color.Red, new List<Vector3> { new(0, 0, 1), new(0, 0, 1), new(0, 0, 1) }),
			new(0, 2, 3, Color.Red, new List<Vector3> { new(0, 0, 1), new(0, 0, 1), new(0, 0, 1) }),
			new(4, 0, 3, Color.Green, new List<Vector3> { new(1, 0, 0), new(1, 0, 0), new(1, 0, 0) }),
			new(4, 3, 7, Color.Green, new List<Vector3> { new(1, 0, 0), new(1, 0, 0), new(1, 0, 0) }),
			new(5, 4, 7, Color.Blue, new List<Vector3> { new(0, 0, -1), new(0, 0, -1), new(0, 0, -1) }),
			new(5, 7, 6, Color.Blue, new List<Vector3> { new(0, 0, -1), new(0, 0, -1), new(0, 0, -1) }),
			new(1, 5, 6, Color.Yellow, new List<Vector3> { new(-1, 0, 0), new(-1, 0, 0), new(-1, 0, 0) }),
			new(1, 6, 2, Color.Yellow, new List<Vector3> { new(-1, 0, 0), new(-1, 0, 0), new(-1, 0, 0) }),
			new(4, 5, 1, Color.Purple, new List<Vector3> { new(0, 1, 0), new(0, 1, 0), new(0, 1, 0) }),
			new(4, 1, 0, Color.Purple, new List<Vector3> { new(0, 1, 0), new(0, 1, 0), new(0, 1, 0) }),
			new(2, 6, 7, Color.Cyan, new List<Vector3> { new(0, -1, 0), new(0, -1, 0), new(0, -1, 0) }),
			new(2, 7, 3, Color.Cyan, new List<Vector3> { new(0, -1, 0), new(0, -1, 0), new(0, -1, 0) }),
		};

		return new Model(vertices, triangles, new Vector3(0, 0, 0), (float)Math.Sqrt(3), "Cube");
	}

	public static Model CreateSphere(int divisions, Color color)
	{
		var vertices = new List<Vector3>();
		var triangles = new List<Triangle>();
		var deltaAngle = 2.0f * Math.PI / divisions;

		// Generate vertices and normals.
		for (var d = 0; d < divisions + 1; d++)
		{
			var y = 2.0f / divisions * (d - divisions * 0.5f);
			var radius = (float)Math.Sqrt(1.0f - y * y);
			for (var i = 0; i < divisions; i++)
			{
				var vertex = new Vector3((float)(radius * Math.Cos(i * deltaAngle)), y,
					(float)(radius * Math.Sin(i * deltaAngle)));
				vertices.Add(vertex);
			}
		}

		// Generate triangles.
		for (var d = 0; d < divisions; d++)
		{
			for (var i = 0; i < divisions; i++)
			{
				var i0 = d * divisions + i;
				var i1 = (d + 1) * divisions + (i + 1) % divisions;
				var i2 = divisions * d + (i + 1) % divisions;
				var tri0 = new List<int> { i0, i1, i2 };
				var tri1 = new List<int> { i0, i0 + divisions, i1 };
				triangles.Add(new Triangle(i0, i1, i2, color,
					new List<Vector3> { vertices[tri0[0]], vertices[tri0[1]], vertices[tri0[2]] }));
				triangles.Add(new Triangle(i0, i0 + divisions, i1, color,
					new List<Vector3> { vertices[tri1[0]], vertices[tri1[1]], vertices[tri1[2]] }));
			}
		}

		return new Model(vertices, triangles, Vector3.Zero, 1.0f, "Sphere");
	}

	#endregion Model Generation

	#region ImGui

	private static readonly Dictionary<Type, string[]> EnumStringsCacheByType = new();

	/// <summary>
	/// Formats an enum string value for by returning a string value
	/// with space-separated capitalized words.
	/// </summary>
	/// <param name="enumValue">String representation of enum value.</param>
	/// <returns>Formatting string representation of enum value.</returns>
	private static string FormatEnumForUI(string enumValue)
	{
		var sb = new StringBuilder(enumValue.Length * 2);
		var capitalizeNext = true;
		var previousWasCapital = false;
		var first = true;
		foreach (var character in enumValue)
		{
			// Treat dashes as spaces. Capitalize the letter after a space.
			if (character == '_' || character == '-')
			{
				sb.Append(' ');
				capitalizeNext = true;
				first = false;
				previousWasCapital = false;
				continue;
			}

			// Lowercase character. Use this character unless we are supposed to
			// capitalize it due to it following a space.
			if (char.IsLower(character))
			{
				if (capitalizeNext)
				{
					sb.Append(char.ToUpper(character));
					previousWasCapital = true;
				}
				else
				{
					sb.Append(character);
					previousWasCapital = false;
				}
			}

			// Uppercase character. Prepend a space, unless this followed another
			// capitalized character, in which case lowercase it. This is to support
			// formatting strings like "YES" to "Yes".
			else if (char.IsUpper(character))
			{
				if (!first && !previousWasCapital)
					sb.Append(' ');
				if (previousWasCapital)
					sb.Append(char.ToLower(character));
				else
					sb.Append(character);
				previousWasCapital = true;
			}

			// For any other character type, just record it as is.
			else
			{
				sb.Append(character);
				previousWasCapital = false;
			}

			first = false;
			capitalizeNext = false;
		}

		return sb.ToString();
	}

	private static string[] GetCachedEnumStrings<T>()
	{
		var typeOfT = typeof(T);
		if (EnumStringsCacheByType.TryGetValue(typeOfT, out var strings))
			return strings;

		var enumValues = Enum.GetValues(typeOfT);
		var numEnumValues = enumValues.Length;
		var enumStrings = new string[numEnumValues];
		for (var i = 0; i < numEnumValues; i++)
			enumStrings[i] = FormatEnumForUI(enumValues.GetValue(i)!.ToString());
		EnumStringsCacheByType[typeOfT] = enumStrings;
		return EnumStringsCacheByType[typeOfT];
	}

	/// <summary>
	/// Draws an ImGui Combo element for the values of of the enum of type T.
	/// </summary>
	/// <typeparam name="T">Enum type of values in the Combo element.</typeparam>
	/// <param name="name">Name of the element for ImGui.</param>
	/// <param name="enumValue">The current value.</param>
	/// <returns>Whether the Combo value has changed.</returns>
	public static bool ComboFromEnum<T>(string name, ref T enumValue) where T : Enum
	{
		var strings = GetCachedEnumStrings<T>();
		var intValue = (int)(object)enumValue;
		var result = ImGui.Combo(name, ref intValue, strings, strings.Length);
		enumValue = (T)(object)intValue;
		return result;
	}

	#endregion ImGui
}
