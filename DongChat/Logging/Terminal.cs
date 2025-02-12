namespace Common.Logging
{
    public enum Color
    {
        Red,
        Green,
        Blue,
        Yellow
    }
    public static class Terminal
    {
        public static void Info(string message) => Write(Color.Blue, message);
        public static void Error(string message) => Write(Color.Red, message);
        public static void Warning(string message) => Write(Color.Yellow, message);
        public static void Success(string message) => Write(Color.Green, message);


        private static object syncObject = new object();


        public static void Write(Color color, string message) {
            string colorCode = color switch
            {
                Color.Red => "\031",
                Color.Green => "\032",
                Color.Blue => "\034",
                Color.Yellow => "\033",
                _ => string.Empty
            };

            string timeString = DateTime.Now.ToString("HH:mm:ss");
            string threadName = Thread.CurrentThread.Name ?? "unknown";

            Task.Factory.StartNew(() => {
                lock (syncObject)
                    File.WriteAllText("log.txt", $"{timeString} [{threadName}] {message}\n");
                
            }, TaskCreationOptions.PreferFairness);

            // write to log here.
            Console.WriteLine($"{colorCode}{timeString} [{threadName}] {message}");
        }
    }
}
