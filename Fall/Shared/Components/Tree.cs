namespace Fall.Shared.Components
{
  public class tree : fall_obj.component
  {
    private readonly int _offset = rand.Next(0, 10);

    public override void Update(fall_obj objIn)
    {
      base.Update(objIn);

      if (fall.InView > 100) return;
      
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