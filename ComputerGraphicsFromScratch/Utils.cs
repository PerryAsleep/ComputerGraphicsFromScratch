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
}
