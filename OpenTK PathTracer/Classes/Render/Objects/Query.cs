using System.Diagnostics;

using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class Query
    {
        public int ID { get; private set; }

        public float ElapsedMilliseconds { get; private set; }

        public uint UpdateQueryRate;
        

        private Stopwatch _timer = Stopwatch.StartNew();
        private bool _doUpdate = false;

        public Query(uint updateQueryRate)
        {
            ID = GL.GenQuery();
            UpdateQueryRate = updateQueryRate;
        }


        /// <summary>
        /// Starts a timer on the GPU
        /// </summary>
        public void Start()
        {
            if (_timer.ElapsedMilliseconds >= UpdateQueryRate)
            {
                GL.BeginQuery(QueryTarget.TimeElapsed, ID);
                _doUpdate = true;
                _timer.Restart();
            }
                
        }

        /// <summary>
        /// Resets the timer on the GPU and gets the result
        /// </summary>
        public void StopAndReset()
        {
            if (_doUpdate)
            {
                GL.EndQuery(QueryTarget.TimeElapsed);
                GL.GetQueryObject(ID, GetQueryObjectParam.QueryResult, out int elapsedTime);
                ElapsedMilliseconds = elapsedTime / 1000000f;
                _doUpdate = false;
            }
        }
    }
}
