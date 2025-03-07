using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace VoIPPresenter.Logic
{
  /// <summary>
  /// Class to handle the listener in C.
  /// </summary>
  public class HandleListener
  {
    private ConcurrentDictionary<Guid, ActiveListener> currentListeners = new ConcurrentDictionary<Guid, ActiveListener>();

    public HandleListener() { }

    /// <summary>
    /// Creates a new listener and adds it to the dictionary.
    /// </summary>
    /// <param name="portNo">Port of the new listener</param>
    /// <param name="ipAddress">IP of the new listener</param>
    /// <returns>True if added</returns>
    public bool AddListener(int portNo, string ipAddress)
    {
      Guid newGuid = Guid.NewGuid();
      return currentListeners.TryAdd(newGuid, new ActiveListener(ipAddress, portNo));
    }

    /// <summary>
    /// Checks if the listener is already active. Will not start a new listener if one is already active.
    /// </summary>
    /// <param name="portNo">Port number of listener</param>
    /// <param name="ipAddress">IP of new listener</param>
    /// <returns>False if this does not exist</returns>
    public bool CheckCurrentListeners(int portNo, string ipAddress)
    {
      foreach(KeyValuePair<Guid, ActiveListener> listener in currentListeners)
      {
        if(listener.Value.portNo != portNo && listener.Value.ipAddress != ipAddress && !listener.Value.isActive)
        {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Stops the listener based on the ID.
    /// </summary>
    /// <param name="listenerId">ID of listener that needs to stop.</param>
    public void StopListener(Guid listenerId) 
    {
      if(currentListeners.TryGetValue(listenerId, out ActiveListener? listener))
      {
        listener.StopCall();
      }
    }

    /// <summary>
    /// Returns a dictionary of all the current listeners.
    /// </summary>
    /// <returns>All current listeners</returns>
    public ConcurrentDictionary<Guid, ActiveListener> GetListeners() => currentListeners;

    /// <summary>
    /// Gets a listener based on the ID.
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public ActiveListener GetListener(Guid Id) => currentListeners.TryGetValue(Id, out ActiveListener listener) ? listener : null;
  }
}
