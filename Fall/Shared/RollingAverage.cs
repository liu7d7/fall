namespace Fall.Shared
{
  public class rolling_avg
  {
    private readonly int _size;
    public readonly Queue<double> Values = new();
    private double _sum;

    public rolling_avg(int size)
    {
      _size = size;
    }

    public double Average => _sum / Values.Count;

    public void Add(double value)
    {
      _sum += value;
      Values.Enqueue(value);
      if (Values.Count <= _size) return;
      _sum -= Values.Dequeue();
    }
  }
}