namespace Chip8.Utils;

 class RomReader
{
        public static bool SelectRomFile(out CPU cpu)
        {
            string? filePath = "";

            filePath = DisplayRomFileOptions(filePath);
            cpu = new CPU();
            cpu.Initialize();
            
            if (DisplayRomFileSelectionResponse(cpu, filePath)) return true;

            return false;
        }

        public static string DisplayRomFileOptions(string filePath)
        {
            Console.WriteLine("Pick which ROM you'd like to load from the list available below. ");
            Console.WriteLine("1. for an IBM logo, this is going to make a million dollars. ");
            Console.WriteLine("2. for a Maze demo. ");
            Console.WriteLine("3. for a Space Invaders thing ");
            Console.WriteLine("4. Test");
            Console.WriteLine("5. Test2");
            Console.WriteLine("6. Chip8 SplashScreen Test");

            if (int.TryParse(Console.ReadLine(), out var option))
            {
                switch (option)
                {
                    case 1:
                        filePath = @"Roms\IBMLogo.ch8";
                        break;
                    case 2:
                        filePath = @"Roms\Maze.ch8";
                        break;
                    case 3:
                        filePath = @"Roms\INVADERS";
                        break;
                    case 4:
                        filePath = @"Roms\TEST";
                        break;
                    case 5:
                        filePath = @"Roms\TEST2";
                        break;
                    case 6:
                        filePath = @"Roms\1-chip8-logo.ch8";
                        break;
                    default:
                        Console.WriteLine("Select an actual option");
                        break;
                }
            }

            return filePath;
        }
        
        
        
        public static bool DisplayRomFileSelectionResponse(CPU cpu, string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("The specified ROM file does not exist.");
                return true;
            }

            Console.WriteLine($"Loading ROM: {filePath}");
            try
            {
                LoadRomIntoMemory(cpu, filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load ROM: {e.Message}");
                return true;
            }

            return false;
        }
        
        public static void LoadRomIntoMemory(CPU cpu, string filePath)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
            {
                int startAddress = 0x200;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    cpu.RAM[startAddress++] = reader.ReadByte();
                }
            }
        }
}