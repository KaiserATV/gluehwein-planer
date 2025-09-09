/// <summary>
/// This class atteches to any element in the scene and is used to log all relevant stats to a csv file.
/// </summary>
public interface IFPSLogger
{
    /// <summary>
    /// When the application ends playing all the values are written to an csv file.
    /// </summary>
    void OnApplicationQuit();
}