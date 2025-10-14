namespace BE.REPOs.Service
{
    public class OpenAIOptions
    {
        public string Model { get; set; } = "gpt-oss-20b";
        public int DefaultMaxTokens { get; set; } = 1024;
        public double DefaultTemperature { get; set; } = 0.3;
    }
}


