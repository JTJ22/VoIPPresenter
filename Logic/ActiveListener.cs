using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VoIPPresenter.Logic
{

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
  public struct socket_params
  {
    public int port_number;
    [MarshalAs(UnmanagedType.LPStr)]
    public string ip_address;
  }
  public class ActiveListener
  {
    private bool isActive;
    private Thread listenerThread;
    private IntPtr listenerParams;
    public ActiveListener(string ipAddress, int portNo)
    {
      isActive = true;
      listenerThread = new Thread(() => StartAsync(ipAddress, portNo));
      listenerThread.Start();
    }

    [DllImport("VoIPListener.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int main(IntPtr listenerParams);

    [DllImport("VoIPListener.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int event_register(HandleReceivedData recDataEvent);

    [DllImport("VoIPListener.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void stop_listener(IntPtr param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]

    private delegate void HandleReceivedData(string data);

    public bool MakeInactive()
    {
      isActive = false;
      return isActive;
    }

    /// <summary>
    /// Starts the listener on a separate thread.
    /// </summary>
    /// <param name="portNo">Port number to listen on</param>
    /// <returns>True if the code executes properly</returns>
    private void StartAsync(string ipAddress, int portNo)
    {
      listenerParams = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(socket_params)));
      Marshal.StructureToPtr(new socket_params { ip_address = ipAddress, port_number = portNo }, listenerParams, false);

      main(listenerParams);
    }

    /// <summary>
    /// Stops the current listener.
    /// </summary>
    public void Stop()
    {
      isActive = false;
      stop_listener(listenerParams);
      listenerThread.Join();
      Marshal.FreeHGlobal(listenerParams);
    }

    /// <summary>
    /// Creates images showing the waveform and spectrogram of the audio file.
    /// </summary>
    /// <param name="audioPath">The path to a wav file</param>
    /// <param name="outputPath">Output of the image</param>
    /// <returns></returns>
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
