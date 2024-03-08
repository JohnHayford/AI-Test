public class CalculatedWeatherResult
{
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
        }; if (response.Probability > .4m && response.Amount > 0.4m)
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
        }; if (response.Probability > .5m && response.Amount > 0.5m)
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
        }; if (response.Probability > .6m && response.Amount > 1m)
        {
            response.IsAlert = true;
            response.Message = "Fertilizer Violation";
        }
        return response;
    }
    public class Alert
    {
        public Alert()
        {
        }

        public AlertData Fertilizer { get; set; }
        public AlertData Manure { get; set; }
    }
    public class AlertData
    {
        public AlertData() { }
        public string Message { get; set; }
        public decimal Probability { get; set; }
        public decimal Amount { get; set; }
        public bool IsAlert { get; set; }
    }
}
  public class OpenWeatherMapHourlyData
  {
      [JsonPropertyName("clouds")]
      public decimal Cloudiness { get; set; }

      [JsonPropertyName("dew_point")]
      public decimal DewPoint { get; set; }

      [JsonPropertyName("feels_like")]
      public decimal FeelsLike { get; set; }

      [JsonPropertyName("humidity")]
      public decimal Humidity { get; set; }

      [JsonPropertyName("pressure")]
      public decimal Pressure { get; set; }

      [JsonPropertyName("rain")]
      public OpenWeatherMapPrecipData? Rain { get; set; }

      public decimal TotalPrecipitationAmount => Rain?.OneHourAmount ?? 0 + Snow?.OneHourAmount ?? 0;

      [JsonPropertyName("snow")]
      public OpenWeatherMapPrecipData? Snow { get; set; }

      [JsonPropertyName("temp")]
      public decimal Temperature { get; set; }

      [JsonPropertyName("dt")]
      public long TimeUTC { get; set; }

      [JsonPropertyName("uvi")]
      public decimal UVIndex { get; set; }

      [JsonPropertyName("visibility")]
      public decimal Visibility { get; set; }

      [JsonPropertyName("weather")]
      public IEnumerable<OpenWeatherMapWeatherDescription> Weather { get; set; }

      [JsonPropertyName("wind_deg")]
      public decimal WindDirectionDegrees { get; set; }

      [JsonPropertyName("wind_gust")]
      public decimal? WindGust { get; set; }

      [JsonPropertyName("wind_speed")]
      public decimal? WindSpeed { get; set; }

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
