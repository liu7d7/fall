namespace Fall.Shared
{
  public sealed class fmt
  {
    public static readonly Dictionary<char, fmt> VALUES = new();

    public static readonly fmt BLACK = new(0, '0');
    public static readonly fmt DARKBLUE = new(0xff0000aa, '1');
    public static readonly fmt DARKGREEN = new(0xff00aa00, '2');
    public static readonly fmt DARKCYAN = new(0xff00aaaa, '3');
    public static readonly fmt DARKRED = new(0xffaa0000, '4');
    public static readonly fmt DARKPURPLE = new(0xffaa00aa, '5');
    public static readonly fmt GOLD = new(0xffffaa00, '6');
    public static readonly fmt GRAY = new(0xffaaaaaa, '7');
    public static readonly fmt DARKGRAY = new(0xff555555, '8');
    public static readonly fmt BLUE = new(0xff5555ff, '9');
    public static readonly fmt GREEN = new(0xff55ff55, 'a');
    public static readonly fmt CYAN = new(0xff55ffff, 'b');
    public static readonly fmt RED = new(0xffff5555, 'c');
    public static readonly fmt PURPLE = new(0xffff55ff, 'd');
    public static readonly fmt YELLOW = new(0xffffff55, 'e');
    public static readonly fmt WHITE = new(0xffffffff, 'f');
    public static readonly fmt RESET = new(0, 'r');
    private readonly uint _code;

    public readonly uint Color;

    private fmt(uint color, char code)
    {
      Color = color;
      _code = code;
      VALUES[code] = this;
    }

    public override string ToString()
    {
      return $"\u00a7{_code}";
    }
  }
}