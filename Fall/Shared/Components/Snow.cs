using Fall.Engine;
using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class snow : fall_obj.component
  {
    private static readonly model3d _snow;

    static snow()
    {
      _snow = model3d.Read("snow", new Dictionary<string, uint> { {"hi", 0} });
      _snow.Scale(0.5f);
    }
    
    private readonly Vector3 _dir = new(rand.NextFloat(-0.7f, 0.7f), rand.NextFloat(-0.6f, -0.2f), rand.NextFloat(-0.7f, 0.7f));
    
    private float _landing = float.MaxValue;
    private readonly int _offset = rand.Next(0, 360);

    public override void Render(fall_obj objIn)
    {
      base.Render(objIn);
      
      render_system.Push();
      render_system.Translate(-objIn.Pos);
      render_system.Scale(MathHelper.Clamp(1 - (Environment.TickCount - _landing) / 1000f, 0, 1));
      render_system.Translate(objIn.Pos);
      _snow.Render(objIn.LerpedPos);
      render_system.Pop();
    }

    public override void Update(fall_obj objIn)
    {
      base.Update(objIn);

      float_pos pos = objIn.Get<float_pos>();
      pos.set_prev();
      
      if (pos.Y > world.HeightAt((pos.X, pos.Z)) - 0.5f)
      {
        float x = ((Environment.TickCount + _offset) / 3f % 360f).Rad();
        pos.X += _dir.X * (MathF.Sin(x) / 2f + 1f);
        pos.Y += _dir.Y * (MathF.Sin(x * 1.6f) * MathF.Sin(x * 1.3f) * MathF.Sin(x * 0.7f)) - 0.1f;
        pos.Z += _dir.Z * (MathF.Cos(x) / 2f + 1f);
      }
      else if (Math.Abs(_landing - float.MaxValue) < 0.3f)
      {
        _landing = Environment.TickCount;
      }
      
      if (Environment.TickCount - _landing > 1000)
      {
        objIn.Removed = true;
      }
    }
  }
}