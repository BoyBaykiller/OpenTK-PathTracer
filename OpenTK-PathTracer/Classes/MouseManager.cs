using OpenTK;
using OpenTK.Input;

namespace OpenTK_PathTracer
{
    public static class MouseManager
    {
        private static MouseState lastMouseState;
        private static MouseState thisMouseState;
        public static void Update(MouseState mouseState)
        {
            lastMouseState = thisMouseState;
            thisMouseState = mouseState;
        }

        public static Vector2 DeltaPosition => new Vector2(thisMouseState.X - lastMouseState.X, thisMouseState.Y - lastMouseState.Y);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if mouseButton is down this update but not last one</returns>
        public static bool IsButtonTouched(MouseButton mouseButton) => thisMouseState.IsButtonDown(mouseButton) && lastMouseState.IsButtonUp(mouseButton);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if mouseButton is down</returns>
        public static bool IsButtonDown(MouseButton mouseButton) => thisMouseState.IsButtonDown(mouseButton);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if mouseButton is up this update but not last one</returns>
        public static bool IsButtonUp(MouseButton mouseButton) => thisMouseState.IsButtonUp(mouseButton) && lastMouseState.IsButtonDown(mouseButton);
    }
}
