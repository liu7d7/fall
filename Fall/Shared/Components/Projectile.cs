using OpenTK.Mathematics;

namespace Fall.Shared.Components
{
  public class projectile : fall_obj.component
  {
    private static readonly model3d _model;
    private readonly Vector3 _dir;
    private readonly float _speed;

    static projectile()
    {
      _model = model3d.Read("icosphere", new Dictionary<string, uint>());
      _model.Scale(0.5f);
    }

    public projectile(Vector3 dir, float speed) : base(fall_obj.comp_type.Projectile)
    {
      _dir = dir;
      _speed = speed;
    }

    public override void Render(fall_obj objIn)
    {
      base.Render(objIn);

      _model.Render(objIn.LerpedPos);
    }

    public override void Update(fall_obj objIn)
    {
      base.Update(objIn);

      float_pos pos = float_pos.Get(objIn);
      pos.SetPrev();
      pos.IncVec3(_dir * _speed);
    }
  }
}