using OpenTK;

namespace OpenTK_PathTracer
{
    struct AABB
    {
        public Vector3 Position;

        public Vector3 Dimensions 
        { 
            get => Max - Min;
            set
            {
                Min = Position - (value * 0.5f);
                Max = Position + (value * 0.5f);
            }
        }
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }
        public AABB(Vector3 position, Vector3 dimensions)
        {
            Position = position;
            Min = position - (dimensions * 0.5f);
            Max = position + (dimensions * 0.5f);
        }

        public void Update()
        {
            Vector3 tempDimenions = Dimensions;
            Min = Position - (tempDimenions * 0.5f);
            Max = Position + (tempDimenions * 0.5f);
        }


        public override string ToString()
        {
            return $"<P: {Position}, D: {Dimensions}>";
        }
    }
}
