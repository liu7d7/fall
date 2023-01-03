using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class snow : fall_obj.component
  {
    private static readonly model3d[] _snow;

    private readonly Vector3 _dir = new(rand.NextFloat(-0.7f, 0.7f), rand.NextFloat(-0.6f, -0.2f),
      rand.NextFloat(-0.7f, 0.7f));

    private readonly int _mul = rand.Next(0, 2) == 1 ? -1 : 1;

    private readonly int _offset = rand.Next(0, 360);

    private float _landing = float.MaxValue;
    
    private readonly int _modelIndex;
    private static int _modelIndexCount;

    public const int MODEL_COUNT = 2;

    static snow()
    {
      _snow = new model3d[MODEL_COUNT];
      for (int i = 0; i < MODEL_COUNT; i++)
      {
        _snow[i] = model3d.Read("snow", new Dictionary<string, uint> { { "hi", (uint)i } });
        _snow[i].Scale(0.5f);
      }
    }

    public snow() : base(fall_obj.comp_type.Snow)
    {
      _modelIndexCount++;
      _modelIndex = _modelIndexCount % MODEL_COUNT;
    }

    public override void Render(fall_obj objIn)
    {
      base.Render(objIn);

      glh.Push();
      glh.Translate(-objIn.LerpedPos);
      glh.Rotate(Environment.TickCount / 2f % 360 * _mul, 0.5f, 1, 0.5f);
      glh.Scale(MathHelper.Clamp(1 - (Environment.TickCount - _landing) / 1000f, 0, 1));
      glh.Translate(objIn.LerpedPos);
      _snow[_modelIndex].Render(objIn.LerpedPos);
      glh.Pop();
    }

    public override void Update(fall_obj objIn)
    {
      base.Update(objIn);

      float_pos pos = float_pos.Get(objIn);
      pos.SetPrev();

      if (pos.Y > world.HeightAt((pos.X, pos.Z)) - 0.5f)
      {
        float x = ((Environment.TickCount + _offset) / 3f % 360f).Rad();
        pos.X += _dir.X * (MathF.Sin(x * 0.5f) / 4f + 1.5f);
        pos.Y += _dir.Y * (MathF.Sin(x * 1.6f) * MathF.Sin(x * 1.3f) * MathF.Sin(x * 0.7f)) - 0.1f;
        pos.Z += _dir.Z * (MathF.Cos(x * 0.5f) / 4f + 1.5f);
      }
      else if (Math.Abs(_landing - float.MaxValue) < 0.3f)
      {
        _landing = Environment.TickCount;
      }

      if (Environment.TickCount - _landing > 1000) objIn.Removed = true;
    }
  }
}