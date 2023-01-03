namespace Fall.Shared.Components
{
  public class tree : fall_obj.component
  {
    public tree() : base(fall_obj.comp_type.Tree)
    {
    }

    public override void Update(fall_obj objIn)
    {
      base.Update(objIn);

      if (fall.InView > snow.MODEL_COUNT * 400) return;

      fall_obj obj = new()
      {
        Updates = true
      };
      obj.Add(new snow());
      obj.Add(new float_pos
      {
        X = objIn.Pos.X, Y = objIn.Pos.Y + 16 + rand.NextFloat(0, 12), Z = objIn.Pos.Z
      });
      fall.World.Objs.Add(obj);
    }
  }
} 