namespace Fall.Shared.Tweens
{
  public class list_tween : base_tween
  {
    private readonly base_tween[] _list;
    private int _idx;
    private float _lastOut;

    public list_tween(params base_tween[] list)
    {
      if (list.Any(it => it.Infinite))
        throw new Exception("Tried to create ListTween with infinite duration tween");
      _list = list;
      _idx = 0;
      Duration = list.Sum(it => it.Duration);
    }

    public override float Output()
    {
      if (!_list[_idx].Done()) return _lastOut = _list[_idx].Output();
      if (_idx == _list.Length - 1) return _lastOut;
      _idx++;
      _list[_idx].LastActivation = Environment.TickCount;
      return _lastOut = _list[_idx].Output();
    }

    public override float output_at(float time)
    {
      float duration = 0f;
      float lastDuration = 0f;
      int i;
      for (i = 0; i < _list.Length; i++)
      {
        lastDuration = duration;
        duration += _list[i].Duration;
        if (duration > time) break;
      }

      i--;
      return _list[i].output_at(time - lastDuration - LastActivation);
    }

    public override bool Done()
    {
      return _idx == _list.Length - 1 && _list[_idx].Done();
    }
  }
}