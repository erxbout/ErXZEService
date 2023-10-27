using ErXZEService.Services;
using ErXZEService.Services.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ErXZEService.Converter
{
    public interface IIndivFieldConverter
    {
        string SerializedPropertyName { get; set; }
        string PropertyName { get; set; }
        object GetConvertedValue(string value);
    }

    public class IndivFieldsConverter<T> : IIndivFieldConverter
    {
        public string SerializedPropertyName { get; set; }
        public string PropertyName { get; set; }

        public object GetConvertedValue(string value)
        {
            var logger = IoC.Resolve<ILogger>();

            try
            {
                if (typeof(T) == typeof(DateTime))
                {
                    List<string> formats = new List<string>() { "dd.MM.yyyy", "dd.MM.yyyy HH:mm:ss", "d.MM.yyyy", "d.M.yyyy", "dd.M.yyyy" };
                    foreach (string format in formats.Where(x => x.Length == value.Length))
                    {
                        try
                        {
                            string convertValue = value;
                            string[] splitted = value.Split(' ');

                            if (splitted.Length == 2)
                                convertValue = splitted[0] + " " + splitted[1].Replace(".", ":");

                            object dateResult = DateTime.ParseExact(convertValue, format, CultureInfo.InvariantCulture);
                            return dateResult;
                        }
                        catch (Exception e)
                        {
                            logger?.LogError($"Cannot convert value: {value} with format {format}", e);
                        }
                    }
                }

                T result = (T)Convert.ChangeType(value, typeof(T));

                if (typeof(T) == typeof(decimal))
                {
                    var res = result as decimal?;

                    if (res != null)
                        result = (T)Convert.ChangeType(res * 0.1m, typeof(T));
                }

                return result;
            }
            catch (Exception e)
            {
                logger?.LogError($"Error changing value {value} to type '{typeof(T)}'", e);
            }

            return null;
        }
    }
}
