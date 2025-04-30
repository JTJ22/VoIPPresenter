using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace VoIPPresenter.Logic
{

  /// <summary>
  /// Struct to hold the parameters for the listener. Unmanaged as these are used in C DLL. 
  /// </summary>
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
  public struct socket_params
  {
    public int port_number;
    [MarshalAs(UnmanagedType.LPStr)]
    public string ip_address;
    [MarshalAs(UnmanagedType.LPStr)]
    public string audio_path;
  }

  /// <summary>
  /// Class representing an active listener.
  /// </summary>
  public class ActiveListener
  {
    public bool isActive { get; set; }
    private Thread listenerThread;
    private IntPtr listenerParams;
    public int portNo { get; set; }
    public string ipAddress { get; set; }
    public string path { get; set; }
    private List<string> imageFiles = new();
    private List<string> audioFiles = new();

    /// <summary>
    /// Constructor for the ActiveListener class. Starts the listener on a separate thread.
    /// </summary>
    /// <param name="ipAddress">IP being used to listen</param>
    /// <param name="portNo">Port listening on</param>
    public ActiveListener(string ipAddress, int portNo)
    {
      this.portNo = portNo;
      this.ipAddress = ipAddress;
      this.path = GenerateUniquePath(ipAddress, portNo);
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

    /// <summary>
    /// Starts the listener on a separate thread.
    /// </summary>
    /// <param name="portNo">Port number to listen on</param>
    /// <param name="ipAddress">IP address to listen on</param>
    private void StartAsync(string ipAddress, int portNo)
    {
      listenerParams = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(socket_params)));
      Marshal.StructureToPtr(new socket_params {ip_address = ipAddress, port_number = portNo, audio_path = path}, listenerParams, false);

      main(listenerParams);
    }

    /// <summary>
    /// Stops the current listener.
    /// </summary>
    public void StopCall()
    {
      if(isActive)
      {
        Stop();
      }
    }

    /// <summary>
    /// Stops the current listener.
    /// </summary>
    private void Stop()
    {
      isActive = false;
      stop_listener(listenerParams);
      listenerThread.Join();
      Marshal.FreeHGlobal(listenerParams);
    }

    /// <summary>
    /// Creates a unique path for the audio files, allows for multiple listeners to run at the same time.
    /// </summary>
    /// <param name="ipAddress">Ip address of the listener</param>
    /// <param name="portNo">Port number of the listener</param>
    /// <returns>Unique path for the listener</returns>
    private string GenerateUniquePath(string ipAddress, int portNo)
    {
      string baseDirectory = Directory.GetCurrentDirectory();
      string folderName = $"{ipAddress.Replace(".", "_")}_{portNo}_{DateTime.Now:yyyyMMdd_HHmmss}";
      string path = Path.Combine(baseDirectory, "wwwroot", "Audio", folderName);

      if(!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }

      return path;
    }

    /// <summary>
    /// Creates images showing the waveform and spectrogram of the audio file.
    /// </summary>
    /// <param name="audioPath">The path to a wav file</param>
    /// <param name="outputPath">Output of the image</param>
    /// <param name="imageName">Unique name for the image</param>>
    /// <returns></returns>
    public async Task<bool> CreateWaveformAudio(string audioPath, string outputPath, string imageName)
    {
      ProcessStartInfo startInfo = new ProcessStartInfo
      {
        FileName = "python",
        Arguments = $"PythonScripts/GenerateImages.py \"{audioPath}\" \"{outputPath}\" \"{imageName}\"",
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

      AddImage(Path.Combine(outputPath, $"{imageName}_wave.png"));
      AddImage(Path.Combine(outputPath, $"{imageName}_spec.png"));
      return true;
    }

    /// <summary>
    /// Adds an image to the list of images.
    /// </summary>
    /// <param name="imagePath">Path of the image to add</param>
    public void AddImage(string imagePath)
    {
      if(!DoesImageExist(imagePath))
      {
        imageFiles.Add(imagePath);
      }
    }

    /// <summary>
    /// Checks if an image exists.
    /// </summary>
    /// <param name="path">The path being checked, prevents duplication</param>
    /// <returns>True if image exists</returns>
    public bool DoesImageExist(string path) => imageFiles.Contains(path);

    /// <summary>
    /// Returns the list of images.
    /// </summary>
    /// <returns>List of image directories</returns>
    public List<string> GetImages() => imageFiles;

    /// <summary>
    /// Get the directories of all audio files.
    /// </summary>
    /// <returns>List of all audio files</returns>
    public List<string> GetAudioFiles()
    {
      string[] files = Directory.GetFiles(path);
      foreach(string paths in files)
      {
        if(!audioFiles.Contains(paths))
        {
          audioFiles.Add(paths);
        }
      }

      return audioFiles;
    }
  }
}
