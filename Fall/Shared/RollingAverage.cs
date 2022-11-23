namespace Fall.Shared
{
  public class rolling_avg
  {
    private readonly int _size;
    private readonly Queue<double> _values = new();
    private double _sum;

    public rolling_avg(int size)
    {
      _size = size;
    }

    public double Average => _sum / _values.Count;

    public void Add(double value)
    {
      _sum += value;
      _values.Enqueue(value);
      if (_values.Count > _size) _sum -= _values.Dequeue();
    }
  }
}