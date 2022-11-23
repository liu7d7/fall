using Fall.Shared.Components;
using OpenTK.Mathematics;

namespace Fall.Shared
{
  public class fall_obj
  {
    private readonly Dictionary<Type, component> _cache = new();
    public bool Updates;
    public bool Removed;

    public Vector3 Pos => Get<float_pos>().to_vector3();
    public Vector3 LerpedPos => Get<float_pos>().to_lerped_vector3();

    public void Update()
    {
      foreach (KeyValuePair<Type, component> component in _cache) component.Value.Update(this);
    }

    public void Render()
    {
      foreach (KeyValuePair<Type, component> component in _cache) component.Value.Render(this);
    }

    public void Collide(fall_obj other)
    {
      foreach (KeyValuePair<Type, component> component in _cache) component.Value.Collide(this, other);
    }

    public void Add(component component)
    {
      _cache[component.GetType()] = component;
    }

    public T Get<T>() where T : component
    {
      return _cache[typeof(T)] as T;
    }

    public bool Has<T>() where T : component
    {
      return _cache.ContainsKey(typeof(T));
    }

    public class component
    {
      public virtual void Update(fall_obj objIn)
      {
      }

      public virtual void Render(fall_obj objIn)
      {
      }

      public virtual void Collide(fall_obj objIn, fall_obj other)
      {
      }

      public override int GetHashCode()
      {
        return GetType().GetHashCode();
      }
    }
  }
}