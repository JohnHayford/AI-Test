public class CalculatedWeatherResult
{
    public async Task<IEnumerable<AlertData>> GetCalculatedWeatherResultAsync(decimal lat, decimal lon, int dt)
    {
        //Assumes forecast data is fetched and stored in advance.
        var forecasts = (await DataContext.Forecasts.Where(x => x.Latitude == lat && x.Longitude == lon)
             .Where(x => x.Date >= dt)
             .OrderBy(x => x.Date)
             .Take(24).ToListAsync());
        IEnumerable<OpenWeatherMapHourlyData> parsedForecasts = forecasts.Select(x => System.Text.Json.JsonSerializer.Deserialize<OpenWeatherMapHourlyData>(x.Data)).Where(x => x != null);
        var firstForecast = parsedForecasts.FirstOrDefault();
        if (firstForecast == null)
            return new CalculatedWeatherResult();
        List<AlertData> response = new List<AlertData>(){
            CalculatedWeatherResult.FertilizerWarning(parsedForecasts),
            CalculatedWeatherResult.ManureWarning(parsedForecasts),
            CalculatedWeatherResult.FertilizerViolation(parsedForecasts),
            CalculatedWeatherResult.ManureViolation(parsedForecasts)
     };
        return response;
    }
    public static AlertData FertilizerWarning(IEnumerable<OpenWeatherMapHourlyData> parsedForecasts)
    {
        var response = new AlertData()
        {
            Amount = parsedForecasts.Take(12).Sum(f => f.TotalPrecipitationAmount).Convert(UnitsConverter.Units.Millimeters, UnitsConverter.Units.Inches),
            Probability = parsedForecasts.Take(12).Max(f => f.PercentOfPrecipitation),
        };
        if (response.Probability > .4m && response.Amount > 0.8m)
        {
            response.IsAlert = true;
            response.Message = "Fertilizer Warning";
        }
        return response;
    }

    public static AlertData ManureWarning(IEnumerable<OpenWeatherMapHourlyData> parsedForecasts)
    {
        var response = new AlertData()
        {
            Amount = parsedForecasts.Take(24).Sum(f => f.TotalPrecipitationAmount).Convert(UnitsConverter.Units.Millimeters, UnitsConverter.Units.Inches),
            Probability = parsedForecasts.Take(24).Max(f => f.PercentOfPrecipitation),
        };
        if (response.Probability > .4m && response.Amount > 0.4m)
        {
            response.IsAlert = true;
            response.Message = "Manure Warning";
        }
        return response;
    }

    public static AlertData ManureViolation(IEnumerable<OpenWeatherMapHourlyData> parsedForecasts)
    {
        var response = new AlertData()
        {
            Amount = parsedForecasts.Take(24).Sum(f => f.TotalPrecipitationAmount).Convert(UnitsConverter.Units.Millimeters, UnitsConverter.Units.Inches),
            Probability = parsedForecasts.Take(24).Max(f => f.PercentOfPrecipitation),
        };
        if (response.Probability > .5m && response.Amount > 0.5m)
        {
            response.IsAlert = true;
            response.Message = "Manure Violation";
        }
        return response;
    }

    public static AlertData FertilizerViolation(IEnumerable<OpenWeatherMapHourlyData> parsedForecasts)
    {
        var response = new AlertData()
        {
            Amount = parsedForecasts.Take(12).Sum(f => f.TotalPrecipitationAmount).Convert(UnitsConverter.Units.Millimeters, UnitsConverter.Units.Inches),
            Probability = parsedForecasts.Take(12).Max(f => f.PercentOfPrecipitation),
        };
        if (response.Probability > .6m && response.Amount > 1m)
        {
            response.IsAlert = true;
            response.Message = "Fertilizer Violation";
        }
        return response;
    }
    public class Alert
    {
        public AlertData Fertilizer { get; set; }
        public AlertData Manure { get; set; }
    }
    public class AlertData
    {
        public string Message { get; set; }
        public decimal Probability { get; set; }
        public decimal Amount { get; set; }
        public bool IsAlert { get; set; }
    }
}
public class OpenWeatherMapHourlyData
{
    [JsonPropertyName("rain")]
    public OpenWeatherMapPrecipData? Rain { get; set; }

    public decimal TotalPrecipitationAmount => Rain?.OneHourAmount ?? 0 + Snow?.OneHourAmount ?? 0;

    [JsonPropertyName("snow")]
    public OpenWeatherMapPrecipData? Snow { get; set; }

    [JsonPropertyName("pop")]
    public decimal PercentOfPrecipitation { get; set; }
}
public class OpenWeatherMapPrecipData
{
    [JsonPropertyName("1h")]
    public decimal OneHourAmount { get; set; }
}
public static class UnitsConverter
{
    public enum Units
    {
        Inches,
        Millimeters
    }
    static readonly Dictionary<(Units, Units), decimal> conversions = new Dictionary<(Units, Units), decimal>
      {
          {(Units.Millimeters,Units.Inches),1/25.4m},
          {(Units.Inches,Units.Millimeters),25.4m}
      };
    public static decimal Convert(this decimal value, Units input, Units desired)
    {
        if (conversions.ContainsKey((input, desired)))
            return conversions[(input, desired)] * value;
        throw new InvalidCastException();
    }
}
