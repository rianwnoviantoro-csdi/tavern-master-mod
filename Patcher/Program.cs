using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Patcher
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("=== Tavern Master Mod Installer & Patcher ===");

            string managedDir = null;

            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                string input = args[0].Trim('\'', '"');
                if (Directory.Exists(input))
                {
                    if (File.Exists(Path.Combine(input, "Assembly-CSharp.dll")))
                    {
                        managedDir = input;
                    }
                    else if (Directory.Exists(Path.Combine(input, "Contents/Resources/Data/Managed")))
                    {
                        managedDir = Path.Combine(input, "Contents/Resources/Data/Managed");
                    }
                    else if (Directory.Exists(Path.Combine(input, "Tavern Master_Data/Managed")))
                    {
                        managedDir = Path.Combine(input, "Tavern Master_Data/Managed");
                    }
                }
                else if (File.Exists(input) && Path.GetFileName(input) == "Assembly-CSharp.dll")
                {
                    managedDir = Path.GetDirectoryName(input);
                }
            }

            if (string.IsNullOrEmpty(managedDir) && Directory.Exists("/Volumes"))
            {
                foreach (var v in Directory.GetDirectories("/Volumes"))
                {
                    string cand = Path.Combine(v, "SteamLibrary/steamapps/common/Tavern Master/TavernMaster_Mac.app/Contents/Resources/Data/Managed");
                    if (File.Exists(Path.Combine(cand, "Assembly-CSharp.dll")))
                    {
                        managedDir = cand;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(managedDir))
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string cand = Path.Combine(home, "Library/Application Support/Steam/steamapps/common/Tavern Master/TavernMaster_Mac.app/Contents/Resources/Data/Managed");
                if (File.Exists(Path.Combine(cand, "Assembly-CSharp.dll")))
                {
                    managedDir = cand;
                }
            }

            if (string.IsNullOrEmpty(managedDir))
            {
                Console.WriteLine("[ERROR] Could not automatically locate Tavern Master. Please specify the path as an argument.");
                return 1;
            }

            string targetDll = Path.Combine(managedDir, "Assembly-CSharp.dll");
            string backupDll = Path.Combine(managedDir, "Assembly-CSharp.dll.bak");
            string baseDir = AppContext.BaseDirectory;
            string modDllSrc = Path.GetFullPath(Path.Combine(baseDir, "../../../../../TavernMod/bin/Debug/netstandard2.1/TavernMod.dll"));
            if (!File.Exists(modDllSrc))
            {
                modDllSrc = "/Users/rianwnoviantoro/Dev/Experimental/tavern/TavernMod/bin/Debug/netstandard2.1/TavernMod.dll";
            }
            string modDllDest = Path.Combine(managedDir, "TavernMod.dll");

            Console.WriteLine($"[INFO] Target Managed Directory: {managedDir}");

            if (!File.Exists(targetDll))
            {
                Console.WriteLine($"[ERROR] Assembly-CSharp.dll not found: {targetDll}");
                return 1;
            }

            if (!File.Exists(modDllSrc))
            {
                Console.WriteLine($"[ERROR] TavernMod.dll not found at: {modDllSrc}");
                return 1;
            }

            // 1. Copy TavernMod.dll into Managed folder
            Console.WriteLine($"[1/4] Copying TavernMod.dll to Managed directory...");
            File.Copy(modDllSrc, modDllDest, true);
            Console.WriteLine($"      -> Copied to {modDllDest}");

            // 2. Backup Assembly-CSharp.dll if not already backed up
            if (!File.Exists(backupDll))
            {
                Console.WriteLine($"[2/4] Creating backup: Assembly-CSharp.dll.bak...");
                File.Copy(targetDll, backupDll, false);
                Console.WriteLine("      -> Backup created successfully.");
            }
            else
            {
                Console.WriteLine($"[2/4] Backup already exists at: {backupDll}");
            }

            // 3. Patch Assembly-CSharp.dll with Mono.Cecil
            Console.WriteLine("[3/4] Inspecting and patching Assembly-CSharp.dll...");
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(managedDir);

            var readerParams = new ReaderParameters
            {
                AssemblyResolver = resolver,
                ReadWrite = false
            };

            byte[] gameDllBytes = File.ReadAllBytes(targetDll);
            byte[] modDllBytes = File.ReadAllBytes(modDllSrc);
            byte[] patchedBytes = null;
            bool alreadyPatched = false;

            using (var gameMs = new MemoryStream(gameDllBytes))
            using (var modMs = new MemoryStream(modDllBytes))
            using (var gameAssembly = AssemblyDefinition.ReadAssembly(gameMs, readerParams))
            using (var modAssembly = AssemblyDefinition.ReadAssembly(modMs, readerParams))
            {
                var modInitMethod = modAssembly.MainModule
                    .GetType("TavernMod.ModMenu")?
                    .Methods.FirstOrDefault(m => m.Name == "Init");

                if (modInitMethod == null)
                {
                    Console.WriteLine("[ERROR] Could not find TavernMod.ModMenu.Init method in TavernMod.dll");
                    return 1;
                }

                // Inject into MainMenuController.Start
                bool mmPatched = true;
                var mainMenuType = gameAssembly.MainModule.GetType("MainMenuController");
                if (mainMenuType != null)
                {
                    var mmStartMethod = mainMenuType.Methods.FirstOrDefault(m => m.Name == "Start");
                    if (mmStartMethod != null)
                    {
                        mmPatched = mmStartMethod.Body.Instructions.Any(inst =>
                            inst.OpCode == OpCodes.Call &&
                            inst.Operand is MethodReference mr &&
                            mr.DeclaringType.FullName == "TavernMod.ModMenu");

                        if (!mmPatched)
                        {
                            Console.WriteLine("      -> Injecting TavernMod.ModMenu::Init() into MainMenuController.Start()...");
                            var importedMmInitMethod = gameAssembly.MainModule.ImportReference(modInitMethod);
                            var il = mmStartMethod.Body.GetILProcessor();
                            il.InsertBefore(mmStartMethod.Body.Instructions[0], Instruction.Create(OpCodes.Call, importedMmInitMethod));
                            alreadyPatched = false;
                        }
                    }
                }

                var cheatCodesType = gameAssembly.MainModule.GetType("CheatCodes");
                if (cheatCodesType == null)
                {
                    Console.WriteLine("[ERROR] Could not find CheatCodes type in Assembly-CSharp.dll");
                    return 1;
                }

                var startMethod = cheatCodesType.Methods.FirstOrDefault(m => m.Name == "Start");
                if (startMethod == null)
                {
                    Console.WriteLine("[ERROR] Could not find CheatCodes.Start method in Assembly-CSharp.dll");
                    return 1;
                }

                // Check if already patched in CheatCodes
                bool ccPatched = startMethod.Body.Instructions.Any(inst =>
                    inst.OpCode == OpCodes.Call &&
                    inst.Operand is MethodReference mr &&
                    mr.DeclaringType.FullName == "TavernMod.ModMenu");

                if (!ccPatched)
                {
                    Console.WriteLine("      -> Injecting TavernMod.ModMenu::Init() into CheatCodes.Start()...");
                    var importedInitMethod = gameAssembly.MainModule.ImportReference(modInitMethod);
                    var il = startMethod.Body.GetILProcessor();
                    il.InsertBefore(startMethod.Body.Instructions[0], Instruction.Create(OpCodes.Call, importedInitMethod));
                    alreadyPatched = false;
                }

                bool needsSave = (!mmPatched) || (!ccPatched);
                if (needsSave)
                {
                    using (var outMs = new MemoryStream())
                    {
                        gameAssembly.Write(outMs);
                        patchedBytes = outMs.ToArray();
                    }
                }
                else
                {
                    Console.WriteLine("      -> Both MainMenuController and CheatCodes are already patched.");
                }
            }

            if (patchedBytes != null)
            {
                File.WriteAllBytes(targetDll, patchedBytes);
                Console.WriteLine("      -> Saved patched Assembly-CSharp.dll successfully.");
            }

            Console.WriteLine("[4/4] Installation Complete!");
            Console.WriteLine("=================================================");
            Console.WriteLine(" Mod Menu berhasil terpasang di Tavern Master!");
            Console.WriteLine(" Buka game, tekan F1 atau klik tombol [⚡ TAVERN MOD] di layar!");
            Console.WriteLine("=================================================");
            return 0;
        }
    }
}
