using FeatureHubSDK;

namespace FHHelper;

public interface IFeatureStateProvider
{
    bool IsEnabled(string featureKey);
}

public class FeatureStateProvider : IFeatureStateProvider
{
    private readonly EdgeFeatureHubConfig? _config;
    private readonly bool _isInitialized;

    public FeatureStateProvider(string url, string sdkKey)
    {
        try
        {
            var config = new EdgeFeatureHubConfig(url, sdkKey);
            config.Init().Wait(TimeSpan.FromSeconds(20));
            _config = config;
            _isInitialized = true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"FeatureHub initialization failed: {e.Message}");
            _config = null;
            _isInitialized = false;
        }
    }

    public bool IsEnabled(string featureKey)
    {
        if (!_isInitialized || _config == null)
        {
            return false;
        }
        
        return (bool)_config.Repository[featureKey].Value!;
    }
}