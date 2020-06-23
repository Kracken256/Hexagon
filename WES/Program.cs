using System;

namespace WES_Main
{
    class Program
    {
        private static bool programStatus = true;
        private static Exception lastError;
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Init();
            while (programStatus)
            {
                try
                {
                    Console.Write("> ");
                    string command = Console.ReadLine();

                    if (command != "" && !ExecuteInnerCommand(command))
                    {
                        string output = await WES.ExecuteAsync(command);
                        if (output != "")
                        {
                            Console.WriteLine(output);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    lastError = ex;
                }
            }
        }
        static void Init()
        {
            Console.Title = "WES HTTP Client";
            Console.Clear();
            Console.WriteLine("Welcome to WES HTTP Client");
            Console.WriteLine("Type 'help' for a list of commands");
            Console.WriteLine();
        }
        static bool ExecuteInnerCommand(string command)
        {
            bool isInnerCommand = false;
            string newCommand = command.Trim().Substring(1).Trim();
            if (newCommand.StartsWith("c"))
            {
                Console.Clear();
                isInnerCommand = true;
            }
            else if (newCommand.StartsWith("err"))
            {
                Console.WriteLine(lastError.ToString());
                isInnerCommand = true;
            }
            else if (newCommand.StartsWith("re"))
            {
                Init();
                isInnerCommand = true;
            }
            else if (newCommand.StartsWith("ex"))
            {
                Environment.Exit(0);
                isInnerCommand = true;
            }
            return isInnerCommand;
        }
    }
}
