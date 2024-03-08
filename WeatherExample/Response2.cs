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
        
        // Get latitude and longitude from user input
        Console.WriteLine("Enter latitude:");
        double latitude = Convert.ToDouble(Console.ReadLine());
        Console.WriteLine("Enter longitude:");
        double longitude = Convert.ToDouble(Console.ReadLine());
        
        // Get time from user input
        Console.WriteLine("Enter time (in UNIX timestamp format):");
        long time = Convert.ToInt64(Console.ReadLine());
        
        string apiUrl = $"https://api.openweathermap.org/data/2.5/onecall?lat={latitude}&lon={longitude}&exclude=minutely&appid={apiKey}";

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
                    EvaluateManureRules(weatherData, time);

                    // Evaluate rules for fertilizer application
                    EvaluateFertilizerRules(weatherData, time);
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

    static void EvaluateManureRules(WeatherData weatherData, long time)
    {
        int index = (int)((time - weatherData.current.dt) / 3600); // Convert UNIX timestamp to index in hourly array
        double precipProbability = weatherData.hourly[index].pop * 100; // Convert probability to percentage
        double precipAccumulation = weatherData.hourly[index].rain + weatherData.hourly[index].snow;

        if (precipProbability < 40 && precipAccumulation < 0.4)
        {
            Console.WriteLine("You can apply manure at the specified time.");
        }
        else
        {
            Console.WriteLine("Warning: Conditions are not favorable for manure application at the specified time.");
        }
    }

    static void EvaluateFertilizerRules(WeatherData weatherData, long time)
    {
        int startIndex = (int)((time - weatherData.current.dt) / 3600); // Convert UNIX timestamp to index in hourly array
        double precipProbability = 0;
        double precipAccumulation = 0;

        // Accumulate precipitation for the next 12 hours
        for (int i = startIndex; i < startIndex + 12; i++)
        {
            precipProbability += weatherData.hourly[i].pop;
            precipAccumulation += weatherData.hourly[i].rain + weatherData.hourly[i].snow;
        }

        precipProbability *= 100; // Convert probability to percentage

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
    public CurrentWeather current { get; set; }
    public HourlyForecast[] hourly { get; set; }
}

public class CurrentWeather
{
    public long dt { get; set; } // Time of data calculation, UNIX, UTC
}

public class HourlyForecast
{
    public double pop { get; set; } // Probability of precipitation
    public double rain { get; set; } // Rain volume
    public double snow { get; set; } // Snow volume
}
