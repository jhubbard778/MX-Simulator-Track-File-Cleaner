namespace TrackFileCleaner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UserInterface.PromptEnter();

            string[] dirs = Directory.GetDirectories(Environment.CurrentDirectory, "*", SearchOption.TopDirectoryOnly);

            foreach (string dir in dirs)
            {
                Console.WriteLine(dir);
            }

        }
    }
}