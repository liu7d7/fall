using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class world
  {
    private readonly Dictionary<Vector2i, chunk> _chunks;
    public readonly List<fall_obj> Objs = new();

    private Vector2i _prevCPos;

    public world()
    {
      _chunks = new Dictionary<Vector2i, chunk>();
    }

    public void Update()
    {
      int k = 0;
      for (int i = 0; i < Objs.Count; i++)
      {
        if (!Objs[i].Updates || (Objs[i].Pos - fall.Player.Pos).Xz.LengthSquared > 72 * 256) continue;
        Objs[i].Update();
        k++;
      }

      fall.InView = k;

      Objs.RemoveAll(it => it.Removed);

      Vector2i chunkPos = fall.Player.Pos.Xz.ToChunkPos();
      if (_prevCPos == chunkPos) return;

      for (int i = -12; i <= 12; i++)
      for (int j = -12; j <= 12; j++)
      {
        int i1 = i + chunkPos.X;
        int j1 = j + chunkPos.Y;
        if (!_chunks.ContainsKey((i1, j1))) _chunks[(i1, j1)] = new chunk((i1, j1));
      }

      _prevCPos = chunkPos;
    }

    public void Render()
    {
      Vector2i chunkPos = fall.Player.Pos.Xz.ToChunkPos();
      for (int i = -8; i <= 8; i++)
      for (int j = -8; j <= 8; j++)
        _chunks[(i + chunkPos.X, j + chunkPos.Y)].Mesh.Render();
      for (int i = 0; i < Objs.Count; i++)
      {
        if ((Objs[i].Pos - fall.Player.Pos).Xz.LengthSquared > 72 * 256) continue;
        Objs[i].Render();
      }
    }

    public static float height_at(Vector2 vec)
    {
      return chunk.height_at(vec);
    }
  }

  public struct chunk
  {
    public readonly mesh Mesh;
    private const float _div = 30f;
    private const int _wh = 16;

    public chunk(Vector2i chunkPos)
    {
      Mesh = new mesh(mesh.draw_mode.TRIANGLE, render_system.BASIC, true, vao.attrib.FLOAT3);

      Span<int> memo = stackalloc int[(_wh + 1) * (_wh + 1)];
      for (int i = 0; i < memo.Length; i++) memo[i] = -1;

      Mesh.Begin();
      for (int i = 0; i < _wh; i++)
      for (int j = 0; j < _wh; j++)
      {
        int i1, i2, i3, i4;
        if ((i1 = memo[i * 17 + j]) == -1)
          i1 = memo[i * 17 + j] = Mesh.Float3(i + chunkPos.X * _wh,
            Noise(i + chunkPos.X * _wh, j + chunkPos.Y * _wh),
            j + chunkPos.Y * _wh).Next();
        if ((i2 = memo[(i + 1) * 17 + j]) == -1)
          i2 = memo[(i + 1) * 17 + j] = Mesh.Float3(i + chunkPos.X * _wh + 1,
            Noise(i + chunkPos.X * _wh + 1, j + chunkPos.Y * _wh),
            j + chunkPos.Y * _wh).Next();
        if ((i3 = memo[(i + 1) * 17 + j + 1]) == -1)
          i3 = memo[(i + 1) * 17 + j + 1] = Mesh.Float3(i + chunkPos.X * _wh + 1,
            Noise(i + chunkPos.X * _wh + 1, j + chunkPos.Y * _wh + 1),
            j + chunkPos.Y * _wh + 1).Next();
        if ((i4 = memo[i * 17 + j + 1]) == -1)
          i4 = memo[i * 17 + j + 1] = Mesh.Float3(i + chunkPos.X * _wh,
            Noise(i + chunkPos.X * _wh, j + chunkPos.Y * _wh + 1),
            j + chunkPos.Y * _wh + 1).Next();
        Mesh.Quad(i1, i2, i3, i4);
      }

      Mesh.End();
    }

    private static float Noise(int x, int y)
    {
      return SimplexNoise.Noise.CalcPixel2D(x, y, 0.0125f) / _div;
    }

    public static float height_at(Vector2 vec)
    {
      int x1 = (int)MathF.Floor(vec.X);
      int y1 = (int)MathF.Floor(vec.Y);

      float v00 = Noise(x1, y1);
      float v10 = Noise(x1 + 1, y1);
      float v01 = Noise(x1, y1 + 1);
      float v11 = Noise(x1 + 1, y1 + 1);
      float x = vec.X - x1;
      float y = vec.Y - y1;

      // bilinear interpolation
      return (1 - x) * (1 - y) * v00 + x * (1 - y) * v10 + (1 - x) * y * v01 + x * y * v11;
    }
  }
}