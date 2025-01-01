using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleProject
{
    class MenuControls
    {
        private int SelectedIndex;
        private string[] Options;
        private string Prompt;
        public MenuControls(string prompt, string[] options)
        {
            Prompt = prompt;
            Options = options;
            SelectedIndex = 0;
        }
        private void DisplayOptions()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Prompt);
            Console.ResetColor();
            for (int i = 0; i < Options.Length; i++)
            {
                try
                {
                    string currentOption = Options[i];
                    string prefix = "";
                    if (i == SelectedIndex)
                    {
                        prefix = "*";
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    }
                    else
                    {
                        prefix = " ";
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    CenterText($"{prefix}|{currentOption}|");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.ResetColor();
        }
        public int Run()
        {
            ConsoleKey keyPressed;
            do
            {
                Console.Clear(); // Clears the console
                DisplayOptions();
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;

                //arrow key controls
                if (keyPressed == ConsoleKey.UpArrow)
                {
                    SelectedIndex--;
                    if (SelectedIndex == -1)
                    {
                        SelectedIndex = Options.Length - 1;
                    }
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    SelectedIndex++;
                    if (SelectedIndex == Options.Length)
                    {
                        SelectedIndex = 0;
                    }
                }
            }
            while (keyPressed != ConsoleKey.Enter);
            return SelectedIndex;
        }
        public static void CenterText(string text)
        {
            int windowWidth = Console.WindowWidth;
            int padding = (windowWidth - text.Length) / 2;
            Console.WriteLine(new string (' ', padding) + text);
        }
        public static void ShowChoices(string[] options)
        {
            int selectionCounter = 1;
            foreach (string option in options)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"\r\n{selectionCounter++}. {option}");
            }
            Console.Write("Choice: ");
        }
    }
}