using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace VoIPPresenter.Logic
{
  /// <summary>
  /// Class to handle the listener in C.
  /// </summary>
  public class HandleListener
  {
    [DllImport("VoIPListener.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int main(string ipAddress, int port_no);

    [DllImport("VoIPListener.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int event_register(HandleReceivedData recDataEvent);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]

    private delegate void HandleReceivedData(string data);

    private ConcurrentDictionary<Guid, ActiveListener> currentListeners = new ConcurrentDictionary<Guid, ActiveListener>();

    private void DataSentEvent(string data)
    {

    }

    public HandleListener() { }

    /// <summary>
    /// Starts the listener on a separate thread.
    /// </summary>
    /// <param name="portNo">Port number to listen on</param>
    /// <returns>True if the code executes properly</returns>
    public async Task<bool> StartAsync(string ipAddress, int portNo)
    {
      bool result = false;
      HandleReceivedData handleReceivedData = new HandleReceivedData(DataSentEvent);
      event_register(handleReceivedData);

      await Task.Run(() =>
      {
        int listenResult = main(ipAddress, portNo);
        result = listenResult == 0;
      });

      return result;
    }

    public async Task<bool> CreateWaveformAudio(string audioPath, string outputPath)
    {
      ProcessStartInfo startInfo = new ProcessStartInfo
      {
        FileName = "python",
        Arguments = $"PythonScripts/GenerateImages.py \"{audioPath}\" \"{outputPath}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };

      using Process? process = Process.Start(startInfo);
      string output = await process.StandardOutput.ReadToEndAsync();
      string error = await process.StandardError.ReadToEndAsync();
      process.WaitForExit();

      if(process.ExitCode != 0)
      {
        Console.Error.WriteLine($"Python error: {error}");
        return false;
      }
      return true;
    }
  }
}
