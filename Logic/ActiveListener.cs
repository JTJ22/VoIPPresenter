namespace VoIPPresenter.Logic
{
  public class ActiveListener
  {
    private int portNo;
    private string ipAddress;
    private bool isActive;
    private int[] decoderType;
    public ActiveListener(int portNo, string ipAddress, bool isActive, int[] decoderType)
    {
      this.portNo = portNo;
      this.ipAddress = ipAddress;
      this.isActive = isActive;
      this.decoderType = decoderType;
    }

    public bool MakeInactive()
    {
      isActive = false;
      return !isActive;
    }

  }
}
