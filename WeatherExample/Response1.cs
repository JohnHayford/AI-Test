using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // Replace "YOUR_API_KEY" with your actual OpenWeatherMap API key
        string apiKey = "YOUR_API_KEY";
        string apiUrl = $"https://api.openweathermap.org/data/2.5/onecall?lat=41.8781&lon=-87.6298&exclude=minutely&appid={apiKey}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(json);

                    // Evaluate rules for manure application
                    EvaluateManureRules(weatherData);

                    // Evaluate rules for fertilizer application
                    EvaluateFertilizerRules(weatherData);
                }
                else
                {
                    Console.WriteLine("Error fetching weather data. Status code: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }

    static void EvaluateManureRules(WeatherData weatherData)
    {
        double precipProbability = weatherData.daily[0].pop * 100; // Convert probability to percentage
        double precipAccumulation = weatherData.daily[0].rain + weatherData.daily[0].snow;

        if (precipProbability < 40 && precipAccumulation < 0.4)
        {
            Console.WriteLine("You can apply manure today.");
        }
        else
        {
            Console.WriteLine("Warning: Conditions are not favorable for manure application today.");
        }
    }

    static void EvaluateFertilizerRules(WeatherData weatherData)
    {
        double precipProbability = weatherData.hourly[0].pop * 100; // Convert probability to percentage
        double precipAccumulation = 0;

        // Accumulate precipitation for the next 12 hours
        for (int i = 0; i < 12; i++)
        {
            precipAccumulation += weatherData.hourly[i].rain + weatherData.hourly[i].snow;
        }

        if (precipProbability < 40 && precipAccumulation < 0.4)
        {
            Console.WriteLine("You can apply fertilizer for the next 12 hours.");
        }
        else
        {
            Console.WriteLine("Warning: Conditions are not favorable for fertilizer application in the next 12 hours.");
        }
    }
}

// Define classes to deserialize JSON response from OpenWeatherMap API
public class WeatherData
{
    public HourlyForecast[] hourly { get; set; }
    public DailyForecast[] daily { get; set; }
}

public class HourlyForecast
{
    public double pop { get; set; } // Probability of precipitation
    public double rain { get; set; } // Rain volume
    public double snow { get; set; } // Snow volume
}

public class DailyForecast
{
    public double pop { get; set; } // Probability of precipitation
    public double rain { get; set; } // Rain volume
    public double snow { get; set; } // Snow volume
}
