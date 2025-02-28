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
    private ConcurrentDictionary<Guid, ActiveListener> currentListeners = new ConcurrentDictionary<Guid, ActiveListener>();

    public HandleListener() { }

    public bool AddListener(ActiveListener listener)
    {
      Guid newGuid = Guid.NewGuid();
      return currentListeners.TryAdd(newGuid, listener);
    }

    public ConcurrentDictionary<Guid, ActiveListener> GetListeners()
    {
      return currentListeners;
    }
  }
}
