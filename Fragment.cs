public class Fragment
{
    private readonly int _number;
    private readonly float _x, _y, _angle;

    public int Number => _number;
    public float X => _x;
    public float Y => _y;
    public float Angle => _angle;


    public Fragment(int number, float x, float y, float angle)
    {
        _number = number;
        _x = x;
        _y = y;
        _angle = angle;
    }
}

