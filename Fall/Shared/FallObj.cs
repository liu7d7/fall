using System.Buffers;
using Fall.Shared.Components;
using OpenTK.Mathematics;

namespace Fall.Shared
{
  public class fall_obj
  {
    public readonly component[] Components = ArrayPool<component>.Shared.Rent((int)comp_type.Size);
    public readonly Guid Id = Guid.NewGuid();
    public bool Removed;
    public bool Updates;

    public Vector3 Pos => Has(comp_type.FloatPos) ? float_pos.Get(this).ToVec3() : float_pos_static.Get(this).ToVec3();
    public Vector3 LerpedPos => Has(comp_type.FloatPos) ? float_pos.Get(this).ToLerpedVec3() : float_pos_static.Get(this).ToVec3();

    public void Update()
    {
      Span<component> c = Components;
      for (int i = 0; i < Components.Length; i++)
      {
        c[i]?.Update(this);
      }
    }

    public void Render()
    {
      Span<component> c = Components;
      for (int i = 0; i < Components.Length; i++)
      {
        c[i]?.Render(this);
      }
    }

    public void Collide(fall_obj other)
    {
      Span<component> c = Components;
      for (int i = 0; i < Components.Length; i++)
      {
        c[i]?.Collide(this, other);
      }
    }

    public void Add(component component)
    {
      Components[(int)component.Type] = component;
    }

    public T Get<T>(comp_type t) where T : component
    {
      return (T)Components[(int)t];
    }

    public bool Has(comp_type t)
    {
      return Components[(int)t] != null;
    }

    public static bool operator ==(fall_obj one, fall_obj two)
    {
      return one?.Equals(two) ?? false;
    }

    public static bool operator !=(fall_obj one, fall_obj two)
    {
      return !(one == two);
    }

    public override bool Equals(object obj)
    {
      return obj is fall_obj other && Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
      return Id.GetHashCode();
    }

    public class component
    {
      public readonly comp_type Type;

      protected component(comp_type type)
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

    public enum comp_type
    {
      NotAType,
      Camera,
      Collision,
      FloatPos,
      Model3D,
      Player,
      Snow,
      Tag,
      Tree,
      Projectile,
      FloatPosStatic,
      Size,
    }
  }
}