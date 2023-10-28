using System.Threading;

namespace CCSWE.nanoFramework.Threading
{
    /// <summary>
    /// Utility "extension" methods for <see cref="WaitHandle"/>.
    /// </summary>
    public static class WaitHandles
    {
        /// <summary>
        /// Waits for all the elements in the specified array to receive a signal.
        /// </summary>
        /// <param name="waitHandles">A WaitHandle array containing the objects for which the current instance will wait. This array cannot contain multiple references to the same object.</param>
        /// <returns>true when every element in waitHandles has received a signal; otherwise the method never returns.</returns>
        public static bool WaitAll(params WaitHandle[] waitHandles)
        {
            return WaitHandle.WaitAll(waitHandles);
        }

        /// <summary>
        /// Waits for all the elements in the specified array to receive a signal, using an Int32 value to specify the time interval and specifying whether to exit the synchronization domain before the wait.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it afterward; otherwise, false.</param>
        /// <param name="waitHandles">A WaitHandle array containing the objects for which the current instance will wait. This array cannot contain multiple references to the same object (duplicates).</param>
        /// <returns>true when every element in waitHandles has received a signal; otherwise, false.</returns>
        public static bool WaitAll(int millisecondsTimeout, bool exitContext, params WaitHandle[] waitHandles)
        {
            return WaitHandle.WaitAll(waitHandles, millisecondsTimeout, exitContext);
        }

        /// <summary>
        /// Waits for any of the elements in the specified array to receive a signal.
        /// </summary>
        /// <param name="waitHandles">A WaitHandle array containing the objects for which the current instance will wait.</param>
        /// <returns>The array index of the object that satisfied the wait.</returns>
        public static int WaitAny(params WaitHandle[] waitHandles)
        {
            return WaitHandle.WaitAny(waitHandles);
        }

        /// <summary>
        /// Waits for any of the elements in the specified array to receive a signal, using a 32-bit signed integer to specify the time interval, and specifying whether to exit the synchronization domain before the wait.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it afterward; otherwise, false.</param>
        /// <param name="waitHandles">A WaitHandle array containing the objects for which the current instance will wait.</param>
        /// <returns>The array index of the object that satisfied the wait, or WaitTimeout if no object satisfied the wait and a time interval equivalent to millisecondsTimeout has passed.</returns>
        public static int WaitAny(int millisecondsTimeout, bool exitContext, params WaitHandle[] waitHandles)
        {
            return WaitHandle.WaitAny(waitHandles, millisecondsTimeout, exitContext);
        }
    }
}
