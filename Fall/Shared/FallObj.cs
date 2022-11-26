using Fall.Shared.Components;
using OpenTK.Mathematics;

namespace Fall.Shared
{
  public class fall_obj
  {
    private readonly component[] _cache = new component[(int)component.type.SIZE];
    public bool Removed;
    public bool Updates;

    public Vector3 Pos => Get<float_pos>(component.type.FLOAT_POS).to_vector3();
    public Vector3 LerpedPos => Get<float_pos>(component.type.FLOAT_POS).to_lerped_vector3();

    public void Update()
    {
      foreach (component component in _cache) component?.Update(this);
    }

    public void Render()
    {
      foreach (component component in _cache) component?.Render(this);
    }

    public void Collide(fall_obj other)
    {
      foreach (component component in _cache) component?.Collide(this, other);
    }

    public void Add(component component)
    {
      _cache[(int)component.Type] = component;
    }

    public T Get<T>(component.type t) where T : component
    {
      return (T)_cache[(int)t];
    }

    public bool Has(component.type t)
    {
      return _cache[(int)t] != null;
    }

    public class component
    {
      public enum type
      {
        NOT_A_TYPE,
        CAMERA,
        COLLISION,
        FLOAT_POS,
        INT_POS,
        MODEL_3D,
        PLAYER,
        SNOW,
        TAG,
        TREE,
        SIZE
      }

      public readonly type Type;

      protected component(type type)
      {
        Type = type;
      }

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