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
    private static readonly List<Color4> _colors = new();

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

    private static float _red, _blue, _green;
    public static Color4 NextColor()
    {
      _red += render_system.THRESHOLD + 0.00001f;
      _blue += render_system.THRESHOLD + 0.00001f;
      _green += render_system.THRESHOLD + 0.00001f;
      _red %= 1f;
      _blue %= 1f;
      _green %= 1f;
      return new Color4(_red, _green, _blue, 1f);
    }
  }

  public static class rand
  {
    public static float NextFloat() => Random.Shared.NextSingle();
    public static float NextFloat(float min, float max) => Random.Shared.NextSingle() * (max - min) + min;
    public static int Next(int min, int max) => Random.Shared.Next(min, max);
    public static int Next(int max) => Random.Shared.Next(max);
    public static int Next() => Random.Shared.Next();
    public static long NextInt64() => Random.Shared.NextInt64();
  }

  public static class extensions
  {
    public static string ContentToString<T>(this Span<T> arr)
    {
      string o = "[";
      foreach (T item in arr) o += item + ", ";
      if (o.Length != 1)
        o = o[..^2];
      o += "]";
      return o;
    }
    
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

    public static void Transform(this ref Vector4 vec, Matrix4 m4F)
    {
      float f = vec.X;
      float g = vec.Y;
      float h = vec.Z;
      float i = vec.W;
      vec.X = MathF.FusedMultiplyAdd(m4F.M11, f,
        MathF.FusedMultiplyAdd(m4F.M21, g, MathF.FusedMultiplyAdd(m4F.M31, h, m4F.M41 + i)));
      vec.Y = MathF.FusedMultiplyAdd(m4F.M12, f,
        MathF.FusedMultiplyAdd(m4F.M22, g, MathF.FusedMultiplyAdd(m4F.M32, h, m4F.M42 + i)));
      vec.Z = MathF.FusedMultiplyAdd(m4F.M13, f,
        MathF.FusedMultiplyAdd(m4F.M23, g, MathF.FusedMultiplyAdd(m4F.M33, h, m4F.M43 + i)));
      vec.W = MathF.FusedMultiplyAdd(m4F.M14, f,
        MathF.FusedMultiplyAdd(m4F.M24, g, MathF.FusedMultiplyAdd(m4F.M34, h, m4F.M44 + i)));
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

    public static void Scale(this ref Matrix4 matrix4, Vector3 scalar)
    {
      matrix4 *= Matrix4.CreateScale(scalar);
    }

    public static void Scale(this ref Matrix4 matrix4, float x, float y, float z)
    {
      matrix4 *= Matrix4.CreateScale(x, y, z);
    }

    public static void Translate(this ref Matrix4 matrix4, Vector3 translation)
    {
      matrix4 *= Matrix4.CreateTranslation(translation);
    }

    public static void Translate(this ref Matrix4 matrix4, float x, float y, float z)
    {
      matrix4 *= Matrix4.CreateTranslation(x, y, z);
    }

    public static void Rotate(this ref Matrix4 matrix4, float angle, Vector3 axis)
    {
      matrix4 *= Matrix4.CreateFromAxisAngle(axis, angle / 180f * MathF.PI);
    }

    public static void Rotate(this ref Matrix4 matrix4, float angle, float x, float y, float z)
    {
      matrix4 *= Matrix4.CreateFromAxisAngle(new Vector3(x, y, z), angle / 180f * MathF.PI);
    }
    
    static class array_accessor<T>
    {
      public static Func<List<T>, T[]> Getter;

      static array_accessor()
      {
        DynamicMethod dm = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T[]), new[] { typeof(List<T>) }, typeof(array_accessor<T>), true);
        ILGenerator il = dm.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
        il.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)); // Replace argument by field
        il.Emit(OpCodes.Ret); // Return field
        Getter = (Func<List<T>, T[]>)dm.CreateDelegate(typeof(Func<List<T>, T[]>));
      }
    }

    public static T[] GetInternalArray<T>(this List<T> list)
    {
      return array_accessor<T>.Getter(list);
    }

    public static void Set(this ref Matrix4 mat, Matrix4 other)
    {
      mat.M11 = other.M11;
      mat.M12 = other.M12;
      mat.M13 = other.M13;
      mat.M14 = other.M14;
      mat.M21 = other.M21;
      mat.M22 = other.M22;
      mat.M23 = other.M23;
      mat.M24 = other.M24;
      mat.M31 = other.M31;
      mat.M32 = other.M32;
      mat.M33 = other.M33;
      mat.M34 = other.M34;
      mat.M41 = other.M41;
      mat.M42 = other.M42;
      mat.M43 = other.M43;
      mat.M44 = other.M44;
    }

    public static Vector2i ToChunkPos(this Vector2 vec)
    {
      return ((int)vec.X >> 4, (int)vec.Y >> 4);
    }
  }
}