namespace ComputerGraphicsFromScratch;

internal readonly struct Point<T>
{
	public readonly T X;
	public readonly T Y;
	public readonly float H;

	public Point(T x, T y)
	{
		X = x;
		Y = y;
		H = 1.0f;
	}

	public Point(T x, T y, float h)
	{
		X = x;
		Y = y;
		H = h;
	}
}
