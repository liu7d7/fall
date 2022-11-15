namespace Fall.Shared.Components
{
    public class tag : fall_obj.component
    {
        public int id;
        public string name;

        public tag(int id, string name = "")
        {
            this.id = id;
            this.name = name;
        }
    }
}