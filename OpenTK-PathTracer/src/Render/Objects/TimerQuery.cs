using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK_PathTracer.Render.Objects
{
    class TimerQuery : IDisposable
    {
        public float ElapsedMilliseconds { get; private set; }

        private readonly Stopwatch timer = new Stopwatch();
        private bool doStopAndReset = false;

        public readonly int ID;
        public int UpdateRate;
        public TimerQuery(int updatePeriodInMs)
        {
            GL.CreateQueries(QueryTarget.TimeElapsed, 1, out ID);
            UpdateRate = updatePeriodInMs;
        }


        /// <summary>
        /// If <see cref="UpdateRate"/> milliseconds are elapsed since the last <see cref="TimerQuery"/>, a new one on will be issued, which measures all render commands from now until <see cref="StopAndReset"/>.
        /// </summary>
        public void Start()
        {
            if (!timer.IsRunning || timer.ElapsedMilliseconds >= UpdateRate)
            {
                GL.BeginQuery(QueryTarget.TimeElapsed, ID);
                doStopAndReset = true;
                timer.Restart();
            }
        }

        /// <summary>
        /// Resets the <see cref="TimerQuery"/> and stores the result in <see cref="ElapsedMilliseconds"/>
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
