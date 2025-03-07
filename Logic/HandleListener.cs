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

    public bool AddListener(int portNo, string ipAddress)
    {
      Guid newGuid = Guid.NewGuid();
      return currentListeners.TryAdd(newGuid, new ActiveListener(ipAddress, portNo));
    }

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

    public void StopListener(Guid listenerId)
    {
      if(currentListeners.TryGetValue(listenerId, out ActiveListener? listener))
      {
        listener.StopCall();
      }
    }

    public ConcurrentDictionary<Guid, ActiveListener> GetListeners()
    {
      return currentListeners;
    }

    public ActiveListener GetListener(Guid Id)
    {
      if(currentListeners.TryGetValue(Id, out ActiveListener? listener))
      {
        return listener;
      }
      return null;
    }

  }
}
