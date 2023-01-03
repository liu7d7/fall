using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared
{
  public static class maps
  {
    public static Dictionary<Tk, Tv> Create<Tk, Tv>(params KeyValuePair<Tk, Tv>[] pairs)
    {
      return pairs.ToDictionary(pair => pair.Key, pair => pair.Value);
    }
  }

  public static class deterministic_random
  {
    private static readonly float[] _rand = new float[1000];

    static deterministic_random()
    {
      for (int i = 0; i < 1000; i++) _rand[i] = rand.NextFloat();
    }

    public static float NextFloat(object val)
    {
      return _rand[Math.Abs(val.GetHashCode()) % 1000];
    }

    public static int NextInt(object val, int max)
    {
      return (int)(_rand[Math.Abs(val.GetHashCode()) % 1000] * max);
    }
  }

  public static class colors
  {
    private static bool _initialized;
    private static readonly Dictionary<string, Color4> _values = new();
    private static readonly list<Color4> _colors = new();

    private static float _red, _blue, _green;

    public static Color4 GetColor(string color)
    {
      if (_initialized) return _values[color.ToLower()];
      foreach (PropertyInfo john in typeof(Color4).GetProperties())
      {
        object val = john.GetValue(null);

        if (val is not Color4 color4) continue;
        _values.Add(john.Name.ToLower(), color4);
        _colors.Add(color4);
      }

      _initialized = true;
      return _values[color.ToLower()];
    }

    public static Color4 GetRandomColor(object val = null)
    {
      val ??= rand.NextInt64();
      if (!_initialized) GetColor("white");
      return _colors[deterministic_random.NextInt(val, _colors.Count)];
    }

    public static Color4 NextColor()
    {
      _red += glh.THRESHOLD + 0.00001f;
      _blue += glh.THRESHOLD + 0.00001f;
      _green += glh.THRESHOLD + 0.00001f;
      _red %= 1f;
      _blue %= 1f;
      _green %= 1f;
      return new Color4(_red, _green, _blue, 1f);
    }
  }

  public static class rand
  {
    public static float NextFloat()
    {
      return Random.Shared.NextSingle();
    }

    public static float NextFloat(float min, float max)
    {
      return Random.Shared.NextSingle() * (max - min) + min;
    }

    public static int Next(int min, int max)
    {
      return Random.Shared.Next(min, max);
    }

    public static int Next(int max)
    {
      return Random.Shared.Next(max);
    }

    public static int Next()
    {
      return Random.Shared.Next();
    }

    public static long NextInt64()
    {
      return Random.Shared.NextInt64();
    }
  }

  public static class extensions
  {
    public static string ContentToString<T>(this IEnumerable<T> arr)
    {
      string o = "[";
      foreach (T item in arr) o += item + ", ";
      if (o.Length != 1)
        o = o[..^2];
      o += "]";
      return o;
    }

    public static string ContentToString<T, Tv>(this Dictionary<T, Tv> arr)
    {
      StringBuilder o = new("Map<");
      o.Append(typeof(T));
      o.Append(", ");
      o.Append(typeof(Tv));
      o.Append(">(");
      foreach (KeyValuePair<T, Tv> item in arr)
      {
        o.Append('(');
        o.Append(item.Key);
        o.Append(", ");
        o.Append(item.Value);
        o.Append("), ");
      }

      if (arr.Count > 0) o.Remove(o.Length - 3, 3);
      o.Append(')');
      
      return o.ToString();
    }

    public static Task ForEachAsync<T>(list<T> list, int batches, Func<T, Task> action)
    {
      async Task partition(IEnumerator<T> part)
      {
        using (part)
        {
          while (part.MoveNext())
          {
            await action(part.Current);
          }
        }
      }

      return Task.WhenAll(Partitioner.Create(list).GetPartitions(batches).AsParallel().Select(partition));
    }
    
    public static Task ForAsync(Range list, int batches, Func<int, Task> action)
    {
      async Task partition(IEnumerator<int> part)
      {
        using (part)
        {
          while (part.MoveNext())
          {
            await action(part.Current);
          }
        }
      }

      return Task.WhenAll(Partitioner.Create(Enumerable.Range(list.Start.Value, list.End.Value)).GetPartitions(batches).AsParallel().Select(partition));
    }

    public static float Rad(this float degrees)
    {
      return (float)(degrees * Math.PI / 180.0);
    }

    public static float Deg(this float radians)
    {
      return (float)(radians * 180 / Math.PI);
    }

    public static void Scale(this ref Matrix4 matrix4, float scalar)
    {
      matrix4 *= Matrix4.CreateScale(scalar);
    }

    public static void Translate(this ref Matrix4 matrix4, Vector3 translation)
    {
      matrix4 *= Matrix4.CreateTranslation(translation);
    }

    public static void Rotate(this ref Matrix4 matrix4, float angle, float x, float y, float z)
    {
      matrix4 *= Matrix4.CreateFromAxisAngle(new Vector3(x, y, z), angle / 180f * MathF.PI);
    }

    public static Vector2i ToChunkPos(this Vector2 vec)
    {
      return ((int)vec.X >> 4, (int)vec.Y >> 4);
    }
  }
}