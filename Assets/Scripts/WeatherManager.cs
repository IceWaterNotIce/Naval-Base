using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    public enum WeatherType { Clear, Fog, Night }
    public WeatherType CurrentWeather = WeatherType.Clear;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetCurrentVisibility()
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
