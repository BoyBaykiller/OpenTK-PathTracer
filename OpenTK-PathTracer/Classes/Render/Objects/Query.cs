using System;
using System.Diagnostics;

using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class Query : IDisposable
    {
        public readonly int ID;
        public float ElapsedMilliseconds { get; private set; }


        private readonly Stopwatch timer = new Stopwatch();
        private bool doStopAndReset = false;

        public uint UpdateQueryRate;
        public Query(uint updateQueryRate)
        {
            ID = GL.GenQuery();
            UpdateQueryRate = updateQueryRate;
        }


        /// <summary>
        /// If <paramref name="UpdateQueryRate"/> milliseconds are elapsed since the last Query, a new Query on the GPU, which captures all render commands from now until StopAndReset, is started.
        /// </summary>
        public void Start()
        {
            if (!timer.IsRunning || timer.ElapsedMilliseconds >= UpdateQueryRate)
            {
                GL.BeginQuery(QueryTarget.TimeElapsed, ID);
                doStopAndReset = true;
                timer.Restart();
            }
        }

        /// <summary>
        /// Resets the Query on the GPU and stores the result in <paramref name="ElapsedMilliseconds"/>
        /// </summary>
        public void StopAndReset()
        {
            if (doStopAndReset)
            {
                GL.EndQuery(QueryTarget.TimeElapsed);
                GL.GetQueryObject(ID, GetQueryObjectParam.QueryResult, out int elapsedTime);
                ElapsedMilliseconds = elapsedTime / 1000000f;
                doStopAndReset = false;
            }
        }

        public void Dispose()
        {
            GL.DeleteQuery(ID);
        }
    }
}
