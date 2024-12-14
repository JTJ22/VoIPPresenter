using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VoIPPresenter.Logic
{
  internal class HandleListener
  {
    [DllImport("VoIPListener.dll", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int main(int port_no);

    /// <summary>
    /// Runs the listener in C on a separate thread as to not block the main thread.
    /// </summary>
    /// <returns></returns>
    internal static async Task Run()
    {
      HandleListener listener = new HandleListener();
      await listener.StartAsync(514);
    }

    /// <summary>
    /// Starts the listener on a separate thread.
    /// </summary>
    /// <param name="portNo">Port number to listen on</param>
    /// <returns>True if the code executes properly</returns>
    internal async Task<bool> StartAsync(int portNo)
    {
      bool result = false;
      await Task.Run(() =>
      {
        int listenResult = main(portNo);
        if(listenResult == 0)
        {
          result = true;
        }
      });

      return result;
    }


  }
}
