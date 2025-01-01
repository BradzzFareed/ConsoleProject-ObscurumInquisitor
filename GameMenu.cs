using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Media;

namespace ConsoleProject
{
    internal class GameMenu
    {
        public void StartTitle()
        {
            Console.Title = "Obscurum Inquisitor";
            RunMainMenu();
        }
        public void RunMainMenu()
        {
            string prompt = @"
                                             _____  _                                                 _____                       _       _  _                
                                            |  _  || |                                               |_   _|                     (_)     (_)| |               
                                            | | | || |__   ___   ___  _   _  _ __  _   _  _ __ ___     | |   _ __    __ _  _   _  _  ___  _ | |_   ___   _ __ 
                                            | | | || '_ \ / __| / __|| | | || '__|| | | || '_ ` _ \    | |  | '_ \  / _` || | | || |/ __|| || __| / _ \ | '__|
                                            \ \_/ /| |_) |\__ \| (__ | |_| || |   | |_| || | | | | |  _| |_ | | | || (_| || |_| || |\__ \| || |_ | (_) || |   
                                             \___/ |_.__/ |___/ \___| \__,_||_|    \__,_||_| |_| |_|  \___/ |_| |_| \__, | \__,_||_||___/|_| \__| \___/ |_|   
                                                                                                                       | |                                    
                                                                                                                       |_|                                    
                                                                           (Use arrow keys to navigate and Enter to select) ";
            string[] options = { "Start Game", "About", "Exit" };
            MenuControls mainMenu = new MenuControls(prompt, options);
            int SelectedIndex = mainMenu.Run();
            switch (SelectedIndex)
            {
                case 0:
                    StartGame();
                    break;
                case 1:
                    DisplayAboutInfo();
                    break;
                case 2:
                    ExitGame();
                    break;
                default:
                    throw new ArgumentException("Invalid Choice. Please select from \"Start Game\", \"About\", \"Exit\" only)");
            }
        }
        public void ExitGame()
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
            Environment.Exit(0);
        }
        private void DisplayAboutInfo()
        {
            Console.Clear();
            Console.WriteLine("The game is still under development. Please wait for announcements regarding official release!");
            Console.ReadKey(true);
            RunMainMenu();
        }
        private void StartGame()
        {
            Game startGame = new Game();
            startGame.StartGame();
        }
        public static void TypingAnimation(string typing, bool waitForKey = true)
        {
            foreach (char letter in typing)
            {
                Console.Write(letter);
                Thread.Sleep(1);
            }
            if (waitForKey)
            {
                Console.ReadKey(intercept: true);
            }
        }
        public static void CreateBox(string textInside, int boxWidth)
        {
            boxWidth = textInside.Length;
            boxWidth -= 2;

            string spaces = "";
            while (boxWidth != 0)
            {
                spaces += " ";
                boxWidth--;
                continue;
            }
            Console.WriteLine($"|{textInside}{spaces}|");
        }
        public static void GameMusic(string filepath)
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = filepath;
            player.Play();
        }
    }
}