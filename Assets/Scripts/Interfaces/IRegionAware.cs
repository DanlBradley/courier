using WorldGeneration;

namespace Interfaces
{
    public interface IRegionAware
    {
        void OnRegionLoaded(Region regionData);
        // void OnRegionLoaded(Region regionData, float regionSize);
        void OnRegionUnloading();
    }
}