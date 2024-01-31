namespace Chip8
{ 
    class Program
    {
        static void Main(string[] args)
        {
            string? filePath = "";
            
            Console.WriteLine("Pick which ROM you'd like to load from the list available below. ");
            Console.WriteLine("1. for Landing");
            Console.WriteLine("2. for Guess");
            Console.WriteLine("3. for an IBM logo, this is going to make a million dollars. ");

            if (int.TryParse(Console.ReadLine(), out var option))
            {
                switch (option)
                {
                    case 1:
                         filePath = "D:\\Source\\Chip8\\Chip8\\bin\\Debug\\Landing.ch8";
                        break;
                    case 2:
                         filePath = "D:\\Source\\Chip8\\Chip8\\bin\\Debug\\Guess.ch8";
                        break;
                    case 3:
                        filePath = "D:\\Source\\Chip8\\Chip8\\bin\\Debug\\IBMLogo.ch8";
                        break;
                    default:
                        Console.WriteLine("Select an actual option");
                        return;
                }
            }
            
            List<string> GetRomNames()
            {
                var delimiter = "\\"; // depends on operating system
                var files = Directory.GetFiles("Roms");
                var roms = new List<string>();
                foreach (var file in files)
                {
                    // rootDir\\Roms\\RomName into rootDir, Roms, RomName
                    var filePathParts = file.Split(delimiter);
                    var romName = filePathParts.Last();
                    roms.Add(romName);
                }

                return roms;
            }
            
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var opcode = reader.ReadUInt16();
                    Console.WriteLine($"{opcode.ToString($"X")}");
                    DisassembleOpcode(opcode);
                }
            }

            GetRomNames();
            ConvertToBytes();
        }
        
        static void ConvertToBytes()
        {
            Console.WriteLine("Enter your Op Code (e.g 0x8034) ");
            var opCode = Console.ReadLine();
            
            Console.WriteLine($"You have entered Op Code {opCode}.");
            var hexBytes = opCode?.Replace("0x", "");
            int intValue = int.Parse(hexBytes!, System.Globalization.NumberStyles.HexNumber);
            
            var lsb = intValue & 0x00FF;
            Console.WriteLine($"The least significant byte is 0x{lsb:X2}.");
            
            var msb = (intValue & 0xFF00) >> 8;
            Console.WriteLine($"The most significant byte is 0x{msb:X2}");
        }
        
        static void DisassembleOpcode(ushort opcode)
        {
            var firstNibble = (opcode & 0xF000) >> 12;

            switch (firstNibble)
            {
                case 0x0:
                {
                    // starting with 0x0
                    // Example: 0x00E0 - Clear the screen
                    // Example: 0x00EE - Return from a subroutine
                    Console.WriteLine("Clear the screen or Return from a subroutine");
                    break;
                }
                case 0x1:
                {
                    // Example: 0x1000 - Jump to address
                    var address = opcode & 0x0FFF;
                    Console.WriteLine($"Jump to address 0x{address:X3}");
                    break;
                }
                    // finish other opcodes here
                    default:
                    Console.WriteLine($"Unknown opcode: 0x{opcode:X4}");
                    break;
            }
        }
    }
}