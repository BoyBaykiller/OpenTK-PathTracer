using OpenTK;

namespace OpenTK_PathTracer
{
    struct IVector2
    {
        public int X, Y;

        public IVector2(int x, int y)
        {
            X = x; Y = y;
        }

        public IVector2(int v)
        {
            X = v; Y = v;
        }

        public IVector2(Vector2 vector2)
        {
            X = (int)vector2.X; Y = (int)vector2.Y;
        }

        public IVector2(IVector2 iVector2)
        {
            X = iVector2.X; Y = iVector2.Y;
        }

        public static IVector2 operator *(IVector2 vector2, float a) => new IVector2((int)(vector2.X * a), (int)(vector2.Y * a));
        public static IVector2 operator *(float a, IVector2 vector2) => new IVector2((int)(vector2.X * a), (int)(vector2.Y * a));
    }
}
