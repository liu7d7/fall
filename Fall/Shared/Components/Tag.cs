namespace Fall.Shared.Components
{
  public class tag : fall_obj.component
  {
    public int Id;
    public string Name;

    public tag(int id, string name = "") : base(type.TAG)
    {
      Id = id;
      Name = name;
    }
  }
}