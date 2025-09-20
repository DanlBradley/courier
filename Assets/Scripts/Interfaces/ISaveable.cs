namespace Interfaces
{
    public interface ISaveable
    {
        string SaveID { get; }
        object CaptureState();
        void RestoreState(object saveData);
    }
}