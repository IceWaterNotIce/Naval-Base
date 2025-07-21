using UnityEngine;

public static class WeatherManager
{
    public enum WeatherType { Clear, Fog, Night }
    public static WeatherType CurrentWeather = WeatherType.Clear;

    public static float GetCurrentVisibility()
    {
        return CurrentWeather switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Fog => 0.3f,
            WeatherType.Night => 0.6f,
            _ => 1.0f
        };
    }
}
