using OpenCvSharp;

namespace OpenCVTest
{
    public class Fragment
    {
        private readonly int _number;
        private readonly float _x, _y, _angle;
        private Mat? _image;

        public int Number => _number;
        public float X => _x;
        public float Y => _y;
        public float Angle => _angle;
        public Mat? Image { get => _image; set => _image = value; }


        public Fragment(int number, float x = 0.0f, float y = 0.0f, float angle = 0.0f, Mat image = null)
        {
            _number = number;
            _x = x;
            _y = y;
            _angle = angle;
            _image = image;
        }
    }
}