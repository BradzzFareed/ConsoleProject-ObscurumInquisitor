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
                {"Mutilated Villager", new Character("Plagued Villager", 115, 18, 4, 21)},
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
                Player = new Character("Detective McGilis \"Detective Zero\" Rosenberger", 200, 30, 4, 50),
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
                        

                        GameMenu.TypingAnimation("\r\nDirector: Ah detective you're finally here, I've been waiting for you. Come, have a seat we have much to talk about.");
                        

                        GameMenu.TypingAnimation("\r\n*You took a seat*\r\n");
                        string[] options = { "Hello Director, another treasure hunt I presume?", "Greetings Director, another missing person again I'm guessing?", "What do you need for me to accomplish this time boss? Review a file case perhaps?" };
                        MenuControls.ShowChoices(options);
                        GameMenu.TypingAnimation("\r\nChoice: ");

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            if (choiceString == "1" || choiceString == "2" || choiceString == "3")
                            {
                                Console.ResetColor();
                                Console.Clear();
                                string detectiveDialogue = choiceString switch
                                {
                                    "1" => "Detective Zero: Hello Director, another treasure hunt I presume?",
                                    "2" => "Detective Zero: Greetings Director, another missing person again I'm guessing?",
                                    "3" => "Detective Zero: What do you need for me to accomplish this time boss? Review a file case perhaps?",
                                    _=> throw new InvalidOperationException("Invalid choice detected.")
                                };
                                GameMenu.TypingAnimation(detectiveDialogue);
                                break;
                            }
                            else
                            {
                                GameMenu.TypingAnimation("\r\nInvalid choice. Please choose from \"1\", \"2\", or \"3\" only.");
                            }
                        }                       
                    }
                    else if (!currentState.Flags.ContainsKey("conversation_with_director"))
                    {
                        currentState.Flags["conversation_with_director"] = true;

                        GameMenu.TypingAnimation("\r\nDirector: Its a different one and its rather interesting. This kind of request is the first of its kind and it is something quite unusual.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Hmm how intriguing, the first of its kind you say? what might this unusual case be director?");
                        

                        GameMenu.TypingAnimation("\r\nDirector:\"Shadow Hunting\". That was the name of the request submitted to the department.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Huh that is indeed unusual. Is there a chance that this is just some wild goose chase director?");
                        

                        GameMenu.TypingAnimation("\r\nDirector: Bizarre as it sounds, we cannot simply label it as some wild goose chase. According to this memo, there seems to be some kind of anomaly looming over the village. A great number of people have reportedly gone missing or dead all in the span of one night and some witnesses say some bodies look either mutilated or drained, as if the soul was forcefully removed from the body...");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: Sweet mother of mercy...");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Seems quite late for a halloween celebration innit? Are there any clues said by the sender that could aid in narrowing the cause of all this?");
                        

                        GameMenu.TypingAnimation("\r\nDirector: Nothing of significance unfortunately. They only said something about a weird shadow surrounding the dead victims before recovering their corpses.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Ohh I see...I have no idea what I'm supposed to make of that. Are you sure this isn't a prank director?");
                        

                        GameMenu.TypingAnimation("\r\nDirector: Believe me I hope it was a prank. Regardless, I'm going to need you to deploy to the town to investigate. If what the towspeople are saying are true then we cannot ignore the unease and casualties that are happening.\r\n");
                        string[] options = { "Express Frustration", "Laugh at the adsurdity" };
                        MenuControls.ShowChoices(options);
                        GameMenu.TypingAnimation("\r\nChoice: ");
                    
                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            if (choiceString == "1" || choiceString == "2")
                            {
                                Console.ResetColor();
                                Console.Clear();
                                string detectiveDialogue = choiceString switch
                                {
                                    "1" => "Detective Zero: *Expresses Frustration*\r\nVery well then director. If you insist that I investigate this case then I shall do so with haste.",
                                    "2" => "Detective Zero: *Laughs at the absurdity*\r\nEver the serious type eh director MWAHAHAHAHA. Do not worry, I will make quick work of these silly antics.",
                                    _ => throw new InvalidOperationException("Invalid choice detected.")
                                };
                                GameMenu.TypingAnimation(detectiveDialogue);                                
                                break;
                            }
                            else
                            {
                                GameMenu.TypingAnimation("\r\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                            }
                        }
                        GameMenu.TypingAnimation("\r\nDirector: To be frank detective, I do not expect you to actually catch a shadow...I don't even think that's possible. What matters the most is to investigate who or what is causing this malevolence in the village.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: You have my word director. So do I have anything to start with aside from the clue you gave earlier?");
                        

                        GameMenu.TypingAnimation("\r\nDirector: As much as I want to provide more clues detective, I am afraid that is all we currently have at the moment.\r\n");
                        string[] options2 = { "*Sigh * I guess I'll make do of what I have for now", "Great...just great. (sarcastically said)" };
                        MenuControls.ShowChoices(options2);
                        GameMenu.TypingAnimation("\r\nChoice: ");
                       
                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            if (choiceString == "1" || choiceString == "2")
                            {
                                Console.ResetColor();
                                Console.Clear();
                                string detectiveDialogue = choiceString switch
                                {
                                    "1" => "Detective Zero: *Sigh* I guess I'll make do of what I have for now",
                                    "2" => "Detective Zero: Great...just great. (sarcastically said)",
                                    _ => throw new InvalidOperationException("Invalid choice detected.")
                                };
                                GameMenu.TypingAnimation(detectiveDialogue);                                
                                break;
                            }
                            else
                            {
                                GameMenu.TypingAnimation("\r\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                            }
                        }

                        GameMenu.TypingAnimation("\r\nDirector: I guess it is settled then. Go to the village of Fanfoss and meet with the local church's head priest. He was the one that enlisted our aid and asking him questions would be a good start. The priest provided the directions to the town, I will send it to you the later.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Many thanks director. I will leave for the town at dawn.");
                        

                        GameMenu.TypingAnimation("\r\nDirector: Godspeed Detective McGilis \"Detective Zero\" Rosenberger. Do not hesitate to call should the situation escalate to a dangerous degree.\r\n");                      
                        string[] options3 = { "Express Confidence", "Consider the Director" };
                        MenuControls.ShowChoices(options3);
                        GameMenu.TypingAnimation("\r\nChoice: ");
                        
                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            switch (choiceString)
                            {
                                case "1":
                                    currentState.Flags["unable_to_contact_director"] = true;
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("Detective Zero: Baaaaah you worry too much director. There is no need to call for I intend to solve this case as fast as possible.");
                                    GameMenu.TypingAnimation("\r\n*Proceeds to exit the office*");
                                    
                                    break;
                                case "2":
                                    currentState.Flags["director_helps"] = true;
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("Detective Zero: I'll keep that in mind director. I'll make sure to immediately contact the department if things go south over there.");
                                    GameMenu.TypingAnimation("\r\n*Proceeds to exit the office*");
                                    
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose again from \"1\" or \"2\" only.");
                                    continue;
                            }                            
                            break;
                        }

                    }
                    else if (!currentState.Flags.ContainsKey("chapter_1_complete"))
                    {
                        currentState.Flags["chapter_1_complete"] = true;
                        currentState.CompletedEvents.Add("Chapter1_Completed");
                        currentState.CurrentChapter++;

                        GameMenu.TypingAnimation("\r\n*You now make your way towards the town of Fanfoss*");
                        

                        Console.Clear();
                        Console.WriteLine("Chapter One Complete! Your choices will affect future events...");
                        Console.WriteLine("Do you wish to go to the next chapter? (y) if yes or (n) if no");
                        
                        while (true)
                        {
                            Console.WriteLine("Do you wish to go to the next chapter? Type 'y' if yes or 'n' if no");
                            string decision = Console.ReadLine().ToLower();
                            switch (decision)
                            {
                                case "y":
                                    SaveGame();
                                    ChapterOnePointTwo();
                                    break;
                                case "n":
                                    SaveGame();
                                    PlayGame();
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose again from 'y' or 'n' only.");
                                    continue;
                            }
                            break;
                        }
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
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_1.1_started"))
                    {
                        currentState.Flags["chapter_1.1_started"] = true;

                        GameMenu.TypingAnimation("Chapter 1.1: The Village");
                        GameMenu.TypingAnimation("\r\n*You now make your way to the village*");
                        

                        GameMenu.TypingAnimation("\r\n*Phone Ringing*");
                        

                        GameMenu.TypingAnimation("\r\n*You checked your phone*");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Ahhhh so this is the village of Fanfoss huh? Strange...Its name does not show up when I try to search it online.");
                        

                        GameMenu.TypingAnimation("\r\nDriver: Sir, we are now here.");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: Oh thank you kind sir. Here is a little extra for the smooth ride.");
                        

                        GameMenu.TypingAnimation("\r\n*You got out of the taxi*");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: *shivers* This place gives me the creeps, there is barely any sunlight here. I should check the town first before I go to the church.");
                        

                        GameMenu.TypingAnimation("\r\n*You roam around the village and see no one. The village has a dim light and many tall trees.*");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: HELLO?!?!...IS ANYONE HERE?!?...my lord where are all the townspeople?. Hold on...is this actually a prank the Director planned? Very funny, It ain't even my birthday yet.");
                        

                        GameMenu.TypingAnimation("\r\n*Tree branches breaking sounds came from behind*");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: WHO GOES THERE!? SHOW YOURSELF!\r\n");
                        
                        string[] options = { "Investigate", "Keep going" };
                        MenuControls.ShowChoices(options);
                        
                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            switch (choiceString)
                            {
                                case "1":
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("You went closer to the tree behind you to investigate the source of those sounds.");
                                    GameMenu.TypingAnimation("\r\nDetective Zero: FREEZE!");
                                    
                                    GameMenu.TypingAnimation("\r\n*A man suddenly disarms you and covers your mouth*");
                                    
                                    GameMenu.TypingAnimation("\r\nMan: Shhhhh quiet down, they will hear you.");
                                    
                                    GameMenu.TypingAnimation("\r\nDetective Zero: *You made muffled noises* \"I'M GONNA BEAT THIS GUY TO A PULP WHEN I GET FREE!\" you said in your mind.");
                                    
                                    GameMenu.TypingAnimation("\r\nMan: Don't worry, I'm a good guy I swear so keep your mouth shut if you don't want to get in trouble.\r\n");
                                    
                                    break;
                                case "2":
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("Detective Zero: Hmm maybe it was just an animal passing by.");
                                    
                                    GameMenu.TypingAnimation("\r\n*You kept on walking until a man suddenly grabs you from behind and covered your mouth.");
                                    
                                    GameMenu.TypingAnimation("\r\nMan: Walking alone at this hour? Are you nuts!? Now shhhh before they hear you.");
                                    
                                    GameMenu.TypingAnimation("\r\nDetective Zero: \"WHO THE HELL IS THIS GUY? IMMA BEAT THE LIVING CRAP OUTTA HIM!\" you said in your thoughts.");
                                    
                                    GameMenu.TypingAnimation("\r\nMan: I'm a good guy so keep quiet before we get in trouble.\r\n");
                                    
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                                    continue;
                            }                           
                            break;
                        }
                        GameMenu.TypingAnimation("*You thought for a while on what would be the best course of action*\r\n");
                        

                        string[] options2 = { "Stay Calm and follow the man's advise.", "Break free from the man's hold and recover your gun." };
                        MenuControls.ShowChoices(options2);

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            switch (choiceString)
                            {
                                case "1":
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("*You stayed calm and obeyed the man*\r\n");
                                    break;
                                case "2":
                                    currentState.Flags["game_over_route"] = true;
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("*You broke free and quickly recovered your gun*\r\n");
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                                    continue;
                            }                           
                            break;
                        }
                    }
                    else if (!currentState.Flags.ContainsKey("met_arthur"))
                    {
                        currentState.Flags["met_arthur"] = true;

                        GameMenu.TypingAnimation("*You followed through your decision and hoped it was the wise choice*");
                        

                        if (currentState.Flags.ContainsKey("game_over_route"))
                        {
                            Console.Clear();
                            GameMenu.TypingAnimation("Detective Zero: I'm gonna have to get your name young man and you got some questions I need answered.");
                            
                            GameMenu.TypingAnimation("\r\n*A single blink of your eye and the color of the forest changed to a dark and creepy one.*");
                            
                            GameMenu.TypingAnimation("\r\nUnknown Voice: C0me mY C#%ld");
                            
                            GameMenu.TypingAnimation("\r\nDetective Zero: WHO GOES THERE!?");
                            
                            GameMenu.TypingAnimation("\r\nUnknown Voice: C’[me cl02ser my 3hi1d");
                            
                            GameMenu.TypingAnimation("\r\n*No matter where you looked, the weird noises just keep getting louder, till…*");
                            
                            GameMenu.TypingAnimation("\r\nDetective Zero: WHAT’S HAPPENING?!?!");
                            
                            GameMenu.TypingAnimation("\r\n*You see these shadow hands and eyes everywhere on the floor*");
                            
                            GameMenu.TypingAnimation("*\r\nYou lose consciousness as the shadows drag you away to who knows where*");
                            GameMenu.TypingAnimation("\r\nYou have been captured by the shadows...try again.");

                            while (true)
                            {
                                Console.WriteLine("\nPress '1' to go back to savepoint.");
                                string decision = Console.ReadLine().ToLower();
                                switch (decision)
                                {
                                    case "1":
                                        LoadGame();
                                        break;
                                    default:
                                        GameMenu.TypingAnimation("\nInvalid choice. '1' is your only choice here.");
                                        continue;
                                }
                                break;
                            }
                        }
                        else
                        {
                            GameMenu.TypingAnimation("\r\n*The man removed his hand from your mouth*");
                            

                            GameMenu.TypingAnimation("\r\nMan: Let’s talk somewhere else, it’s too dangerous here");
                            

                            GameMenu.TypingAnimation("\r\n*You followed the man*");
                            

                            GameMenu.TypingAnimation("\r\nMan: My name's Arthur, I’m a villager here. I’m sorry I had to cover your mouth earlier.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Greetings Arthur, you have my thanks for saving my existence. I’ve been sent here to investigate some anomalies made by \"shadows\". Uhhh may I ask what are those hands you’re talking about are?");
                            

                            GameMenu.TypingAnimation("\r\nArthur: You won’t be able to get out of the forest when the \"hands\" get you, we were informed by the chief that the people have been missing in the woods so we named it “hands” because something pulls you there and you die.");
                            

                            Console.Clear();
                            GameMenu.TypingAnimation("Arthur: We’ve never actually seen the \"hands\", only the church worker named Erebus did.");
                            

                            GameMenu.TypingAnimation("\r\nArthur: The only time that we townspeople are able to find the missing people is when they are either dead or their bodies mutilated at dark hidden places.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Hmm...hidden places you say? \"I guess the director was not joking when he said about the dead bodies being drained of their soul\"");
                            

                            GameMenu.TypingAnimation("\r\nArthur: My friend has been caught and he is still missing. It’s been months and I still have no clue where he is. I just hope he’s alive.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: I’m sorry about your friend. It seems that the village has gone through a lot of hard things. I would like to help the village in finding out the cause of these deaths. Arthur, will you aid me? I’m not very familiar with the village so I need some help to navigate in case I accidentally get close to the “hands”.");
                            

                            GameMenu.TypingAnimation("\r\nArthur: I’ll do anything to find my friend. I will do what I can to give you all the help you need.");
                            

                            Console.Clear();
                            GameMenu.TypingAnimation("Detective Zero: Excellent! So the first thing we need to do is go to the church because my boss told me to meet the priest. He said that he may hold information about the anomaly that is happening here.");
                            

                            GameMenu.TypingAnimation("\r\nArthur: Okay Mr.Detective. I’ll guide you there.");
                            

                            GameMenu.TypingAnimation("\r\nArthur: Come to think of it detective, what is your name?");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Ahhhh my name is Mcgilis \"Detective Zero\" Rosenberger.");
                            

                            GameMenu.TypingAnimation("\r\nArthur: Detective Zero? Why are you called Detective Zero? Not to be rude or anything but I think it does not fit with the rest of your name.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Not to worry. This isn't the first that anyone is bothered by my nickname. I got that nickname because of my record in the investigations department. I have Detective Zero pending cases because I always managed to solve all of them then as time passed, my colleagues began to call me \"Detective Zero\" and it kind of grew on me.");
                            

                            GameMenu.TypingAnimation("\r\nArthur: Ohhhh then its an honor to work with you Detective \"Detective Zero\".");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Its a pleasure to work with you as well, Arthur");
                            
                        }
                    }
                    else if (!currentState.Flags.ContainsKey("chapter_1.1_complete"))
                    {
                        currentState.Flags["chapter_1.1_complete"] = true;
                        currentState.CompletedEvents.Add("Chapter_1.1_Completed");
                        currentState.CurrentChapter++;

                        Console.Clear();
                        GameMenu.TypingAnimation("\r\n*After meeting arthur, both of you procede to make your way towards the church*");
                        

                        Console.Clear();
                        Console.WriteLine("Chapter 1.1 Complete! Your choices will affect future events...");
                   
                        while (true)
                        {
                            Console.WriteLine("Do you wish to go to the next chapter? Type 'y' if yes or 'n' if no");
                            string decision = Console.ReadLine().ToLower();
                            switch (decision)
                            {
                                case "y":
                                    SaveGame();
                                    ChapterOnePointTwo();
                                    break;
                                case "n":
                                    SaveGame();
                                    PlayGame();
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose again from 'y' or 'n' only.");
                                    continue;
                            }
                            break;
                        }
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
        private void ChapterOnePointTwo()
        {
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_1.2_started"))
                    {
                        currentState.Flags["chapter_1.2_started"] = true;

                        GameMenu.TypingAnimation("Chapter 1.2: The Church");
                        

                        GameMenu.TypingAnimation("\r\n*You have arrived at the church*");
                        

                        GameMenu.TypingAnimation("\r\nArthur: I’ll go get the priest sir, just wait here");
                        

                        GameMenu.TypingAnimation("\r\n*You looked around the church and saw no one, just a dim light inside and wooden chairs and a lit candle.*");
                        

                        GameMenu.TypingAnimation("\r\n*10 minutes have passed*");
                        

                        GameMenu.TypingAnimation("\r\n*Arthur came back with a priest and another person*");
                        

                        GameMenu.TypingAnimation("\r\nArthur: Sir I have the priest with me and his worker");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Priest Lucius: Good afternoon to you sir, you must be the Detective that the Director sent to investigate what’s happening in the village. Allow me to introduce myself, I’m Priest Lucius, the one and only priest of this village and this is Erebus, a worker in the church that knows a lot about these shadows. He will be helping you to solve this case. Thank you very much for coming all the way here.");
                        

                        GameMenu.TypingAnimation("\r\nErebus: It’s a pleasure to meet you Mr. Detective, feel free to ask me any questions");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Thank you for welcoming me in this village sir, also you can just call me Detective Zero, that’s my codename.");
                        

                        GameMenu.TypingAnimation("\r\nErebus: Ah! I see Mr. Detective Zero, I will be accompanying you, please tell me if you want me to do something.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Thank you Erebus and Father Lucius, I will do my best to solve this case and help the village.");
                        

                        GameMenu.TypingAnimation("\r\nPriest Lucius: Thank you very much Mr. Detective Zero. Has the Director already informed you of what is happening here?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: Yes Father, he has already informed me about the situation.");
                        

                        GameMenu.TypingAnimation("\r\nPriest Lucius: Then I won’t need to explain it to you again, Erebus can give you more information about it when you have something in mind to ask.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Understood Father. Erebus may I ask you some questions before we start investigating?");
                        

                        GameMenu.TypingAnimation("\r\nErebus: Of course sir, ask away and I will try to answer as many as possible.");
                        string[] questions = { "About the hands", "About the shadows", "About the village", "About you", "Thank you Eberus for answering my questions" };
                        
                        bool isSelected = true;
                        while (isSelected)
                        {
                            try
                            {
                                GameMenu.TypingAnimation("\r\nErebus: What would you like to know about?");
                                GameMenu.TypingAnimation("\r\nEnter the number of your desired choice. (type 5 to finish conversation)");
                                MenuControls.ShowChoices(questions);
                                int inquiryChoice = Convert.ToInt32(Console.ReadLine());
                                if (inquiryChoice == 5)
                                {
                                    isSelected = false;
                                    Console.Clear();
                                    Console.ResetColor();
                                    GameMenu.TypingAnimation("\r\nOn second thought, I will continue the questioning later once I have an outline of the situation.");
                                    
                                    break;
                                }
                                if (inquiryChoice < 1 || inquiryChoice > questions.Length)
                                {
                                    GameMenu.TypingAnimation("\r\nErebus: I am afraid that is outside my knowledge detective.");
                                    Thread.Sleep(2000);
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
                            catch (FormatException ex)
                            {
                                Console.WriteLine($"\nError: {ex.Message}");
                                Console.WriteLine("\nInvalid Input. Please only type a number.");
                            }
                        }
                        GameMenu.TypingAnimation("\r\nDetective Zero: Regardless, thank you Erebus for your concerns.");
                        

                        GameMenu.TypingAnimation("\r\nErebus: You’re welcome sir");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Well then, it’s time to go. Father Lucius, thank you again for welcoming me in the village and having Erebus to help me in this case.");
                        

                        GameMenu.TypingAnimation("\r\nPriest Lucius: It is alright my child, if it means helping the village then I will do anything in my power.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Commendable Father, we shall be going now");
                        
                        
                    }
                    else if (!currentState.Flags.ContainsKey("chapter_1.2_complete"))
                    {
                        currentState.Flags["chapter_1.2_complete"] = true;
                        currentState.CompletedEvents.Add("Chapter_1.2_Completed");
                        currentState.CurrentChapter++;

                        GameMenu.TypingAnimation("\r\n*You, along with Erebus and Erebus exited the church*");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: The investigation proper shall now commence");
                        

                        Console.Clear();
                        Console.WriteLine("Chapter 1.2 Complete! Your choices will affect future events...");
                        

                        while (true)
                        {
                            Console.WriteLine("\nDo you wish to go to the next chapter? Type 'y' if yes or 'n' if no");
                            string decision = Console.ReadLine().ToLower();
                            switch (decision)
                            {
                                case "y":
                                    SaveGame();
                                    ChapterTwo();
                                    break;
                                case "n":
                                    SaveGame();
                                    PlayGame();
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from 'y' or 'n' only.");
                                    continue;
                            }                            
                            break;
                        }
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
        private void ChapterTwo()
        {
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_2_started"))
                    {
                        currentState.Flags["chapter_2_started"] = true;

                        GameMenu.TypingAnimation("Chapter 2: "); 
                        GameMenu.TypingAnimation("*You are going to the villagers to ask about their experiences*");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Arthur, can you be the guard outside of the house, tell me if you see a shadow or something suspicious.");
                        

                        GameMenu.TypingAnimation("\r\nArthur: Okay sir, I’ll do that");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Thank you. Erebus, come with me inside, You’re the one with the most experience with these shadows so I’ll need that in interviewing the villagers.");
                        

                        GameMenu.TypingAnimation("\r\nErebus: Alright sir.");
                        

                        GameMenu.TypingAnimation("\r\nYou have entered the first house");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Erebus: Good afternoon Ms. Anita, I’m sorry for the intrusion, We would just want to interview you quickly to help \r\nour Detective in finding these shadows that have been harming our village.");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita rushed to you and grabbed your shoulders.");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita: SIR PLEASE FIND MY BROTHER, IT’S BEEN AGES SINCE I SAW HIM.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Ma’am please calm down, I will try my best to find your brother and bring him back.");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita wiped her tears and sat down");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita: I’m sorry sir, it’s just been…really hard for me.");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: I understand Ms. Anita, that’s why I’m here to get some information to find your brother.");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita: Thank you sir *sobbing*");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Ms. Anita I’m really sorry but I don’t really have much time to stay here for too long so I’ll be asking you the questions right away.");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita wiped her tears");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita: You can ask me now sir.");
                        

                        GameMenu.TypingAnimation("\r\nZero: Have you seen a shadow?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Ms. Anita: Just once sir, it was when they took my brother…");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: How did it take your brother?");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita: I just saw my little brother going closer to the shadow. When I blinked, he was gone.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Did he leave anything?");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita: No sir, there was nothing left behind");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: What time did this happen?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Ms. Anita: It was almost night when I saw the shadow. I was cooking dinner. I kept calling my brother but he wouldn’t respond so \r\nI went to see what he’s doing. But then I saw him going to the shadow, I shouted at him but I was too late.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: I see. What did the shadow look like?");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita: It was eye-shaped sir, just staring at my brother.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Got that, where did you see it?");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita: At the middle of the ceiling sir");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Were there any lights?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Ms. Anita: There were no lights sir because we lost electricity that day, only the moonlight was there.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Understood, thank you Ms. Anita. These will be my questions for now, I’ll come back if I have something I have to ask you. I will find your brother so don’t worry.");
                        

                        GameMenu.TypingAnimation("\r\nMs. Anita : Thank you sir, please get him back.");
                        

                        GameMenu.TypingAnimation("\r\nYou left the house");
                        

                        GameMenu.TypingAnimation("\r\nArthur: You done sir?");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Yes, did anything happen?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Arthur: None sir, everything looks normal");
                        

                        GameMenu.TypingAnimation("\r\nZero: Ok that’s good, let’s move to the next house");
                        

                        GameMenu.TypingAnimation("\r\nYou have reached the second house");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Let’s do what we did earlier again");
                        

                        GameMenu.TypingAnimation("\r\nArthur: Got it sir.");
                        

                        GameMenu.TypingAnimation("\r\nYou knocked at the door");

                        GameMenu.TypingAnimation("\r\nNo one responds\r\n");
                        string[] options = { "Knock again", "Go in without permission" };
                        MenuControls.ShowChoices(options);

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            switch (choiceString)
                            {
                                case "1":
                                    Console.ResetColor();
                                    Console.Clear();
                                    currentState.Flags["delayed_action"] = true;

                                    GameMenu.TypingAnimation("\r\n*No one responds again*");
                                    

                                    GameMenu.TypingAnimation("\r\nDetective Zero: Let’s open the door Erebus");
                                    

                                    GameMenu.TypingAnimation("\r\nDetective Zero: Wait here Arthur");
                                    

                                    GameMenu.TypingAnimation("\r\nArthur: Will do detective");
                                    

                                    GameMenu.TypingAnimation("\r\nErebus: Right away sir");
                                    

                                    GameMenu.TypingAnimation("\r\n*You have entered the house and saw a pale man in the corner of the house*");
                                    
                                    break;
                                case "2":
                                    Console.ResetColor();
                                    Console.Clear();
                                    currentState.Flags["swift_action"] = true;

                                    GameMenu.TypingAnimation("\r\nYou have entered the house and saw a pale man in the corner of the house");
                                    
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                                    continue;
                            }                           
                            break;
                        }
                    }
                    else if (!currentState.Flags.ContainsKey("met_pale_man"))
                    {
                        currentState.Flags["met_pale_man"] = true;

                        if (currentState.Flags.ContainsKey("delayed_action"))
                        {
                            currentState.Flags["memory_trigger"] = true;
                            GameMenu.TypingAnimation("\r\nDetective Zero: Sir, are you all right?");
                            

                            GameMenu.TypingAnimation("\r\n*The Person didn’t respond*");
                            

                            GameMenu.TypingAnimation("\r\n*You went closer*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Sir, can you hear me?");
                            

                            GameMenu.TypingAnimation("\r\n*No response*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Erebus, why won’t he speak? Is there something that happened to him?");
                            

                            GameMenu.TypingAnimation("\r\n*While talking to Erebus, the man suddenly stood and pounced on you!*");
                            

                            GameMenu.TypingAnimation("\r\nErebus: Detective behind you! \"Erebus shouted\".");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: WHAT THE?!");
                            

                            GameMenu.TypingAnimation("\r\nMan: I WILL HAVE MY VENGEANCE DAMNED SHADOW! \"The man said in an enraged manner\"");
                            

                            GameMenu.TypingAnimation("\r\n*You managed to dodge the attack but the man's attention went to Erebus*");
                            

                            GameMenu.TypingAnimation("\r\nErebus: AAAAAAAH!");
                            

                            GameMenu.TypingAnimation("\r\n*The man with no hint of stopping prepares to slash Erebus' throat*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: DAMNED LUNATIC!");
                            

                            GameMenu.TypingAnimation("\r\n*Without hesitation, you took out your gun and shot the man right on the head*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Heavens. Well that was unexpected, you all right lad?");
                            

                            GameMenu.TypingAnimation("\r\nErebus: Thanks detective, I owe you one but damn, it saddens me that he became like this");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: You knew this man? Who is he?");
                            

                            GameMenu.TypingAnimation("\r\nHis name was Mr. Mark, he lost his wife and daughter from the shadows that’s why he’s so pale, decimated that his family was taken away from him by these cursed entities. I guess his mind broke unable to accept what reality had given him");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: May his soul rest in piece. \"God...this case has now gotten a lot more serious\".");
                            

                            GameMenu.TypingAnimation("\r\nErebus: We should not stay here any longer detective, night is fast approaching.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: That would be the best course of action. I swear on my name that justice be served for  the people of this vilage");
                            
                        }
                        else if (currentState.Flags.ContainsKey("swift_action"))
                        {
                            GameMenu.TypingAnimation("\r\nDetective Zero: Sir, are you all right?");
                            

                            GameMenu.TypingAnimation("\r\n*The Person didn’t respond*");
                            

                            GameMenu.TypingAnimation("\r\n*You went closer*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Sir, can you hear me?");
                            

                            GameMenu.TypingAnimation("\r\n*No response*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Erebus, why won’t he speak? Is there something that happened to him?");
                            

                            GameMenu.TypingAnimation("\r\nErebus: He is Mr. Mark, he lost his wife and daughter from the shadows that’s why he’s like that. \r\nHe saw a lot of things that’s why I figured we should go here but I guess he really can’t talk.");
                            

                            GameMenu.TypingAnimation("\r\nYou went in front of Sir Mark");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Sir, I promise we will find your daughter and wife so please don’t give up.");
                            

                            GameMenu.TypingAnimation("\r\nErebus: We should get going sir, it’s almost night.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Let’s go then.\r\n");
                            
                            string[] options = { "Leave Bread", "Continue on" };
                            MenuControls.ShowChoices(options);

                            while (true)
                            {
                                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                                char choice = keyInfo.KeyChar;
                                string choiceString = choice.ToString();
                                switch (choiceString)
                                {
                                    case "1":
                                        Console.ResetColor();
                                        Console.Clear();
                                        currentState.Flags["mark_thankful"] = true;
                                        GameMenu.TypingAnimation("*You left some leftover bread beside Mark and left.*");
                                        break;
                                    case "2":
                                        Console.ResetColor();
                                        Console.Clear();
                                        GameMenu.TypingAnimation("*You continued on*");
                                        break;
                                    default:
                                        GameMenu.TypingAnimation("\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                                        continue;
                                }                               
                                break;
                            }
                        }
                    }
                    else if (!currentState.Flags.ContainsKey("met_grandma_quin"))
                    {
                        currentState.Flags["met_grandma_quin"] = true;

                        GameMenu.TypingAnimation("\r\nDetective Zero: Tell me Erebus, what time do these shadows often reveal themselves?");
                        

                        GameMenu.TypingAnimation("\r\nErebus: Hmmmmmm, I tend to see many of them whenever it is a full moon but when it’s not a full moon, their numbers seem to be less.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: I see.");
                        

                        GameMenu.TypingAnimation("\r\n*You have arrived at the last house*");
                        

                        GameMenu.TypingAnimation("\r\nErebus: Sir I’ll join Arthur outside since it’s almost night so he would be safe incase something happens.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Ahhhhh yes please do. Call for help should anything appear. Am I understood?");
                        

                        GameMenu.TypingAnimation("\r\nErebus: Will do detective.");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("*You knocked at the door*");
                        

                        GameMenu.TypingAnimation("\r\nGrandma noise: Who is it?");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: I’m a Detective ma’am, I was dispatched here to solve the case about these shadows. I just want to ask you some questions about any experiences with them.");
                        

                        GameMenu.TypingAnimation("\r\n*The grandma opened the door*");
                        

                        GameMenu.TypingAnimation("\r\nGrandma: Come in sir");
                        

                        GameMenu.TypingAnimation("\r\n*You have entered the house*");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Grandma: Take a seat sir, my name is Quin.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: I’m sorry for disturbing your night Mrs. Quin.");
                        

                        GameMenu.TypingAnimation("\r\nGrandma Quin : It’s quite alright sir, is there anything I can help you with?\r\n");
                        string[] options = { "Shadow Encounters", "Suspicious People" };
                        MenuControls.ShowChoices(options);

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            switch (choiceString)
                            {
                                case "1":
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("Detective Zero: Have you had any encounters with these shadows lately?");
                                    
                                    GameMenu.TypingAnimation("Unfortunately yes, the shadows they...took my husband.");
                                    break;
                                case "2":
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("Detective Zero: Have you seen any suspicious activities lately?");
                                    
                                    GameMenu.TypingAnimation("\r\nGrandma Quin: I am sorry to say this detective but I can't say for certain about suspicious people as I have not stepped out of this house ever since that incident.");
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                                    continue;
                            }
                            break;
                        }
                        GameMenu.TypingAnimation("\r\nDetective Zero: I’m sorry to hear that, do you know how everything happened?");
                        

                        GameMenu.TypingAnimation("\r\nGrandma Quin: It was dinner time, I went to call my husband to eat dinner from outside, then I heard some screaming outside, \r\nI knew it was my husband. I rushed where the scream was coming from and I saw him. I saw his soul getting taken away.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: But how, I thought the victim goes to the shadow and disappears");
                        

                        GameMenu.TypingAnimation("\r\nGrandma Quin: I don’t think it was a shadow that took my husband's life, there were shadows but there’s some kind of human-shaped shadow.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Huh?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Grandma Quin: He was saying something");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: What did he say?");
                        

                        GameMenu.TypingAnimation("\r\nGrandma Quin: He said \"Don’t even think about ruining my plan\" then my husband said \"I’ll kill you -\n");
                        

                        GameMenu.TypingAnimation("\r\nErebus: SIR I NEED YOUR HELP OUTSIDE, A SHADOW APPEARED");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: WHAT, I’ll just finish what Mrs. Quin is saying");
                        

                        GameMenu.TypingAnimation("\r\nErebus: SIR WE DON’T HAVE THE TIME WE NEED TO GO");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Arthur: SIR RUN!!");
                        

                        GameMenu.TypingAnimation("\r\nA black smoke covered the entire house");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero : WHERE ARE YOU GUYS!?");
                        

                        GameMenu.TypingAnimation("\r\nYou heard Arthur somewhere");
                        

                        GameMenu.TypingAnimation("\r\nArthur: SIR");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: WHERE ARE YOU ARTHUR?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Arthur: I DON’T KNOW SIR");
                        

                        GameMenu.TypingAnimation("\r\nThe smoke went away");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: what…");
                        

                        GameMenu.TypingAnimation("\r\nArthur: SIR ARE YOU OK?!");
                        

                        GameMenu.TypingAnimation("\r\nErebus: *cough cough");
                        

                        GameMenu.TypingAnimation("\r\nThe three of you stopped in shock");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: how…");
                        

                        GameMenu.TypingAnimation("\r\nArthur: No way…");
                        

                        GameMenu.TypingAnimation("\r\nErebus: …");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Mrs. …Quin?");
                        

                        GameMenu.TypingAnimation("\r\nYou saw Mrs. Quin’s body on the floor lifeless");
                        

                        GameMenu.TypingAnimation("\r\nYou rushed and checked if she’s alive");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero : MRS. QUIN WAKE UP");
                        

                        GameMenu.TypingAnimation("\r\nMrs. Quin didn’t move");
                        

                        GameMenu.TypingAnimation("\r\nYou kept on waking her up");
                        

                        GameMenu.TypingAnimation("\r\nErebus: Sir pls stop");
                        

                        GameMenu.TypingAnimation("\r\nArthur: Let’s go give him to the priest for a burier");
                        

                        GameMenu.TypingAnimation("\r\nErebus: I’ll bring him to the church");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: Please do take good care of her");
                        

                        GameMenu.TypingAnimation("\r\nErebus: I will sir");
                        

                        GameMenu.TypingAnimation("\r\nErebus went to the church while you and Arthur stayed at the house");
                        

                    }
                    else if (!currentState.Flags.ContainsKey("chapter_2_complete"))
                    {
                        currentState.Flags["chapter_2_complete"] = true;
                        currentState.CompletedEvents.Add("Chapter_2_Completed");
                        currentState.CurrentChapter++;


                        GameMenu.TypingAnimation("\r\nErebus went to the church while you and Arthur stayed at the house");
                        

                        GameMenu.TypingAnimation("\"It looks like things are starting to escalate\".");
                        


                        Console.Clear();
                        Console.WriteLine("Chapter 2 Complete! Your choices will affect future events...");

                        while (true)
                        {
                            Console.WriteLine("Do you wish to go to the next chapter? Type 'y' if yes or 'n' if no");
                            string decision = Console.ReadLine().ToLower();
                            switch (decision)
                            {
                                case "y":
                                    SaveGame();
                                    ChapterTwoPointOne();
                                    break;
                                case "n":
                                    SaveGame();
                                    PlayGame();
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from 'y' or 'n' only");
                                    continue;
                            }                           
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

            }
        }
        private void ChapterTwoPointOne()
        {
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_2.1_started"))
                    {
                        currentState.Flags["chapter_2.1_started"] = true;

                        GameMenu.TypingAnimation("Chapter 2.1: ");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: What is happenning outside Arthur?");
                        

                        GameMenu.TypingAnimation("\r\nArthur: We saw smoke going towards us so we immediately wanted to warn you sir");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: no shadow?");
                        

                        GameMenu.TypingAnimation("\r\nArthur: None sir, this is the first time this has happened");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: I see, I’m gonna go outside to check");
                        

                        GameMenu.TypingAnimation("\r\nArthur: I’ll come with you sir");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Both of you went outside");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: It looks completely normal now");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: Help me…");
                        

                        GameMenu.TypingAnimation("\r\nArthur: SIR BEHIND YOU, RUN");
                        

                        GameMenu.TypingAnimation("\r\nYou turned around and saw… a shadow");
                        
                    }
                    else if (!currentState.Flags.ContainsKey("first_shadow_encounter"))
                    {
                        currentState.Flags["first_shadow_encounter"] = true;

                        GameMenu.TypingAnimation("\r\nThe entire place suddenly turned into nothingness then a white human-shaped smoke appeared");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: MY LORD...WHAT ARE YOU?!");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I am...a human");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Human? Who are you trying to fool here cursed shadow!?");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: Help me go back please");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: What do you mean go back? What trickery is this?");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: No trickery. I’m trapped, they took me...");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: Trapped where? All right, explain things clearly shadow. I am all ears.");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I am a human, the shadow took me to the deepest parts of the forest. They tried to turn me into a shadow but they failed and now I’m on the run.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: I’m getting lost, who or what tried to turn you into a shadow? REMEMBER!");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: They take all the humans they captured and turn them into shadows but they kill them if the transformation fails but I...I escaped.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: \"That would explain the dead bodies\" How did you escape? and what is this about turning people into shadows?");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I...don't know why but I saw...another shadow broke from their spell and regain himself then he tried escaping. He managed to grab the stone but he got caught. \r\nThen he threw the stone to me then he told me to find someone to ask for help and then I came across you.");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: Now what is this about some stone? \"I am starting to believe this is a trap...\"");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I only know that this stone may be a piece needed to open a gateway to their world, the realm of shadows.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: How sure are you that this stone is the one they need the most?");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I am uncertain but it has some \"perks\". Once you get a hold of it, you will be able to feel shadows, it  might give you the power to harm them.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: If that is really true, I’ll help you. Having the means to combat these creatures would tip the scales in our favor, but what about you, do you still have your body?");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: It’s gone for now, I can get it back when the Big Tree recovers to its former state.");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: Why don’t you fight the shadows yourself then");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I can’t use the power of the stone.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Why is that?");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: Shadows can’t use these stones to gain power, they can only use it to open the realm of shadows. It is different for corporeal beings like you.");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: Please help me, I wish to become human again. I will do all that I can to aid against them.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Very well but I have two questions first.");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Anonymous voice: What would you ask of me?");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Did you kill Grandma Quin?");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I didn’t kill anyone, I just said that I escaped them.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: How can I be sure that you're telling the truth?");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I swear I just got here, I have no reason to kill someone. The only ones I want to kill are these cursed shadows.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: If you really are still yourself, what is your name then");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Anonymous voice: I...I can’t remember. My mind is still cloudy due to the transformation process.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: You are sounding suspicious.");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I am a failed shadow, there are some memories that I lost but not all.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Give me a reason to trust you");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I don’t have any solid proof for you to trust me. You’re the best person I can find for this\r\n");
                        string[] options = { "Trust the Shadow", "Don't trust the Shadow" };
                        MenuControls.ShowChoices(options);

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            switch (choiceString)
                            {
                                case "1":
                                    currentState.Flags["trust_anonymous_shadow"] = true;
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("Detective Zero: Fine then. I’ll trust you, but I'll keep a close watch on you.");
                                    
                                    break;
                                case "2":
                                    currentState.Flags["don't_trust_the_shadow"] = true;
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("\r\nAnonymous voice: Ah I see. Your loss then.");
                                    
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                                    continue;
                            }  
                            break;
                        }
                        if (currentState.Flags.ContainsKey("trust_anonymous_shadow"))
                        {
                            GameMenu.TypingAnimation("\r\nAnonymous voice: I’m not, I swear. Thank you for trusting me. I will do my best to help.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Thank you, you might just be our trump card failed shadow.");
                            

                            GameMenu.TypingAnimation("\r\nAnonymous voice: You can communicate with me using your mind.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Telepathy huh? Thanks for letting me know.");
                            

                            GameMenu.TypingAnimation("\r\nAnonymous voice: One last thing");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: What is it");
                            

                            GameMenu.TypingAnimation("\r\nAnonymous voice: Don’t tell anyone you have the stone, keep this between us only, there might be shadows around that are watching you and they might take the stone from you when you least expect it.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Copy that. I'll keep that in mind.");
                            

                            GameMenu.TypingAnimation("\r\nSlowly the nothingness place faded and you got back from where you were. You told Arthur that the shadow failed to take you. \r\nArthur kept asking you questions about it but you quickly changed the topic to the next plan");
                            
                        }
                        else if (currentState.Flags.ContainsKey("don't_trust_the_shadow"))
                        {
                            GameMenu.TypingAnimation("\r\nDetective Zero: Huh, and why so shadow?");
                            

                            GameMenu.TypingAnimation("\r\nAnonymous voice: Only the person that will help me should know about the stone. I can't risk letting others know through you of its possession.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Wait...you can't possibly mean to-.");
                            

                            GameMenu.TypingAnimation("\r\nAnonymous voice: You left me with no choice...detective.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: What are you planning to-");
                            

                            GameMenu.TypingAnimation("\r\nAnonymous voice: I’ll end your life here now...forgive me");
                            

                            GameMenu.TypingAnimation("\r\n*You felt thorns inside your body and you slowly died*");
                            while (true)
                            {
                                Console.WriteLine("\nPress '1' to go back to savepoint.");
                                string decision = Console.ReadLine().ToLower();
                                switch (decision)
                                {
                                    case "1":
                                        LoadGame();
                                        break;
                                    default:
                                        GameMenu.TypingAnimation("\nInvalid choice. '1' is your only choice here.");
                                        continue;
                                }
                                break;
                            }
                        }
                    }
                    else if (!currentState.Flags.ContainsKey("chapter_2.1_complete"))
                    {
                        currentState.Flags["chapter_2.1_complete"] = true;
                        currentState.CompletedEvents.Add("Chapter2.1_Completed");
                        currentState.CurrentChapter++;


                        GameMenu.TypingAnimation("Now having recruited an unexpected ally.\nYou hoped the decision you made was the wise one once again");
                        


                        Console.Clear();
                        Console.WriteLine("Chapter 2.1 Complete! Your choices will affect future events...");

                        while (true)
                        {
                            Console.WriteLine("Do you wish to go to the next chapter? Type 'y' if yes or 'n' if no");
                            string decision = Console.ReadLine().ToLower();
                            switch (decision)
                            {
                                case "y":
                                    SaveGame();
                                    ChapterTwoPointTwo();
                                    break;
                                case "n":
                                    SaveGame();
                                    PlayGame();
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose 'y' or 'n' only.");
                                    continue;
                            }
                            break;
                        }
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Invalid Input. Please choose from the provided selections only.");
                }
            }
        }
        private void ChapterTwoPointTwo()
        {
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_2.2_started"))
                    {
                        currentState.Flags["chapter_2.2_started"] = true;

                        GameMenu.TypingAnimation("Chapter 2.2: The Stone");
                        

                        GameMenu.TypingAnimation("*With the unknown failed shadow offering you help. You continued on the investigation.*");
                        
                        
                        GameMenu.TypingAnimation("\r\nYou have been thinking if having Arthur and Erebus might be a good idea to bring on your journey.");
                        

                    }
                    else if (!currentState.Flags.ContainsKey("argue_with_arthur"))
                    {
                        currentState.Flags["argue_with_arthur"] = true;

                        GameMenu.TypingAnimation("\r\nAnonymous voice: Sir, I don’t think you should bring anyone with you, later on they will know that you have the stone. \r\nIf they found out about it, another shadow from afar may hear you two talking, this is why I’m only talking to you thru mind so no one can hear us.");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: It will also be very dangerous to keep someone beside you when you can only protect yourself.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: That’s what I’ve been thinking too. I should to talk to them about this.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Listen Arthur, I don’t think you should follow me any further, I’m the investigator here, I think I got everything \r\nI need to know about this case. I can’t risk you getting caught by a shadow, tell Erebus the same.");
                        

                        GameMenu.TypingAnimation("\r\nArthur: But I can handle myself, I can do this. I can protect myself sir.");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: It’s too dangerous Arthur, I don’t want you to die and end up like your friend did. You still have a long life ahead of you.");
                        

                        GameMenu.TypingAnimation("\r\nArthur: I don’t want go back detective! Not after we are this close to ending this case. I’m doing this for my friend so I’m going whether you like it or not!");
                        

                        GameMenu.TypingAnimation("\r\n*Arthur walked past you*");
                        

                        GameMenu.TypingAnimation("\r\n*You pulled Arthur and pinned down*");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: I won't repeat myself again Arthur. Go...back.");
                        

                        GameMenu.TypingAnimation("\r\n*Arthur angered and started crying*");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Arthur: WHY DO YOU EVEN CARE ABOUT ME, IF YOUR JOB IS TO INVESTIGATE AND GET RID OF THESE SHADOWS THEN DON’T MIND ME, I WANT TO GET MY FRIEND");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: IDIOT, YOU’RE JUST RISKING YOUR LIFE FOR NOTHING, YOU CAN’T GET CLOSE TO THEM.\r\n When you see a shadow you have to run and you can’t just keep running, SO JUST GO BACK!");
                        

                        GameMenu.TypingAnimation("\r\nArthur: THAT GOES THE SAME FOR YOU! ALL YOU CAN DO IS RUN AS WELL!");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: BUT I CAN'T RISK MORE PEOPLE BECOMING VICTIMS.");
                        

                        GameMenu.TypingAnimation("\r\nArthur: GET YOUR HANDS OFF ME! I’M GOING.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Arthur...I promise you, I’m bringing back your friend but you need to go back.");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Arthur: But...I don’t want to leave...*sobbing*");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: I know that you want to save your friend but what matters here is your safety.");
                        

                        GameMenu.TypingAnimation("\r\n*After a long time of crying and telling Arthur why he should just stay back...*");
                        

                        GameMenu.TypingAnimation("\r\nArthur: Fine, I’ll trust you with everything but if you ever fail to get my friend, I’ll beat you up");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: I promise you Arthur, thank you for trusting me. I’ll definitely make sure your friend comes back.");
                        

                        GameMenu.TypingAnimation("\r\n*Arthur began walking away to the church to tell Erebus what happened.*");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Anonymous voice: Good decision sir, we'll make sure to get his friend back.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Let’s go");
                        

                        GameMenu.TypingAnimation("\r\n*You started walking to the Big Tree with the guide of the shadow.*");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Shadow, can you tell me the power of this stone");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I’m not familiar with its power however we can test it if you want");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: With who?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Anonymous voice: look at your left");
                        

                        GameMenu.TypingAnimation("\r\n*You looked at your left and saw an eye shadow.*");
                        

                        GameMenu.TypingAnimation("\r\n*It started pulling you into the darkness*");
                        

                        InitiateCombat(enemies["Shade"]);
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nDetective Zero: This is too powerful");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: I agree. Did it do any harm to you?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: I don’t feel any pain");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: That’s good");
                        
                    }
                    else if (!currentState.Flags.ContainsKey("chapter_2.2_complete"))
                    {
                        currentState.Flags["chapter_2.2_complete"] = true;
                        currentState.CompletedEvents.Add("Chapter2.2_Completed");
                        currentState.CurrentChapter++;


                        GameMenu.TypingAnimation("After an unexpected encounter, an intense argument. You have finally obtained a clue that would lead to the heart of this case.");
                        

                        Console.Clear();
                        Console.WriteLine("Chapter 2.2 Complete! Your choices will affect future events...");

                        while (true)
                        {
                            Console.WriteLine("Do you wish to go to the next chapter? Type 'y' if yes or 'n' if no");
                            string decision =Console.ReadLine().ToLower();
                            switch (decision)
                            {
                                case "y":
                                    SaveGame();
                                    ChapterTwoPointThree();
                                    break;
                                case "n":
                                    SaveGame();
                                    PlayGame();
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from 'y' or 'n' only.");
                                    continue;
                            }
                            break;
                        }
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        // This point onward lacking flags
        private void ChapterTwoPointThree()
        {
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_2.3_started"))
                    {
                        currentState.Flags["chapter_2.3_started"] = true;

                        int shadowDefeated = 0;
                        string[] symbols = { "New Moon", "Crescent Moon", "Half Moon", "Gibbous Moon", "Full Moon" };
                        int number = 0;
                        string[] direction = { "[L]eft", "[R]ight" };

                        if (currentState.Flags.ContainsKey("memory_trigger"))
                        {
                            GameMenu.TypingAnimation("*You recall some past memories about similar experiences*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Say shadow, would you care for a quick chat?");
                            

                            GameMenu.TypingAnimation("\r\nAnonymous voice: Sure thing detective but I apologize if I do not have much to talk about because you know...cloudy memory.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: That is fine shadow. I shall be the one to start then if its fine with you?");
                            

                            GameMenu.TypingAnimation("\r\nAnonymous voice: By all means, please do.");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Right then. It was during my novice days, first day on the job on being an investigator. I was given to solve a murder case.");
                            

                            Console.Clear();
                            GameMenu.TypingAnimation("*Flashback*");
                            

                            GameMenu.TypingAnimation("\r\n*Scene: A dimly lit interrogation room. Detective Hall, a rookie, sits across from Mr. Grayson, a nervous suspect in the murder of his business partner.*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Mr. Grayson, you were the last person seen with Mr. Carter before his death. Can you explain why you stayed late at the office that night?");
                            

                            GameMenu.TypingAnimation("\r\nMr. Grayson: I’ve told you—he was working on something, and I left after our meeting. I didn’t kill him!");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: But your fingerprints were found on the murder weapon—a letter opener from Carter's desk. That’s hard to explain, isn’t it?");
                                                       

                            GameMenu.TypingAnimation("\r\nMr. Grayson: I-I handled it earlier in the day when we were going over paperwork! That doesn’t mean I used it to kill him!");
                            

                            GameMenu.TypingAnimation("\r\n*Detective Zero pauses, thinking. Something about the case feels off. He glances at the autopsy report again.*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: (murmurs to himself) The wound... too precise for a sudden act of rage. Almost surgical.");
                            

                            GameMenu.TypingAnimation("\r\n*He looks up at Mr. Grayson, who appears genuinely distraught. Then it hits him—the missing piece.*");
                            

                            GameMenu.TypingAnimation("\r\nDetective Zero: Mr. Grayson, one last question: Did you know Carter was seeing a new doctor? A surgeon, perhaps?");
                                                        

                            GameMenu.TypingAnimation("\r\nMr. Grayson: A surgeon? No. Why does that matter?");

                            GameMenu.TypingAnimation("*Detective Zero stands abruptly and heads toward the door.*");
                            

                            GameMenu.TypingAnimation("(to himself) The precision of the wound... it wasn’t Grayson. It was someone who knew anatomy well.");

                            GameMenu.TypingAnimation("*He turns back to Mr. Grayson with a slight smirk.*");

                            GameMenu.TypingAnimation("Looks like your alibi just got a second chance. I think the real killer isn’t in this room—but I know where to find them.");
                            

                            GameMenu.TypingAnimation("*Plot twist: The murderer turns out to be Carter’s own private doctor, who was secretly involved in financial schemes with Carter's wife.*");
                            
                        }
                        while (shadowDefeated < 5)
                        {
                            try
                            {
                                GameMenu.TypingAnimation("\nWhich direction do you want to go?");
                                MenuControls.ShowChoices(direction);
                                string input = Console.ReadLine();

                                if (input?.ToLower() == "l" || input == "1")
                                {
                                    if (Random.Shared.Next(2) == 0) // 50% chance to encounter a shadow
                                    {
                                        GameMenu.TypingAnimation("\nA shadow appears! Prepare for battle.");
                                        InitiateCombat(enemies["Shadow Fiend"]); 
                                    }
                                    else
                                    {
                                        GameMenu.TypingAnimation("\nYou turned left and discovered nothing.");
                                    }
                                }
                                else if (input?.ToLower() == "r" || input == "2")
                                {
                                    GameMenu.TypingAnimation("\nYou turned right and discovered a faint glow.");
                                }
                                else
                                {
                                    Console.WriteLine("\nInvalid Selection. Please type 'L' or 'R'.");
                                    continue; 
                                }

                                
                                if (number < symbols.Length)
                                {
                                    Console.WriteLine($"\nYou have gained the symbol: {symbols[number]}");
                                    number++;
                                }

                                shadowDefeated++;
                            }
                            catch (Exception ex) when (ex is FormatException || ex is IndexOutOfRangeException)
                            {
                                Console.WriteLine("\nInvalid Input. Please only select from the choices provided.");
                            }
                        }
                        GameMenu.TypingAnimation("\nYou have acquired all of the moon symbols!");
                    }
                    else if (!currentState.Flags.ContainsKey("chapter_2.3_complete"))
                    {
                        currentState.Flags["chapter_2.3_complete"] = true;
                        currentState.CompletedEvents.Add("Chapter2.3_Completed");
                        currentState.CurrentChapter++;

                        GameMenu.TypingAnimation("After having acquired all the symbols, you are one step closer to the shadows' world.");
                        

                        Console.Clear();
                        Console.WriteLine("Chapter 2.3 Complete! Your choices will affect future events...");
                        Console.WriteLine("Do you wish to go to the next chapter? (y) if yes or (n) if no");

                        while (true)
                        {
                            Console.WriteLine("Do you wish to go to the next chapter? Type 'y' if yes or 'n' if no");
                            string decision = Console.ReadLine().ToLower();
                            switch (decision)
                            {
                                case "y":
                                    SaveGame();
                                    ChapterTwoPointFour();
                                    break;
                                case "n":
                                    SaveGame();
                                    PlayGame();
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose again from 'y' or 'n' only.");
                                    continue;
                            }
                            break;
                        }
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        private void ChapterTwoPointFour()
        {
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_2.4_started"))
                    {
                        currentState.Flags["chapter_2.4_started"] = true;

                        GameMenu.TypingAnimation("\r\n*You inserted all the symbols and the door unlocked.*");

                        GameMenu.TypingAnimation("\r\nDetective Zero: Finally, it’s open.");
                        

                        GameMenu.TypingAnimation("\r\nYou stepped inside and found yourself in a mystical chamber illuminated by a bright moonlight.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Finally, it’s open.");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: This is the heart of the shadows' power. Be cautious.");
                        

                        GameMenu.TypingAnimation("\r\nYou walked around the chamber, examining the walls and the symbols.");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Detective Zero: These symbols...They seem to tell a story.");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: They depict the rise of the God of Shadows and the creation of the stone.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: So this stone is connected to the God of Shadows?");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: It must be, it holds a fragment of his power.");
                        
                    }
                    else if (!currentState.Flags.ContainsKey("chapter_2.4_complete"))
                    {
                        currentState.Flags["chapter_2.4_complete"] = true;
                        currentState.CompletedEvents.Add("Chapter2.4_Completed");
                        currentState.CurrentChapter++;

                        Console.Clear();
                        Console.WriteLine("Chapter 2.4 Complete! Your choices will affect future events...");
                        Console.WriteLine("Do you wish to go to the next chapter? (y) if yes or (n) if no");

                        while (true)
                        {
                            Console.WriteLine("Do you wish to go to the next chapter? Type 'y' if yes or 'n' if no");
                            string decision = Console.ReadLine().ToLower();
                            switch (decision)
                            {
                                case "y":
                                    SaveGame();
                                    ChapterThree();
                                    break;
                                case "n":
                                    SaveGame();
                                    PlayGame();
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose again from 'y' or 'n' only.");
                                    continue;
                            }
                            break;
                        }
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

            }

        }
        private void ChapterThree()
        {
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_3_started"))
                    {
                        currentState.Flags["chapter_3_started"] = true;

                        GameMenu.TypingAnimation("\r\nYou continued your journey, guided by the shadow.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: This place is like a maze.");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: Stay focused. We’re getting closer.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: LOOK OUT!");
                        

                        InitiateCombat(enemies["Mutilated Villager"]);
                        GameMenu.TypingAnimation("\r\nAnother one huh.");
                        

                        InitiateCombat(enemies["Shade"]);
                        GameMenu.TypingAnimation("\r\nYou encountered more shadows along the way, but the stone protected you.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: This stone is incredible. It’s like a shield.");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: Use it wisely. The God of Shadows will not be pleased.\r\n");
                        
                        string[] options = { "Press on towards the Big Tree", "Take a moment to rest" };
                        MenuControls.ShowChoices(options);

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            switch (choiceString)
                            {
                                case "1":
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("\r\nYou pushed forward, determined to reach the Big Tree.");
                                    

                                    GameMenu.TypingAnimation("\r\nDetective Zero: I can see it. We’re almost there.");
                                    

                                    GameMenu.TypingAnimation("\r\nAnonymous voice: Be prepared. The God of Shadows will be waiting.");
                                    
                                    break;
                                case "2":
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("\r\nYou took a brief rest to gather your strength.");
                                    

                                    GameMenu.TypingAnimation("\r\nDetective Zero: I need to be at my best for this.");
                                    

                                    GameMenu.TypingAnimation("\r\nAnonymous voice: Rest, but stay alert. The shadows are always watching.");
                                    
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                                    continue;
                            }
                        }
                    }
                    else if (!currentState.Flags.ContainsKey("chapter_3_complete"))
                    {
                        currentState.Flags.ContainsKey("chapter_3_complete");
                        currentState.CompletedEvents.Add("Chapter3_Completed");
                        currentState.CurrentChapter++;

                        GameMenu.TypingAnimation("You approach the Big Tree, the last destination where this case will end once and for all. You steel your heart and mind for things are about to get heavy");
                        

                        while (true)
                        {
                            Console.WriteLine("Do you wish to go to the next chapter? Type 'y' if yes or 'n' if no");
                            string decision = Console.ReadLine().ToLower();
                            switch (decision)
                            {
                                case "y":
                                    SaveGame();
                                    ChapterThree();
                                    break;
                                case "n":
                                    SaveGame();
                                    PlayGame();
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from 'y' or 'n' only.");
                                    continue;
                            }                           
                            break;
                        }
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

            }
        }
        private void ChapterThreePointOne()
        {
            while (true)
            {
                try
                {
                    if (!currentState.Flags.ContainsKey("chapter_3.1_started"))
                    {
                        currentState.Flags["chapter_3.1_started"] = true;

                        GameMenu.TypingAnimation("\r\nYou reached the Big Tree, and a figure emerged from the shadows.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Who are you?");
                        

                        GameMenu.TypingAnimation("\r\nErebus: It’s me, Erebus. I’ve been waiting for you.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: Erebus? What are you doing here?");
                        

                        GameMenu.TypingAnimation("\r\nErebus: I am not just a villager. I am the God of Shadows.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: What?! You’ve been deceiving us all along?");
                        

                        Console.Clear();
                        GameMenu.TypingAnimation("Erebus: Yes, and now you’ve brought the stone to me. Once I get a hold of it, I will summon an entire army that will plunge this world into darkness!.\r\n");
                        
                        string[] options = { "Confront Erebus", "Try to reason with Erebus" };
                        MenuControls.ShowChoices(options);

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            switch (choiceString)
                            {
                                case "1":
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("\r\nDetective Zero: I won’t let you take the stone!");
                                    

                                    GameMenu.TypingAnimation("\r\nErebus: Foolish human. You cannot stop me.");
                                    

                                    InitiateCombat(enemies["Dreadlord"]);
                                    GameMenu.TypingAnimation("\r\nA fierce battle ensued, with Erebus using his shadow powers against you.");
                                    
                                    break;
                                case "2":
                                    Console.ResetColor();
                                    Console.Clear();
                                    GameMenu.TypingAnimation("\r\nDetective Zero: Erebus, you don’t have to do this. We can find another way.");
                                    

                                    GameMenu.TypingAnimation("\r\nErebus: There is no other way. The power of the shadows is my destiny.");
                                    

                                    GameMenu.TypingAnimation("\r\nDetective Zero: Please, Erebus. Think about the villagers. Think about Arthur.");
                                    

                                    GameMenu.TypingAnimation("\r\nErebus: Enough! The stone’s power will be mine!");
                                    
                                    InitiateCombat(enemies["Dreadlord"]);
                                    break;
                                default:
                                    GameMenu.TypingAnimation("\nInvalid choice. Please choose from \"1\" or \"2\" only.");
                                    continue;
                            }
                            
                            break;
                        }
                    }
                    else if (!currentState.Flags.ContainsKey("finishing_blow"))
                    {
                        currentState.Flags["finishing_blow"] = true;

                        Console.Clear();
                        GameMenu.TypingAnimation("\r\nYou channeled the stone’s power, creating a blinding light that weakened Erebus.");
                        

                        GameMenu.TypingAnimation("\r\nErebus: No! This cannot be!");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: It’s over, Erebus. The shadows will no longer harm the villagers.");
                        

                        GameMenu.TypingAnimation("\r\nErebus: You may have won this battle, but the shadows will always exist.");
                        

                        GameMenu.TypingAnimation("\r\nErebus vanished into the darkness, leaving you victorious.");
                        

                        GameMenu.TypingAnimation("\r\nDetective Zero: It’s finally over.");
                        

                        GameMenu.TypingAnimation("\r\nAnonymous voice: You’ve done well, Detective Zero. The village is safe.\r\n");
                        
                        string[] options2 = { "Return to the village", "Reflect on the case" };
                        MenuControls.ShowChoices(options2);

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); 
                            char choice = keyInfo.KeyChar;
                            string choiceString = choice.ToString();
                            if (choiceString == "1" || choiceString == "2")
                            {
                                Console.ResetColor();
                                Console.Clear();
                                string detectiveDialogue = choiceString switch
                                {
                                    "1" => "Detective Zero: Now that is done, what do you say we return to the village failed shadow? We have earned our rest.",
                                    "2" => "Detective Zero: I guess that is another case solved. The name \"Detective Zero\" still stands and I still keep my record. \"That was certainly out of the ordinary. I should probably request a vacation to the director after this\".",
                                    _ => throw new InvalidOperationException("Invalid choice detected.")
                                };
                                GameMenu.TypingAnimation(detectiveDialogue);
                                break;
                            }
                            else
                            {
                                GameMenu.TypingAnimation("\r\nInvalid choice. Please choose from \"1\", \"2\", or \"3\" only.");
                            }
                        }
                    }
                    else if (!currentState.Flags.ContainsKey("chapter_3.1_complete"))
                    {
                        currentState.Flags["chapter_3.1_complete"] = true;
                        currentState.CompletedEvents.Add("Chapter3.1_Completed");
                        currentState.CurrentChapter++;


                    }
                    else
                    {
                        break;
                    }
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

        }
    }
}