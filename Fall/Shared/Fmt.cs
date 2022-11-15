namespace Fall.Shared
{
    public sealed class fmt
    {
        
        public static readonly Dictionary<char, fmt> values = new();

        public static readonly fmt black = new(0, '0');
        public static readonly fmt darkblue = new(0xff0000aa, '1');
        public static readonly fmt darkgreen = new(0xff00aa00, '2');
        public static readonly fmt darkcyan = new(0xff00aaaa, '3');
        public static readonly fmt darkred = new(0xffaa0000, '4');
        public static readonly fmt darkpurple = new(0xffaa00aa, '5');
        public static readonly fmt gold = new(0xffffaa00, '6');
        public static readonly fmt gray = new(0xffaaaaaa, '7');
        public static readonly fmt darkgray = new(0xff555555, '8');
        public static readonly fmt blue = new(0xff5555ff, '9');
        public static readonly fmt green = new(0xff55ff55, 'a');
        public static readonly fmt cyan = new(0xff55ffff, 'b');
        public static readonly fmt red = new(0xffff5555, 'c');
        public static readonly fmt purple = new(0xffff55ff, 'd');
        public static readonly fmt yellow = new(0xffffff55, 'e');
        public static readonly fmt white = new(0xffffffff, 'f');
        public static readonly fmt reset = new(0, 'r');

        public readonly uint color;
        private readonly uint _code;

        private fmt(uint color, char code)
        {
            this.color = color;
            _code = code;
            values[code] = this;
        }

        public override string ToString()
        {
            return $"\u00a7{_code}";
        }
    }
}