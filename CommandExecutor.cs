using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SCON
{
    public static class CommandExecutor
    {
        private static Type consoleWindowType;
        private static MethodInfo submitMethod;
        private static bool initialized = false;

        private static void Initialize()
        {
            if (initialized) return;

            try
            {
                // Find the ConsoleWindow class in Assembly-CSharp
                var assemblyCSharp = Assembly.Load("Assembly-CSharp");
                
                // Try common class names for the console in Stationeers
                consoleWindowType = assemblyCSharp.GetType("Assets.Scripts.UI.ConsoleWindow") 
                    ?? assemblyCSharp.GetType("ConsoleWindow")
                    ?? assemblyCSharp.GetType("DebugConsole")
                    ?? assemblyCSharp.GetType("DeveloperConsole");

                if (consoleWindowType == null)
                {
                    Plugin.Log.LogWarning("Could not find console type. Searching all types...");
                    SearchForConsoleType(assemblyCSharp);
                }

                if (consoleWindowType != null)
                {
                    // Look for methods that might execute commands
                    // Be more specific to avoid ambiguous matches
                    submitMethod = consoleWindowType.GetMethod("Submit", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null)
                        ?? consoleWindowType.GetMethod("Submit", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null)
                        ?? consoleWindowType.GetMethod("ExecuteCommand", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null)
                        ?? consoleWindowType.GetMethod("ExecuteCommand", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null)
                        ?? consoleWindowType.GetMethod("RunCommand", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null)
                        ?? consoleWindowType.GetMethod("RunCommand", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null)
                        ?? consoleWindowType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null)
                        ?? consoleWindowType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);

                    if (submitMethod != null)
                    {
                        Plugin.Log.LogInfo($"Found console method: {consoleWindowType.FullName}.{submitMethod.Name}");
                        initialized = true;
                    }
                    else
                    {
                        Plugin.Log.LogWarning($"Found console type {consoleWindowType.FullName} but no suitable execution method");
                        LogAvailableMethods();
                    }
                }
                else
                {
                    Plugin.Log.LogError("Could not find console type in Assembly-CSharp");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error initializing CommandExecutor: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void SearchForConsoleType(Assembly assembly)
        {
            // Search for console types
            var consoleTypes = new System.Collections.Generic.List<Type>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.FullName.ToLower().Contains("console"))
                {
                    consoleTypes.Add(type);
                }
            }
            
            // Look for DeveloperConsole or ConsoleWindow specifically
            foreach (var type in consoleTypes)
            {
                var typeName = type.Name.ToLower();
                var fullName = type.FullName.ToLower();
                
                // Skip message and command types
                if (fullName.Contains("message") || fullName.Contains("command"))
                    continue;
                
                // Look for developer console or console window
                if (typeName == "developerconsole" || typeName == "consolewindow" || typeName == "console")
                {
                    consoleWindowType = type;
                    break;
                }
            }
        }

        private static void LogAvailableMethods()
        {
            if (consoleWindowType == null) return;

            Plugin.Log.LogInfo($"Available methods in {consoleWindowType.Name}:");
            foreach (var method in consoleWindowType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                var parameters = string.Join(", ", Array.ConvertAll(method.GetParameters(), p => $"{p.ParameterType.Name} {p.Name}"));
                Plugin.Log.LogInfo($"  {method.Name}({parameters})");
            }
        }

        public static void ExecuteCommand(string command)
        {
            if (!initialized)
            {
                Initialize();
            }

            if (!initialized || submitMethod == null)
            {
                Plugin.Log.LogError("Command execution not available - console system not found");
                TryAlternativeExecution(command);
                return;
            }

            try
            {
                // Try to find console instance
                object consoleInstance = null;

                if (!submitMethod.IsStatic)
                {
                    // Look for Instance property or field
                    var instanceProperty = consoleWindowType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    var instanceField = consoleWindowType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);

                    if (instanceProperty != null)
                    {
                        consoleInstance = instanceProperty.GetValue(null);
                    }
                    else if (instanceField != null)
                    {
                        consoleInstance = instanceField.GetValue(null);
                    }
                    else
                    {
                        // Try to find instance via FindObjectOfType
                        consoleInstance = UnityEngine.Object.FindObjectOfType(consoleWindowType);
                    }

                    if (consoleInstance == null)
                    {
                        Plugin.Log.LogWarning("Console instance not found, trying static execution");
                        TryAlternativeExecution(command);
                        return;
                    }
                }

                // Execute the command
                var parameters = submitMethod.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                {
                    submitMethod.Invoke(consoleInstance, new object[] { command });
                    Plugin.Log.LogInfo($"Command executed: {command}");
                }
                else
                {
                    Plugin.Log.LogError($"Unexpected method signature for {submitMethod.Name}");
                    TryAlternativeExecution(command);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error executing command: {ex.Message}\n{ex.StackTrace}");
                TryAlternativeExecution(command);
            }
        }

        private static void TryAlternativeExecution(string command)
        {
            try
            {
                // Alternative: Try to use Unity's SendMessage system
                var console = GameObject.Find("Console");
                if (console != null)
                {
                    console.SendMessage("Submit", command, SendMessageOptions.DontRequireReceiver);
                    Plugin.Log.LogInfo($"Command sent via SendMessage: {command}");
                    return;
                }

                // Alternative: Parse and execute directly if we can find command handlers
                Plugin.Log.LogWarning($"Could not execute command through standard methods: {command}");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Alternative execution also failed: {ex.Message}");
            }
        }
    }
}
