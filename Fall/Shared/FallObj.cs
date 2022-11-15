using Fall.Shared.Components;
using OpenTK.Mathematics;

namespace Fall.Shared
{
    public class fall_obj
    {
        private readonly HashSet<component> _components;
        public bool markedForRemoval;

        public Vector3 pos => get<float_pos>().to_vector3(); 
        public Vector3 lerped_pos => get<float_pos>().to_lerped_vector3(); 

        public fall_obj()
        {
            _components = new HashSet<component>();
        }

        public void update()
        {
            foreach (component component in _components)
            {
                component.update(this);
            }
        }

        public void render()
        {
            foreach (component component in _components)
            {
                component.render(this);
            }
        }

        public void collide(fall_obj other)
        {
            foreach (component component in _components)
            {
                component.collide(this, other);
            }
        }

        public void add(component component)
        {
            _components.Add(component);
            _cache[component.get_type()] = component;
        }
        
        private static bool comp_finder<t>(component comp)
        {
            return typeof(t) == comp.get_type();
        }

        private readonly Dictionary<Type, component> _cache = new();

        public t get<t>() where t : component
        {
            if (_cache.TryGetValue(typeof(t), out component comp))
            {
                return (t) comp;
            }
            
            t val = (t)_components.FirstOrDefault(comp_finder<t>, null);
            _cache[typeof(t)] = val;
            return val;
        }
        
        public bool has<t>() where t : component
        {
            return _components.Any(comp_finder<t>);
        }

        public class component
        {
            public virtual void update(fall_obj objIn)
            {
                
            }

            public virtual void render(fall_obj objIn)
            {
                
            }
            
            public virtual void collide(fall_obj objIn, fall_obj other)
            {
                
            }

            public override int GetHashCode()
            {
                return this.get_type().get_hash_code();
            }
        }
    }
}