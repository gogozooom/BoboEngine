using System.Reflection;

namespace ConsoleCommand
{
    /// <summary>
    /// Automatically creates a command to be used in the console
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    class CommandAttribute : Attribute
    {
        public string command;
        public string description;
        public bool combineParams;

        /// <param name="command">The command to use</param>
        /// <param name="description">The command's description in 'help'</param>
        public CommandAttribute(string command, string description = "[NULL]", bool combineParams = false)
        {
            this.command = command;
            this.description = description;
            this.combineParams = combineParams;
        }
    }

    public static class ConsoleCmd
    {
        // -- Public --
        public static bool debugTraceEnabled = false;

        public static bool setupDone { get; private set; }
        public static List<CommandInfo> Commands { get; private set; }

        // -- Private --

        public static void Initialize()
        {
            if (setupDone)
            {
                return;
            }

            var methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass)
                .SelectMany(x => x.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(CommandAttribute), false).FirstOrDefault() != null);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<CommandAttribute>();

                //Program.Log($"Registering command '{attr.command}' with description '{attr.description}' with method '{method}'");

                CreateCommand(attr.command, attr.description, method, attr.combineParams);
            }

            Program.ConsoleInputted += HandleConsoleInput;


            Console.WriteLine("""
                   ______                  ______                       __   
                  / ____/___  ____ _____  / ____/___  ____  _________  / /__ 
                 / / __/ __ \/ __ `/ __ \/ /   / __ \/ __ \/ ___/ __ \/ / _ \
                / /_/ / /_/ / /_/ / /_/ / /___/ /_/ / / / (__  ) /_/ / /  __/
                \____/\____/\__, /\____/\____/\____/_/ /_/____/\____/_/\___/ 
                           /____/   
                
                Type 'help' for options...
                """);

            setupDone = true;
        }
        
        // -- Inputs --
        static void HandleConsoleInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            List<string> args = input.Split(' ').ToList();
            string commandT = args[0];
            args.RemoveAt(0);

            if (Commands == null)
            {
                Program.LogWarning($"[Warning] No Commands Exist!");
                return;
            }

            CommandInfo commandFound = Commands.Find(c => c.command == commandT);

            if (commandFound == null)
            {
                Program.LogWarning($"[Warning] Command '{commandT}' not found!");
                return;
            }

            if (debugTraceEnabled)
            {
                commandFound.Invoke(args);
            }
            else
            {
                try
                {
                    commandFound.Invoke(args);
                }
                catch (Exception e)
                {
                    Program.LogError($"[ERROR] Failed to invoke command '{commandFound.command}'!");
                    Program.LogError($"Error: {e.Message}");
                }
            }
        }

        // -- Public --

        public static bool CreateCommand(string command, string description, MethodInfo method, bool combineParams = false)
        {
            if (Commands == null) Commands = new();

            if (Commands.Any(c => c.command == command))
            {
                Program.LogWarning($"[Warning] Command '{command}' already exists!");
                return false;
            }

            Commands.Add(new(command, description, method, combineParams));
            return true;
        }
        public static bool ParseBool(string input)
        {
            string formattedInput = input.ToLower();

            if (formattedInput == "t" || formattedInput == "true" || formattedInput == "1")
            {
                return true;
            }
            else if (formattedInput == "f" || formattedInput == "false" || formattedInput == "0")
            {
                return false;
            }

            throw new ArgumentException($"Could not parse {input}");
        }
    }
    static class BaseCommands
    {
        [Command("help", "Opens this help menu!")]
        public static void Help()
        {
            ConsoleCmd.Commands.Sort();

            Program.LogLine();

            Type lastType = null;

            foreach (var command in ConsoleCmd.Commands)
            {
                Type type = command.method.DeclaringType;

                if (lastType != type)
                {
                    Program.LogMessage($"  [{type}]");
                    lastType = type;
                }

                Help(command);
            }
        }
        public static void Help(CommandInfo cmd)
        {
            Program.LogMessage($"    - {cmd.command} - {cmd.description}");
        }
        [Command("debug", "Enables more detailed stack trace when executing commands")]
        public static void ToggleDebug(string input)
        {
            bool valueToToggleTo = !ConsoleCmd.debugTraceEnabled;

            if (!string.IsNullOrEmpty(input))
            {
                valueToToggleTo = ConsoleCmd.ParseBool(input);
            }

            ConsoleCmd.debugTraceEnabled = valueToToggleTo;

            Program.LogMessage($"debugTraceEnabled is now set to '{valueToToggleTo}'");
        }
    }
    public class CommandInfo : IComparable<CommandInfo>
    {
        public string command;
        public string description;
        public MethodInfo method;
        public bool combineParams;

        public CommandInfo(string command, string description, MethodInfo method, bool combineParams = false)
        {
            this.command = command;
            this.description = description;
            this.method = method;
            this.combineParams = combineParams;
        }

        public int CompareTo(CommandInfo other)
        {
            string a = method.DeclaringType.Name + " " + command;
            string b = other.method.DeclaringType.Name + " " + other.command;

            return a.CompareTo(b);
        }
        public bool Invoke(List<string> args)
        {
            // Help Check
            if (args.Count == 1)
            {
                if (args[0].ToLower() == "help" || args[0].ToLower() == "?")
                {
                    BaseCommands.Help(this);
                    return true;
                }
            }

            List<object> parameters = new();

            ParameterInfo[] methodParams = method.GetParameters();

            if (combineParams && methodParams.Length > 0)
            {
                parameters.Add(string.Join(" ", args));
            }
            else
            {
                for (int i = 0; i < methodParams.Length; i++)
                {
                    if (args.Count > i)
                    {
                        object value = Convert.ChangeType(args[i], methodParams[i].ParameterType);

                        if (value == null)
                        {
                            throw new ArgumentException($"Parameter '{args[i]}' should be type '{methodParams[i].ParameterType}' but failed to convert!");
                        }

                        parameters.Add(value);
                    }
                    else
                    {
                        parameters.Add(null);
                    }
                }
            }

            if (method.IsStatic)
            {
                method.Invoke(null, parameters.ToArray());
            }
            else
            {
                throw new NotImplementedException();
            }

            return true;
        }
    }
}