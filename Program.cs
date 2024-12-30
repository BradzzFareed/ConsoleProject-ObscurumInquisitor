namespace ConsoleProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GameMenu consoleProject = new GameMenu();
                consoleProject.StartTitle();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
    }
}