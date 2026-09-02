using InfrastructureLayerDeepDive.Weather.Application.Models;
using InfrastructureLayerDeepDive.Weather.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayerDeepDive.Weather.Application.Mapper
{
    public static class WeatherMapper
    {
        public static WeatherEntity ToEntity(this WeatherModel weather)
        {
            return new WeatherEntity
            {
                Date = weather.Date,
                Summary = weather.Summary,
                TemperatureC = weather.TemperatureC,
            };
        }
    }
}
