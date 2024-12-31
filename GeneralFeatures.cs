using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.IO;
using System.IO.Pipes;
using System.Reflection;

namespace ConsoleProject
{
    public class Character
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpecialSkill { get; set; }
        public Character(string name, int health, int attack, int defense, int specialSkill)
        {
            Name = name;
            Health = health;
            MaxHealth = health;
            Attack = attack;
            Defense = defense;
            SpecialSkill = specialSkill;
        }
    }
    public class GameState
    {
        public Character Player { get; set; }
        public int CurrentChapter { get; set; }
        public Dictionary<String, bool> Flags { get; set; }
        public List<String> CompletedEvents { get; set; } = new List<String>(); // to store completed chapters
        public GameState()
        {
            Flags = new Dictionary<string, bool>();
        }
    }
    public class Game
    {
        private GameState currentState;
        private readonly Dictionary<string, Character> enemies;
        private const string SAVE_FILE_PATH = "save_game.json";

        public Game()
        {
            InitializeGame();
            enemies = InitializeEnemies();
        }
        private Dictionary<string, Character> InitializeEnemies()
        {
            return new Dictionary<string, Character>
            {
                {"Cultists", new Character("Cultists", 100, 15, 2, 20)},
                {"Plagued Villager", new Character("Plagued Villager", 115, 18, 4, 21)},
                {"Shade", new Character("Shade", 120, 21, 5, 22)},
                {"Ghoul", new Character("Ghoul", 150, 23, 10, 25)},
                {"Shadow Fiend", new Character("Shadow Fiend", 80, 48, 25, 60)},
                {"Dreadlord", new Character("Dreadlord", 250, 35, 18, 55)}
            };
        }
        private void InitializeGame()
        {
            currentState = new GameState
            {
                Player = new Character("Detective McGilis \"Zero\" Rosenberger", 200, 30, 4, 50),
                CurrentChapter = 1
            };
        }
        public void StartGame()
        {
            while (true)
            {
                try
                {
                    ShowStartMenu();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }
        private void ShowStartMenu()
        {
            Console.Clear();
            MenuControls.CenterText("Start Options");
            string[] options = { "1. New Game", "2. Load Game", "Back" };
            MenuControls startMenu = new MenuControls("Select an Action.", options);
            int SelectedIndex = startMenu.Run();
            switch (SelectedIndex)
            {
                case 0:
                    PlayGame();
                    break;
                case 1:
                    LoadGame();
                    break;
                case 2:
                    GameMenu mainMenu = new GameMenu();
                    mainMenu.RunMainMenu();
                    break;
                default:
                    throw new ArgumentException("Invalid Choice. Please select from the following options only.");
            }
        }
        private void PlayGame()
        {
            while (true)
            {
                try
                {
                    ShowGameInterface();
                    Console.WriteLine("\nCommands: [S]ave, [P]lay, [Q]uit");
                    goback:
                    char input = Convert.ToChar(Console.ReadLine().ToUpper());
                    switch (input)
                    {
                        case 'S':
                            SaveGame();
                            break;
                        case 'P':
                            Console.Clear();
                            ProgressStory();
                            break;
                        case 'Q':
                            return;
                        default:
                            goto goback;
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Invalid Input. Please enter the shown characters only.");
                }
            }
        }
        private void ShowGameInterface()
        {
            Console.Clear();
            Console.WriteLine($"Chapter: {currentState.CurrentChapter}");
            Console.WriteLine($"Health: {currentState.Player.Health}/{currentState.Player.MaxHealth}");
            GameMenu.CreateBox("What would you like to do?", 5);
        }        
        private void InitiateCombat(Character enemy)
        {
            Console.Clear();
            GameMenu.CreateBox($"A {enemy.Name} appears before you!", 5);
            Thread.Sleep(1000);

            while (enemy.Health > 0 && currentState.Player.Health > 0)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine($"\nPlayer HP: {currentState.Player.Health}/{currentState.Player.MaxHealth}");
                    Console.WriteLine($"{enemy.Name} HP: {enemy.Health}/{enemy.MaxHealth}");
                    string[] moves = { "[A]ttack", "[D]efend", "[S]pecial Skill" };
                    foreach (string move in moves)
                    {
                        MenuControls.CenterText(move);
                    }
                    
                    string selectedMove = Console.ReadLine().ToUpper();
                    int damage;
                    switch (selectedMove)
                    {
                        case "A":
                            damage = Math.Max(0, currentState.Player.Attack - enemy.Defense);
                            enemy.Health -= damage;
                            GameMenu.CreateBox($"You dealt {damage} to the {enemy.Name}", 5);
                            break;
                        case "D":
                            currentState.Player.Defense *= 2;
                            Console.WriteLine("You braced against the incoming attack", 5);
                            break;
                        case "S":
                            if (Random.Shared.Next(2) == 0)
                            {
                                damage = Math.Max(0, currentState.Player.SpecialSkill - enemy.Defense);
                                enemy.Health -= damage;
                                GameMenu.CreateBox("You dealt massive damage!", 5);
                            }
                            GameMenu.CreateBox("Skill Missed!", 5);
                            break;
                        default:
                            throw new ArgumentException("Invalid Combat Choice!");
                    }
                    
                    // enemy turn
                    if (enemy.Health < 0.3 * enemy.Health)
                    {
                        if (Random.Shared.Next(2) == 0)
                        {
                            damage = Math.Max(0, enemy.SpecialSkill - currentState.Player.Defense);
                            currentState.Player.Health -= damage;
                            GameMenu.CreateBox($"{enemy.Name} has used its special skill!", 5);
                            GameMenu.CreateBox($"{enemy.Name} has dealt a massive {damage} damage!", 5);
                        }
                        GameMenu.CreateBox($"{enemy.Name} attack missed!", 5);
                    }
                    else
                    {
                        damage = Math.Max(0, enemy.Attack - currentState.Player.Defense);
                        currentState.Player.Health -= damage;
                        GameMenu.CreateBox($"{enemy.Name} has dealt {damage} damage!", 5);
                    }
                    if (currentState.Player.Defense > 10)
                    {
                        currentState.Player.Defense = 10;
                    }
                    Thread.Sleep(1000);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            if (currentState.Player.Health <= 0)
            {
                GameMenu.CreateBox("Game Over!", 2);
                Thread.Sleep(2000);
                InitializeGame();
            }
            else
            {
                GameMenu.CreateBox($"You have defeated the {enemy.Name}", 2);
                Thread.Sleep(2000);
            }
        }
        private void SaveGame()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(currentState);
                File.WriteAllText(SAVE_FILE_PATH, jsonString);
                GameMenu.CreateBox("Successfully Saved Game!", 5);
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving game: {ex.Message}");
            }
        }
        private void LoadGame()
        {
            try
            {
                if (!File.Exists(SAVE_FILE_PATH))
                {
                    throw new FileNotFoundException("No save file found.");
                }
                string jsonString = File.ReadAllText(SAVE_FILE_PATH);
                currentState = JsonSerializer.Deserialize<GameState>(jsonString);
                GameMenu.CreateBox("Game Loaded Successfully", 33);
                Thread.Sleep(1000);
                PlayGame();
            }
            catch (Exception ex)
            {
                throw new Exception($"Problem encountered loading game: {ex.Message}");
            }
        }
        private void ProgressStory()
        {
            switch (currentState.CurrentChapter)
            {
                case 1:
                    ChapterOne();
                    break;
                case 2:
                    ChapterOnePointOne();
                    break;
                case 3:
                    ChapterOnePointTwo();
                    break;
                case 4:
                    ChapterTwo();
                    break;
                case 5:
                    ChapterTwoPointOne();
                    break;
                case 6:
                    ChapterTwoPointTwo();
                    break;
                case 7:
                    ChapterTwoPointThree();
                    break;
                case 8:
                    ChapterTwoPointFour();
                    break;
                case 9:
                    ChapterThree();
                    break;
                case 10:
                    ChapterThreePointOne();
                    break;
                default:
                    GameMenu.CreateBox("You have reached the end of the current story!", 2);
                    Thread.Sleep(2000);
                    return;
            }
        }
        private void ChapterOne()
        {
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_1_started"))
                    {
                        currentState.Flags["chapter_1_started"] = true;

                        GameMenu.TypingAnimation("Chapter One: The Case of Fanfoss");
                        GameMenu.TypingAnimation("\r\n*You entered the office of the director*");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDirector: Ah detective you're finally here, I've been waiting for you. Come, have a seat we have much to talk about.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\n*You took a seat*\r\n");
                        string[] options = { "Hello Director, another treasure hunt I presume?", "Greetings Director, another missing person again I'm guessing?", "What do you need for me to accomplish this time boss? Review a file case perhaps?" };
                        MenuControls.ShowChoices(options);
                        GameMenu.TypingAnimation("\r\nChoice: ");

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                            char choice = keyInfo.KeyChar;
                            if (choice == '1' || choice == '2' || choice == '3')
                            {
                                Console.ResetColor();
                                Console.Clear();
                                string detectiveDialogue = choice switch
                                {
                                    '1' => "Detective Zero: Hello Director, another treasure hunt I presume?",
                                    '2' => "Detective Zero: Greetings Director, another missing person again I'm guessing?",
                                    '3' => "Detective Zero: What do you need for me to accomplish this time boss? Review a file case perhaps?",
                                    _=> throw new InvalidOperationException("Invalid choice detected.")
                                };
                                GameMenu.TypingAnimation(detectiveDialogue);
                                break;
                            }
                            else
                            {
                                GameMenu.TypingAnimation("\r\nInvalid choice. Please choose from the provided options.");
                            }
                        }
                        Thread.Sleep(2000);
                    }
                    else if (!currentState.Flags.ContainsKey("conversation_with_director"))
                    {
                        currentState.Flags["conversation_with_director"] = true;

                        GameMenu.TypingAnimation("\r\nDirector: Its a different one and its rather interesting. This kind of request is the first of its kind and it is something quite unusual.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: Hmm how intriguing, the first of its kind you say? what might this unusual case be director?");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDirector:\"Shadow Hunting\". That was the name of the request submitted to the department.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: Huh that is indeed unusual. Is there a chance that this is just some wild goose chase director?");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDirector: Bizarre as it sounds, we cannot simply label it as some wild goose chase. According to this memo, there seems to be some kind of anomaly looming over the village. A great number of people have reportedly gone missing or dead all in the span of one night and some witnesses say some bodies look either mutilated or drained, as if the soul was forcefully removed from the body...");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: Sweet mother of mercy...");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: Seems quite late for a halloween celebration innit? Are there any clues said by the sender that could aid in narrowing the cause of all this?");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDirector: Nothing of significance unfortunately. They only said something about a weird shadow surrounding the dead victims before recovering their corpses.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: Ohh I see...I have no idea what I'm supposed to make of that. Are you sure this isn't a prank director?");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDirector: Believe me I hope it was a prank. Regardless, I'm going to need you to deploy to the town to investigate. If what the towspeople are saying are true then we cannot ignore the unease and casualties that are happening.");
                        string[] options = { "Express Frustration", "Laugh at the adsurdity" };
                        MenuControls.ShowChoices(options);
                        GameMenu.TypingAnimation("\r\nChoice: ");

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                            char choice = keyInfo.KeyChar;
                            if (choice == '1' || choice == '2' || choice == '3')
                            {
                                Console.ResetColor();
                                Console.Clear();
                                string detectiveDialogue = choice switch
                                {
                                    '1' => "Detective Zero: *Expresses Frustration*\r\nVery well then director. If you insist that I investigate this case then I shall do so with haste.",
                                    '2' => "Detective Zero: *Laughs at the absurdity*\r\nEver the serious type eh director MWAHAHAHAHA. Do not worry, I will make quick work of these silly antics.",
                                    _ => throw new InvalidOperationException("Invalid choice detected.")
                                };
                                GameMenu.TypingAnimation(detectiveDialogue);
                                break;
                            }
                            else
                            {
                                GameMenu.TypingAnimation("\r\nInvalid choice. Please choose from the provided options.");
                            }
                        }
                        GameMenu.TypingAnimation("\r\nDirector: To be frank detective, I do not expect you to actually catch a shadow...I don't even think that's possible. What matters the most is to investigate who or what is causing this malevolence in the village.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: You have my word director. So do I have anything to start with aside from the clue you gave earlier?");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDirector: As much as I want to provide more clues detective, I am afraid that is all we currently have at the moment.");
                        string[] options2 = { "*Sigh * I guess I'll make do of what I have for now", "Great...just great. (sarcastically said)" };
                        MenuControls.ShowChoices(options2);
                        GameMenu.TypingAnimation("\r\nChoice: ");

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                            char choice = keyInfo.KeyChar;
                            if (choice == '1' || choice == '2' || choice == '3')
                            {
                                Console.ResetColor();
                                Console.Clear();
                                string detectiveDialogue = choice switch
                                {
                                    '1' => "Detective Zero: *Sigh* I guess I'll make do of what I have for now",
                                    '2' => "Detective Zero: Great...just great. (sarcastically said)",
                                    _ => throw new InvalidOperationException("Invalid choice detected.")
                                };
                                GameMenu.TypingAnimation(detectiveDialogue);
                                break;
                            }
                            else
                            {
                                GameMenu.TypingAnimation("\r\nInvalid choice. Please choose from the provided options.");
                            }
                        }

                        GameMenu.TypingAnimation("\r\nDirector: I guess it is settled then. Go to the village of Fanfoss and meet with the local church's head priest. He was the one that enlisted our aid and asking him questions would be a good start. The priest provided the directions to the town, I will send it to you the later.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: Many thanks director. I will leave for the town at dawn.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDirector: Godspeed Detective McGilis \"Zero\" Rosenberger. Do not hesitate to call should the situation escalate to a dangerous degree.");
                        Console.ReadKey(intercept: true);
                        string[] options3 = { "Express Confidence", "Consider the Director" };
                        MenuControls.ShowChoices(options3);
                        GameMenu.TypingAnimation("\r\nChoice: ");

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                            char choice = keyInfo.KeyChar;
                            switch (choice)
                            {
                                case '1':
                                    currentState.Flags["option_1_chosen"] = true;
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("Detective Zero: Baaaaah you worry too much director. There is no need to call for I intend to solve this case as fast as possible.");
                                    GameMenu.TypingAnimation("\r\n*Proceeds to exit the office*");
                                    break;
                                case '2':
                                    currentState.Flags["director_helps"] = true;
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("Detective Zero: I'll keep that in mind director. I'll make sure to immediately contact the department if things go south over there.");
                                    GameMenu.TypingAnimation("\r\n*Proceeds to exit the office*");
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice or already selected option. Please choose again.");
                                    continue;
                            }
                            Thread.Sleep(2000);
                            break;
                        }

                    }
                    else if (!currentState.Flags.ContainsKey("chapter_1_complete"))
                    {
                        GameMenu.TypingAnimation("\r\n*You now make your way towards the town of Fanfoss*");
                        Thread.Sleep(2000);

                        currentState.Flags["conversation_with_director"] = true;
                        currentState.CompletedEvents.Add("ChapterOne_Completed");
                        currentState.CurrentChapter++;

                        Console.Clear();
                        Console.WriteLine("Chapter One Complete! Your choices will affect future events...");
                        Console.WriteLine("Do you wish to go to the next chapter? (y) if yes or (n) if no");
                        while (true)
                        {
                            char decision1 = Convert.ToChar(Console.ReadLine().ToLower());
                            if (decision1 == 'y')
                            {
                                Console.Clear();
                                ChapterOnePointOne();
                                break;
                            }
                            else if (decision1 == 'n')
                            {
                                SaveGame();
                                PlayGame();
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Select 'y' or 'n' only");
                            }
                        }
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                    Console.WriteLine("\nInvalid Input. Please only type in what is presented in choices.");
                }
            }
        }
        private void ChapterOnePointOne()
        {
            try
            {
                if (!currentState.Flags.ContainsKey("chapter_1.1_started"))
                {
                    currentState.Flags["chapter_1.1_started"] = true;
                    GameMenu.TypingAnimation("Chapter 1.1: The Village");
                    GameMenu.TypingAnimation("\r\n*You now make your way to the village*");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\n*Phone Ringing*");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\n*You checked your phone*");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nDetective Zero: Ahhhh so this is the village of Fanfoss huh? Strange...Its name does not show up when I try to search it online.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nDriver: Sir, we are now here.");
                    Console.ReadKey(intercept: true);
                    Console.Clear();
                    GameMenu.TypingAnimation("\r\nDetective Zero: Oh thank you kind sir. Here is a little extra for the smooth ride.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\n*You got out of the taxi*");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nDetective Zero: *shivers* This place gives me the creeps, there is barely any sunlight here. I should check the town first before I go to the church.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\n*You roam around the village and see no one. The village has a dim light and many tall trees.*");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nDetective Zero: HELLO?!?!...IS ANYONE HERE?!?...my lord where are all the townspeople?. Hold on...is this actually a prank the Director planned? Very funny, It ain't even my birthday yet.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\n*Tree branches breaking sounds came from behind*");
                    Console.ReadKey(intercept: true);
                    Console.Clear();
                    GameMenu.TypingAnimation("\r\nDetective Zero: WHO GOES THERE!? SHOW YOURSELF!");
                    Console.ReadKey(intercept: true);
                    string[] options = { "Investigate", "Keep going" };
                    MenuControls.ShowChoices(options);
                    again:

                    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                    char choice = keyInfo.KeyChar;
                    switch (choice)
                    {
                        case '1':
                            Console.ResetColor();
                            Console.Clear();
                            GameMenu.TypingAnimation("\r\nYou went closer to the tree behind you to investigate the source of those sounds.");
                            GameMenu.TypingAnimation("\r\nDetective Zero: FREEZE!");
                            Console.ReadKey(intercept: true);
                            GameMenu.TypingAnimation("\r\n*A man suddenly disarms you and covers your mouth*");
                            Console.ReadKey(intercept: true);
                            GameMenu.TypingAnimation("\r\nMan: Shhhhh quiet down, they will hear you.");
                            Console.ReadKey(intercept: true);
                            GameMenu.TypingAnimation("\r\nDetective Zero: *You made muffled noises* \"I'M GONNA BEAT THIS GUY TO A PULP WHEN I GET FREE!\" you said in your mind.");
                            Console.ReadKey(intercept: true);
                            GameMenu.TypingAnimation("\r\nMan: Don't worry, I'm a good guy I swear so keep your mouth shut if you don't want to get in trouble.");
                            Console.ReadKey(intercept: true);
                            break;
                        case '2':
                            Console.ResetColor();
                            Console.Clear();
                            GameMenu.TypingAnimation("\r\nDetective Zero: Hmm maybe it was just an animal passing by.");
                            Console.ReadKey(intercept: true);
                            GameMenu.TypingAnimation("\r\n*You kept on walking until a man suddenly grabs you from behind and covered your mouth.");
                            Console.ReadKey(intercept: true);
                            GameMenu.TypingAnimation("\r\nMan: Walking alone at this hour? Are you nuts!? Now shhhh before they hear you.");
                            Console.ReadKey(intercept: true);
                            GameMenu.TypingAnimation("\r\nDetective Zero: \"WHO THE HELL IS THIS GUY? IMMA BEAT THE LIVING CRAP OUTTA HIM!\" you said in your thoughts.");
                            Console.ReadKey(intercept: true);
                            GameMenu.TypingAnimation("\r\nMan: I'm a good guy so keep quiet before we get in trouble.");
                            Console.ReadKey(intercept: true);
                            break;
                        default:
                            goto again;
                    }
                    string[] options2 = { "Stay Calm and follow the man's advise.", "Break free from the man's hold and recover your gun." };
                    MenuControls.ShowChoices(options2);
                    again2:

                    ConsoleKeyInfo keyInfo2 = Console.ReadKey(intercept: true);
                    char choice2 = keyInfo2.KeyChar;
                    switch (choice2)
                    {
                        case '1':
                            Console.Clear();
                            GameMenu.TypingAnimation("*You stayed calm and obeyed the man*");
                            break;
                        case '2':
                            Console.Clear();
                            GameMenu.TypingAnimation("*You broke free and quickly recovered your gun*");
                            currentState.Flags["game_over_route"] = true;
                            break;
                        default:
                            goto again2;
                    }
                    Thread.Sleep(2000);
                }

                else if (!currentState.Flags.ContainsKey("met_arthur"))
                {
                    GameMenu.TypingAnimation("*You followed through your decision and hoped it was the wise choice*");
                    Thread.Sleep(1000);

                    if (currentState.Flags.ContainsKey("game_over_route"))
                    {
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nDetective Zero: I'm gonna have to get your name young man and you got some questions I need answered.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\n*A single blink of your eye and the color of the forest changed to a dark and creepy one.*");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nUnknown Voice: C0me mY C#%ld");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: WHO GOES THERE!?");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nUnknown Voice: C’[me cl02ser my 3hi1d");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\n*No matter where you looked, the weird noises just keep getting louder, till…*");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: WHAT’S HAPPENING?!?!");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\n*You see these shadow hands and eyes everywhere on the floor*");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\n…\r\nYou have been captured by the shadows….try again?");
                        Console.WriteLine("Type 'y' if yes or 'n' if no");
                        replay:
                        char decision2 = Convert.ToChar(Console.ReadLine().ToLower());
                        if (decision2 == 'y')
                        {
                            LoadGame();
                        }
                        else if (decision2 == 'n')
                        {
                            PlayGame();
                        }
                        else
                        {
                            goto replay;
                        } 
                    }
                    else
                    {
                        GameMenu.TypingAnimation("\r\n*The man removed his hand from your mouth*");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nMan: Let’s talk somewhere else, it’s too dangerous here");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\n*You followed the man*");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nMan: My name's Arthur, I’m a villager here. I’m sorry I had to cover your mouth earlier.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: Greetings Arthur, you have my thanks for saving my existence. I’ve been sent here to investigate some anomalies made by \"shadows\". Uhhh may I ask what are those hands you’re talking about are?");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nArthur: You won’t be able to get out of the forest when the \"hands\" get you, we were informed by the chief that the people have been missing in the woods so we named it “hands” because something pulls you there and you die.");
                        Console.ReadKey(intercept: true);
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nArthur: We’ve never actually seen the \"hands\", only the church worker named Erebus did.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nArthur: The only time that we townspeople are able to find the missing people is when they are either dead or their bodies mutilated at dark hidden places.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: Hmm...hidden places you say? \"I guess the director was not joking when he said about the dead bodies being drained of their soul\"");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nArthur: My friend has been caught and he is still missing. It’s been months and I still have no clue where he is. I just hope he’s alive.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: I’m sorry about your friend. It seems that the village has gone through a lot of hard things. I would like to help the village in finding out the cause of these deaths. Arthur, will you aid me? I’m not very familiar with the village so I need some help to navigate in case I accidentally get close to the “hands”.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nArthur: I’ll do anything to find my friend. I will do what I can to give you all the help you need.");
                        Console.ReadKey(intercept: true);
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nDetective Zero: Excellent! So the first thing we need to do is go to the church because my boss told me to meet the priest. He said that he may hold information about the anomaly that is happening here.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nArthur: Okay Mr.Detective. I’ll guide you there.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nArthur: Come to think of it detective, what is your name?");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nDetective Zero: Ahhhh my name is Mcgilis \"Zero\" Rosenberger.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("\r\nArthur: Zero? Why are you called Zero? Not to be rude or anything but I think it does not fit with the rest of your name.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("Detective Zero: Not to worry. This isn't the first that anyone is bothered by my nickname. I got that nickname because of my record in the investigations department. I have zero pending cases because I always managed to solve all of them then as time passed, my colleagues began to call me \"Zero\" and it kind of grew on me.");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("Arthur: Ohhhh then its an honor to work with you Detective \"Zero\".");
                        Console.ReadKey(intercept: true);
                        GameMenu.TypingAnimation("Its a pleasure to work with you as well, Arthur");
                    }
                    currentState.Flags["met_arthur"] = true;
                    currentState.CompletedEvents.Add("Chapter1.1_Completed");
                    currentState.CurrentChapter++;

                    Console.Clear();                   
                    Console.WriteLine("Chapter 1.1 Completed! Your choices will affect future events.");
                    Thread.Sleep(2000);

                    repeat:
                    Console.WriteLine("\r\nDo you wish to go to the next chapter? (y) if yes or (n) if no");
                    char decision3 = Convert.ToChar(Console.ReadLine().ToLower());
                    if (decision3 == 'y')
                    {
                        ChapterOnePointTwo();
                    }
                    else if (decision3 == 'n')
                    {
                        SaveGame();
                        PlayGame();
                    }
                    else
                    {
                        Console.WriteLine("Select 'y' or 'n' only");
                        goto repeat;
                    }
                }
                Thread.Sleep(2000);
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
        private void ChapterOnePointTwo()
        {
            try
            {
                if (!currentState.Flags.ContainsKey("chapter_1.2_started"))
                {
                    currentState.Flags["chapter_1.2_started"] = true;
                    GameMenu.TypingAnimation("Chapter 1.2: The Church");
                    Thread.Sleep(2000);

                    GameMenu.TypingAnimation("\r\n*You have arrived at the church*");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nArthur: I’ll go get the priest sir, just wait here");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\n*You looked around the church and saw no one, just a dim light inside and wooden chairs and a lit candle.*");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\n*10 minutes have passed*");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\n*Arthur came back with a priest and another person*");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nArthur: Sir I have the priest with me and his worker");
                    Console.ReadKey(intercept: true);
                    Console.Clear();
                    GameMenu.TypingAnimation("\r\nPriest Lucius: Good afternoon to you sir, you must be the investigator that the Director sent to investigate what’s happening in the village. Allow me to introduce myself, I’m Priest James, the one and only priest of this village and this is Erebus, a worker in the church that knows a lot about these shadows. He will be helping you to solve this case. Thank you very much for coming all the way here.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nErebus: It’s a pleasure to meet you Mr. Investigator, feel free to ask me any questions");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nDetective Zero: Thank you for welcoming me in this village sir, also you can just call me Zero, that’s my codename.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nErebus: Ah! I see Mr. Zero, I will be accompanying you, please tell me if you want me to do something.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nDetective Zero: Thank you Erebus and Father Lucius, I will do my best to solve this case and help the village.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nPriest Lucius: Thank you very much Mr.Zero. Did the Director tell you what has been happening here?");
                    Console.ReadKey(intercept: true);
                    Console.Clear();
                    GameMenu.TypingAnimation("\r\nDetective Zero: Yes Father");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nPriest Lucius: Then I won’t need to explain it to you again, Erebus can give you more information about it when you have something in mind to ask.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nDetective Zero: Understood Father. Erebus may I ask you some questions before we start investigating?");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nErebus: Of Course sir, what would you like to know?");
                    Console.ReadKey(intercept: true);
                    string[] questions = { "About the hands", "About the shadows", "About the village", "About you", "Thank you Eberus for answering my questions" };

                    bool isSelected = true;
                    while (isSelected)
                    {
                        try
                        {
                            Console.WriteLine("\n\nEnter the number of your desired choice. (type 5 to finish conversation)");
                            MenuControls.ShowChoices(questions);
                            int inquiryChoice = Convert.ToInt32(Console.ReadLine());
                            if (inquiryChoice == 5)
                            {
                                isSelected = false;
                                Console.Clear();
                                Console.ResetColor();
                                GameMenu.TypingAnimation("\r\nOn second thought, I will continue the questioning later once I have an outline of the situation.");
                                Console.ReadKey(intercept: true);
                                continue;
                            }
                            if (inquiryChoice < 1 || inquiryChoice > questions.Length)
                            {
                                GameMenu.TypingAnimation("\r\nErebus: I am afraid that is outside my knowledge detective.");
                                continue;
                            }
                            switch (inquiryChoice)
                            {
                                case 1:
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("\"About the hands\"\r\nThe “hands” are the shadows that grabs you into somewhere no one knows.\r\nApparently people that have been caught by “hands” went missing for days or weeks and even months, \r\nthen we just find those missing people somewhere in the village dead. \r\nWe don’t really see any stab wounds or anything that shows blood. They just look very lifeless… \r\ntheir skin is so pale and they aren’t breathing.");
                                    continue;
                                case 2:
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("\"About the shadows\"\r\nWe see these shadows all the other time except when it's dark. They have a different kind of figure, they dont look like a human. Some have big hands with very sharp nails that almost look like a werewolf. Others have a very thin body with a hole inside its chest. Most of them are eye-shaped and just stare at you. They are found near houses and in the forest.  We don’t really know where they’re coming from. All we know is when we see them, we have to hide or run away, because if we don’t… they will take us away.");
                                    continue;
                                case 3:
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("\"About the village\"\r\nThe village was very lively and bright before, there’s a lot of villagers that lived a normal life. \r\nThere was a big tree in the middle of the forest where everyone got their blessings, \r\nuntil one day… some black vines or something started growing around it and it gave some black smoke around it. \r\nThese black smokes suddenly went to people and those people that got caught disappeared and were never found. \r\nThe villagers abandoned the big tree. Months later, the village lost almost all the light there was, the village almost looked colorless.\r\n");
                                    continue;
                                case 4:
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("\"About you\"\r\nAh! I’m Erebus, a worker in this church. I’m not really from this village, I wasn’t born here. \r\nI was abandoned and lost so I roamed around and found this small village. The priest welcomed me and took me in. \r\nI started working to pay the priest’s kindness to me. My work is mostly roaming around the village and check everyone if they’re alright, \r\nthat’s how I got information about these shadows.");
                                    continue;
                            }
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("\nInvalid Input: Please only enter a valid number.");
                        }
                    }
                    Thread.Sleep(2000);
                    
                }
                else if (!currentState.Flags.ContainsKey("chapter_1.2_completed"))
                {
                    GameMenu.TypingAnimation("\r\nDetective Zero: Regardless, thank you Erebus for your concerns.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nErebus: You’re welcome sir");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nZero: Well then, it’s time to go. Father James, thank you again for welcoming me in the village and having Erebus to help me in this case.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nPriest Lucius: It is alright my child, if it means helping the village then I will do anything in my power.");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\nDetective Zero: Commendable Father, we shall be going now");
                    Console.ReadKey(intercept: true);
                    GameMenu.TypingAnimation("\r\n*You and the others exited the church*");
                    Console.ReadKey(intercept: true);

                    currentState.Flags["chapter_1.2_completed"] = true;
                    currentState.CompletedEvents.Add("Chapter_1.2_Completed");
                    currentState.CurrentChapter++;


                    Console.Clear();
                    GameMenu.TypingAnimation("Chapter 1.2 Completed! Your choices will affect future events...");
                    Thread.Sleep(2000);

                    repeat:
                    Console.WriteLine("\r\nDo you wish to go to the next chapter? (y) if yes or (n) if no");
                    char decision3 = Convert.ToChar(Console.ReadLine().ToLower());
                    if (decision3 == 'y')
                    {
                        SaveGame();
                        ChapterTwo();
                    }
                    else if (decision3 == 'n')
                    {
                        SaveGame();
                        PlayGame();
                    }
                    else
                    {
                        Console.WriteLine("Select 'y' or 'n' only");
                        goto repeat;
                    }
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void ChapterTwo()
        {
            try
            {
                if (currentState.Flags.ContainsKey("chapter_2_started"))
                {
                    currentState.Flags["chapter_2_started"] = true;

                        ConsoleKey menu;
                    do
                    {
                        GameMenu.TypingAnimation("\r\nYou are going to the villagers to ask about their experiences");
                        menu = Console.ReadKey(intercept: true).Key;
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Arthur, can you be the guard outside of the house, tell me if you see a shadow or something suspicious.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : Ok sir, I’ll do that");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Thank you. Erebus, come with me inside, You’re the one with the most experience with these shadows so I’ll need that in interviewing the villagers.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : Alright sir.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou have entered the first house");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nErebus : Good afternoon Ms. Anita, I’m sorry for the intrusion, We would just want to interview you quickly to help \r\nour investigator in finding these shadows that have been harming our village.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita rushed to you and grabbed your shoulders.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita : SIR PLEASE FIND MY BROTHER, IT’S BEEN AGES SINCE I SAW HIM.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Ma’am please calm down, I will try my best to find your brother and bring him back.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita wiped her tears and sat down");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita : I’m sorry sir, it’s just been…really hard for me.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : I understand Ms. Anita, that’s why I’m here to get some information to find your brother.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita : Thank you sir *sobbing*");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Ms. Anita I’m really sorry but I don’t really have much time to stay here for too long so I’ll be asking you the questions right away.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita wiped her tears");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita : You can ask me now sir.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Have you seen a shadow?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nMs. Anita : Just once sir, it was when they took my brother…");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : How did it take your brother?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita : I just saw my little brother going closer to the shadow. When I blinked, he was gone.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Did he leave anything?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita : No sir, there was nothing left behind");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : What time did this happen?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nMs. Anita : It was almost night when I saw the shadow. I was cooking dinner. I kept calling my brother but he wouldn’t respond so \r\nI went to see what he’s doing. But then I saw him going to the shadow, I shouted at him but I was too late.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I see. What did the shadow look like?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita : It was eye-shaped sir, just staring at my brother.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Got that, where did you see it?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita : At the middle of the ceiling sir");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Were there any lights?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nMs. Anita : There were no lights sir because we lost electricity that day, only the moonlight was there.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Understood, thank you Ms. Anita. These will be my questions for now, I’ll come back if I have something I have to ask you. I will find your brother so don’t worry.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMs. Anita : Thank you sir, please get him back.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou left the house");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : You done sir?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Yes, did anything happen?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nArthur : None sir, everything looks normal");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Ok that’s good, let’s move to the next house");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou have reached the second house");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Let’s do what we did earlier again");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : Got it sir.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou knocked at the door");
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nNo one responds");

                        GameMenu.TypingAnimation("\r\n1) Knock again\r\n2) Go in without permission");
                    again9:
                        ConsoleKeyInfo keyInfo9 = Console.ReadKey(intercept: true);
                        char choice9 = keyInfo9.KeyChar;
                        switch (choice9)
                        {
                            case '1':
                                Console.Clear();
                                GameMenu.TypingAnimation("\r\nNo one responds again");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero : Let’s open the door Erebus");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nErebus : Ok sir");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nYou have entered the house and saw a pale man in the corner of the house");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                break;
                            case '2':
                                Console.Clear();
                                GameMenu.TypingAnimation("\r\nYou have entered the house and saw a pale man in the corner of the house");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                break;
                
                            default:
                                goto again9;
                
                        }
                        GameMenu.TypingAnimation("\r\nZero : Sir, are you Ok?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nThe Person didn’t respond");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou went closer");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Sir can you hear me?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nNo respond");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Erebus, why won’t he talk?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nErebus : He is Mark, he lost his wife and daughter from the shadows that’s why he’s like that. \r\nHe saw a lot of things that’s why I figured we should go here but I guess he really can’t talk.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou went in front of Sir Mark");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Sir, I promise we will find your daughter and wife so please don’t give up.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : We should get going sir, it’s almost night.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Let’s go then");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou left a bread beside Mark and left.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : Erebus, what time are shadows often seen?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : Hmmmmmm, from my experience I saw a lot during the full moon. But when it’s not a full moon, \r\nthey mostly show up in the evening when only the moon and stars shine.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I see.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou have arrived at the last house");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : Sir I’ll join Arthur outside since it’s almost night so he would be safe incase something happens.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Ok");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nYou knocked at the door");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nGrandma noise : Who is it?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I’m an Investigator ma’am, just want to ask you some questions regarding these shadows");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nThe grandma opened the door");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nGrandma : Come in sir");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou have entered the house");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nGrandma : Take a seat sir, my name is Quin.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I’m sorry for disturbing your night Mrs. Quin.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nGrandma Quin : It’s alright sir, what can I help you with?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I want to ask you some questions about the shadow ma’am, will that be ok?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nGrandma Quin : Of course go ahead");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Did you see any shadow?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nGrandma Quin : I have… he took my husband");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I’m sorry to hear that, do you know how everything happened?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nGrandma Quin : It was dinner time, I went to call my husband to eat dinner from outside, then I heard some screaming outside, \r\nI knew it was my husband. I rushed where the scream was coming from and I saw him. I saw his soul getting taken away.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : But how, I thought the victim goes to the shadow and disappears");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nGrandma Quin : I don’t think it was a shadow that took my husband's life, there were shadows but there’s some kind of human-shaped shadow.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Huh?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nGrandma Quin : He was saying something");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : What did he say?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nGrandma Quin : He said \"Don’t even think about ruining my plan\" then my husband said \"I’ll kill you -\n");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : SIR I NEED YOUR HELP OUTSIDE, A SHADOW APPEARED");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : WHAT, I’ll just finish what Mrs. Quin is saying");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : SIR WE DON’T HAVE THE TIME WE NEED TO GO");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nArthur : SIR RUN!!");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nA black smoke covered the entire house");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : WHERE ARE YOU GUYS!?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou heard Arthur somewhere");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : SIR");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : WHERE ARE YOU ARTHUR?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nArthur : I DON’T KNOW SIR");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nThe smoke went away");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : what…");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : SIR ARE YOU OK?!");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : *cough cough");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nThe three of you stopped in shock");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : how…");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : No way…");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : …");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Mrs. …Quin?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou saw Mrs. Quin’s body on the floor lifeless");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou rushed and checked if she’s alive");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : MRS. QUIN WAKE UP");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nMrs. Quin didn’t move");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou kept on waking her up");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : Sir pls stop");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : Let’s go give him to the priest for a burial");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : I’ll bring him to the church");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : Please do take good care of her");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus : I will sir");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nErebus went to the church while you and Arthur stayed at the house");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                
                
                    } while (menu != ConsoleKey.Escape);


                }

                
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void ChapterTwoPointOne()
        {
            try
            {
                if (!currentState.Flags.ContainsKey("chapter_2.1_started"))
                {
                    currentState.Flags["chapter_2.1_started"] = true;
                        ConsoleKey menu;
                   do
                   {
                        GameMenu.TypingAnimation("\r\nZero : what happened outside Arthur");
                        menu = Console.ReadKey(intercept: true).Key;
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : We saw a smoke going towards us so we told you sir");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : no shadow?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : None sir, this is the first time this happened");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I see, I’m gonna go outside to check");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : I’ll come with you sir");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nBoth of you went outside");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : It looks completely normal now");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Help me…");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : SIR BEHIND YOU, RUN");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou turned around and saw… a shadow");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nThe entire place turned into nothingness then a white human-shaped smoke appeared");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : WHO ARE YOU?!");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I am… a human");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Huh?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Help me go back please");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : What do you mean");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I’m trapped, they took me…");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : Explain everything to me clearly");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I am a human, the shadow took me to the black forest. They turned me into a shadow but they failed and now I’m on the run.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I’m lost, make it more clear");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Sir, They take all the humans they captured and turn them into a shadow but they kill them when they fail but I, I escaped");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : How did you escape?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Another shadow broke from their spell and regained himself and he tried escaping. He grabbed the stone but he got caught. \r\nHe threw the stone to me then he told me to find someone to ask for help which is you");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : How can I make sure this is not a trap");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I got one of the stones they need to open the realm of shadows and I can give it to you.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : How can I make sure this stone is the one they need");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Once you get a hold of it, you will be able to feel shadows, It gives you powers to fight them.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : If that is really true, I’ll help you. But what about you, do you still have your body?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : It’s gone for now, I can get it back when the Big Tree recovers.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : Why don’t you fight the shadows yourself then");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I can’t use the power of the stone.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Why is that?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Shadows can’t use these stones to gain power, they only use it to open the realm of shadows");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Please help me, I really want to go back to my body");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I have two question");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Ask me");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Did you kill Grandma Quin");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I didn’t kill anyone, I just got here");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : How can I make sure");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I swear I just got here, I have no reason to kill someone because I can control myself");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : If you really are yourself, what is your name then");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I..I can’t remember");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : You sound suspicious");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I am a failed shadow, there are some memories that I lost");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Give me a reason to trust you");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I don’t have any solid proof for you to trust me. You’re the best person I can find for this");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\n1) Trust the Shadow \r\n2) Don't trust the Shadow");
                    again10:
                        ConsoleKeyInfo keyInfo10 = Console.ReadKey(intercept: true);
                        char choice10 = keyInfo10.KeyChar;
                        switch (choice10)
                        {
                            case '1':
                                Console.Clear();
                                GameMenu.TypingAnimation("\r\nZero: I’ll trust you, I hope you’re not lying to me");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nAnonymous voice : I’m not, I swear. Thank you for trusting me. I will help you in your journey");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero : Thank you, I’ll need that a lot.");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nAnonymous voice : You can communicate with me from your mind");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero : Thanks for letting me know");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nAnonymous voice : One last thing");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero : What is it");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nAnonymous voice : Don’t tell anyone you have the stone, keep this between us only, there might be shadows around that are watching you and they might take the stone from you.");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero : Hmmm, ok");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nSlowly the nothingness place faded and you got back from where you were. You told Arthur that the shadow failed to take you. \r\nArthur kept asking you questions about it but you quickly changed the topic to the next plan");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                break;
                            case '2':
                                Console.Clear();
                                GameMenu.TypingAnimation("\r\nAnonymous voice : Ah I see. I’m sorry");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero : Huh, why");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nAnonymous voice : Only the person that will help me should know about the stone");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero : Wait…");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nAnonymous voice : I don’t want to do this but I have to");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero : What are you planning to…");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nAnonymous voice : I’ll end your life here now… forgive me");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nU felt thorns inside your body and you slowly died");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                break;
                            default:
                                goto again10;
                        
                        }
                        
                    } while (menu != ConsoleKey.Escape);
                        
                        
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void ChapterTwoPointTwo()
        {
            try
            {
                if (!currentState.Flags.ContainsKey("chapter_2.2_started"))
                {
                    currentState.Flags["chapter_2.2_started"] = true;

                    ConsoleKey menu;
                    do
                    {
                        GameMenu.TypingAnimation("\r\nYou have been thinking if having Arthur and Erebus might be a good idea to bring on your journey.");
                        menu = Console.ReadKey(intercept: true).Key;
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Sir, I don’t think you should bring anyone with you, later on they will know that you have the stone. \r\nIf they found out about it, another shadow from afar may hear you two talking, this is why I’m only talking to you thru mind so no one can hear us.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nIt will also be very dangerous to keep someone beside you when you can only protect yourself.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero: That’s what I’ve been thinking too");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero: Arthur, I don’t think you should follow me any further, I’m the investigator here, I think I got everything \r\nI need to know about this case. I can’t risk you getting caught by a shadow, tell Erebus the same.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : But I can handle myself, I can do this. I can protect myself sir.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : It’s too dangerous Arthur, I don’t want you to die");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : I don’t wanna go back! I’m doing this for my friend so I’m going whether you like it or nah");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur walked past you");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou pulled Arthur and pinned down");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Go… Back");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur started crying");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nArthur : WHY DO YOU CARE ABOUT ME, IF YOUR JOB IS TO INVESTIGATE AND GET RID OF THESE SHADOWS THEN DON’T MIND ME, I WANT TO GET MY FRIEND");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : IDIOT, YOU’RE JUST RISKING YOUR LIFE FOR NOTHING, YOU CAN’T GET CLOSE TO THEM.\r\n When you see a shadow you have to run and you can’t just keep running, SO JUST GO BACK!");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : THAT GOES THE SAME FOR YOU! ALL YOU CAN DO IS RUN AS WELL!");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : BUT I CAN DO BETTER THAN YOU");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : GET OFF ME, I’M GOING");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Arthur please… I promise you, I’m bringing back your friend");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nArthur : I don’t wanna leave… *sobbing*");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I know that… but I want you to be safe");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAfter a long time of crying and telling Arthur why he should just stay back…");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur : Fine, I’ll trust you with everything but if you ever fail to get my friend, I’ll beat you up");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I promise you Arthur, thank you for trusting me. I’ll definitely make sure your friend comes back.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nArthur began walking away to the church to tell Erebus what happened.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Good decision sir, I’ll make sure to get his friend back.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Let’s go");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou started walking to the Big Tree with the guide of the shadow.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Shadow, can you tell me the power of this stone");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I’m not familiar with its power however we can test it if you want");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : With who?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nAnonymous voice : look at you left");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou looked at your left and saw an eye shadow.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nIt started pulling you into the darkness");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        InitiateCombat(enemies["Cultists"]);
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : This is too powerful");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I agree. Did it do any harm to you?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : I don’t feel any pain");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : That’s good");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                
                    } while (menu != ConsoleKey.Escape);
                
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void ChapterTwoPointThree()
        {
            try
            {
                if (!currentState.Flags.ContainsKey("chapter_2.3_started"))
                {
                    currentState.Flags["chapter_2.3_started"] = true;
                    ConsoleKey menu;

                    do
                    {
                        GameMenu.TypingAnimation("\r\nYou continued walking.");
                        menu = Console.ReadKey(intercept: true).Key;
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou see the Big Tree from afar");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : I see it!");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou continued walking");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : Huh, what is this black smoke");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : Try going inside");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nYou walked straight inside the smoke and you got out from where you’ve been");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero: Huh");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I guess we should try going around");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou went around and found a door with 5 symbols that looks like the phases of moon");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero : They look like phases of moon, The New Moon, Crescent Moon, Half Moon, Gibbous Moon and Full Moon");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice : I believe I saw some shadows that had those symbols around here when I escaped.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero : We should roam around then");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;

                        int shadowDefeated = 0;
                        string[] symbols = { "New Moon", "Cresent Moon","Half Moon", "Gibbous Moon", "Full Moon" };
                        int number = 0;
                        string[] direction = { "[L}eft", "[R]ight" };
                        

                        while (shadowDefeated != 5)
                        {
                            try
                            {
                                GameMenu.TypingAnimation("\nWhich direction do you want to go?");
                                MenuControls.ShowChoices(direction);
                                int directionChoice = Convert.ToInt32(Console.ReadLine());
                                if (directionChoice == 1)
                                {
                                    if (Random.Shared.Next(2) == 0)
                                    {
                                        
                                        InitiateCombat(enemies["Dreadlord"]);
                                    }
                                    GameMenu.TypingAnimation("You turned left and discovered nothing");
                                }
                                else if (directionChoice == 2)
                                {

                                }
                                else
                                {
                                    Console.WriteLine("\nInvalid Selection. Please only type in the shown number of each choice.");
                                    continue;
                                }
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("\nInvalid Input. Please only select from the choices provided");
                            }
                            MenuControls.ShowChoices(direction);
                            Console.WriteLine("you have gained" + symbols[number]);
                            number++;
                            shadowDefeated++;
                        }
                        GameMenu.TypingAnimation("You have acquired all of the moon symbols");





                        

                        GameMenu.TypingAnimation("\r\n(eto na yung turn left or right tas may makikitang shadow)");
                        Console.ReadKey(intercept: true);
                                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou finally got the last symbol and you went back to the door");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou inserted all the symbols and the door unlocked.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                
                    } while (menu != ConsoleKey.Escape);


                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void ChapterTwoPointFour()
        {
            try
            {
                if (!currentState.Flags.ContainsKey("chapter_2.4_started"))
                {
                    currentState.Flags["chapter_2.4_started"] = true;

                        ConsoleKey menu;
                    do
                    {
                        GameMenu.TypingAnimation("\r\nYou inserted all the symbols and the door unlocked.");
                        menu = Console.ReadKey(intercept: true).Key;
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero: Finally, it’s open.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou stepped inside and found yourself in a mystical chamber illuminated by a bright moonlight.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero: Finally, it’s open.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice: This is the heart of the shadows' power. Be cautious.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou walked around the chamber, examining the walls and the symbols.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nZero: These symbols... They seem to tell a story.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice: They depict the rise of the God of Shadows and the creation of the stone.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero: So this stone is connected to the God of Shadows?");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice: Yes, it holds a fragment of his power.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                
                
                    } while (menu != ConsoleKey.Escape);


                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void ChapterThree()
        {
            try
            {
                if (!currentState.Flags.ContainsKey("chapter_3_started"))
                {
                    currentState.Flags["chapter_3_started"] = true;
                    
                         ConsoleKey menu;
                    do
                    {
                        GameMenu.TypingAnimation("\r\nYou continued your journey, guided by the shadow.");
                        menu = Console.ReadKey(intercept: true).Key;
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero: This place is like a maze.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice: Stay focused. We’re getting closer.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nYou encountered more shadows along the way, but the stone protected you.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nZero: This stone is incredible. It’s like a shield.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\nAnonymous voice: Use it wisely. The God of Shadows will not be pleased.");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
                        GameMenu.TypingAnimation("\r\n1) Press on towards the Big Tree.\r\n2) Take a moment to rest.\r\n");
                        Console.ReadKey(intercept: true);
                        if (menu == ConsoleKey.Escape) break;
               
                        again11:
                        ConsoleKeyInfo keyInfo11 = Console.ReadKey(intercept: true);
                        char choice11 = keyInfo11.KeyChar;
                        switch (choice11)
                        {
                            case '1':
                                Console.Clear();
                                GameMenu.TypingAnimation("\r\nYou pushed forward, determined to reach the Big Tree.");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero: I can see it. We’re almost there.");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nAnonymous voice: Be prepared. The God of Shadows will be waiting.");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;

                                break;
                            case '2':
                                Console.Clear();
                                GameMenu.TypingAnimation("\r\nYou took a brief rest to gather your strength.");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nZero: I need to be at my best for this.");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
                                GameMenu.TypingAnimation("\r\nAnonymous voice: Rest, but stay alert. The shadows are always watching.");
                                Console.ReadKey(intercept: true);
                                if (menu == ConsoleKey.Escape) break;
               
                                goto again11;
                            default:
                                goto again11;
               
               
                        }
                    } while (menu != ConsoleKey.Escape);

                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void ChapterThreePointOne()
        {
            try
            {
                if (!currentState.Flags.ContainsKey("chapter_3.1_started"))
                {
                    currentState.Flags["chapter_3.1_started"] = true;
                        ConsoleKey menu;
                    do
                    {
                       GameMenu.TypingAnimation("\r\nYou reached the Big Tree, and a figure emerged from the shadows.");
                       menu = Console.ReadKey(intercept: true).Key;
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nZero: Who are you?");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nErebus: It’s me, Erebus. I’ve been waiting for you.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nZero: Erebus? What are you doing here?");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nErebus: I am not just a villager. I am the God of Shadows.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nZero: What?! You’ve been deceiving us all along?");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       Console.Clear();
                       GameMenu.TypingAnimation("\r\nErebus: Yes, and now you’ve brought the stone to me. Its power will be mine once again.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\n1) Confront Erebus\r\n2) Try to reason with Erebus.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
               
                       again12:
                       ConsoleKeyInfo keyInfo12 = Console.ReadKey(intercept: true);
                       char choice12 = keyInfo12.KeyChar;
                       switch (choice12)
                       {
                           case '1':
                               Console.Clear();
                               GameMenu.TypingAnimation("\r\nZero: I won’t let you take the stone!");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
                               GameMenu.TypingAnimation("\r\nErebus: Foolish human. You cannot stop me.");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
                               GameMenu.TypingAnimation("\r\nA fierce battle ensued, with Erebus using his shadow powers against you.");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
               
                               break;
                           case '2':
                               Console.Clear();
                               GameMenu.TypingAnimation("\r\nZero: Erebus, you don’t have to do this. We can find another way.");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
                               GameMenu.TypingAnimation("\r\nErebus: There is no other way. The power of the shadows is my destiny.");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
                               GameMenu.TypingAnimation("\r\nZero: Please, Erebus. Think about the villagers. Think about Arthur.");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
                               GameMenu.TypingAnimation("\r\nErebus: Enough! The stone’s power will be mine!");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
                               GameMenu.TypingAnimation("\r\nbattle*");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
                               break;
                           default:
                               goto again12;
               
               
                       }
                       Console.Clear();
                       GameMenu.TypingAnimation("\r\nYou channeled the stone’s power, creating a blinding light that weakened Erebus.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nErebus: No! This cannot be!");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nZero: It’s over, Erebus. The shadows will no longer harm the villagers.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nErebus: You may have won this battle, but the shadows will always exist.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nErebus vanished into the darkness, leaving you victorious.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nZero: It’s finally over.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\nAnonymous voice: You’ve done well, Zero. The village is safe.");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
                       GameMenu.TypingAnimation("\r\n1) Return to the village\r\n2) Reflect on the journey");
                       Console.ReadKey(intercept: true);
                       if (menu == ConsoleKey.Escape) break;
               
                       again13:
                       ConsoleKeyInfo keyInfo13 = Console.ReadKey(intercept: true);
                       char choice13 = keyInfo13.KeyChar;
                       switch (choice13)
                       {
                           case '1':
                               Console.Clear();
                               GameMenu.TypingAnimation("\r\nTo be continued...");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
                               break;
                           case '2':
                               Console.Clear();
                               GameMenu.TypingAnimation("\r\nTo be continued...");
                               Console.ReadKey(intercept: true);
                               if (menu == ConsoleKey.Escape) break;
                               break;
                           default:
                               goto again13;
               
               
                       }
               
               
                   } while (menu != ConsoleKey.Escape);
                    

                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}