namespace MqttServerExtension.Extensions
{
    public static class Int
    {
        public static string ToMinLengthString(this int integer, int minLength)
        {
            var result = integer.ToString();

            while (result.Length < minLength)
                result = "0" + result;

            return result;
        }
    }
}
