﻿namespace Chip8
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
                         filePath = "D:\\Source\\Chip8\\Chip8\\bin\\Debug\\Guess";
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
                    var highByte = reader.ReadByte();
                    var lowByte = reader.ReadByte();
                    ushort opcode = (ushort)(highByte << 8 | lowByte);
                    // Console.WriteLine($"{opcode.ToString($"X")}");
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
            switch (opcode)
            {
                case 0x00E0:
                {
                    Console.WriteLine($"{opcode:X4} CLS");
                    break;
                }
                case 0x00EE:
                {
                    Console.WriteLine($"{opcode:X4} RET");
                    break;
                }
            }

            switch (opcode & 0xF00F)
            {
                case 0x1000:
                    Console.WriteLine($"{opcode:X4} JP {(opcode & 0x0FFF):X3}");
                    break;
                case 0x2000:
                    Console.WriteLine($"{opcode:X4} CALL {(opcode & 0x0FFF):X3}");
                    break;
                case 0x3000:
                    Console.WriteLine($"{opcode:X4} SE V{(opcode & 0x0F00) >> 8:X1}, {(opcode & 0x00FF):X2}");
                    break;
                case 0x4000:
                    Console.WriteLine($"{opcode:X4} SNE V{(opcode & 0x0F00) >> 8:X1}, {(opcode & 0x00FF):X2}");
                    break;
                case 0x5000:
                    Console.WriteLine($"{opcode:X4} SE V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                    break;
                case 0x6000:
                    Console.WriteLine($"{opcode:X4} LD V{(opcode & 0x0F00) >> 8:X1}, {(opcode & 0x00FF):X2}");
                    break;
                case 0x7000:
                    Console.WriteLine($"{opcode:X4} ADD V{(opcode & 0x0F00) >> 8:X1}, {(opcode & 0x00FF):X2}");
                    break;
                case 0x8000:
                switch (opcode & 0x000F)
                {
                    case 0x0:
                        Console.WriteLine($"{opcode:X4} LD V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                        break;
                    case 0x1:
                        Console.WriteLine($"{opcode:X4} OR V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                        break;
                    case 0x2:
                        Console.WriteLine($"{opcode:X4} AND V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                        break;
                    case 0x3:
                        Console.WriteLine($"{opcode:X4} XOR V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                        break;
                    case 0x4:
                        Console.WriteLine($"{opcode:X4} ADD V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                        break;
                    case 0x5:
                        Console.WriteLine($"{opcode:X4} SUB V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                        break;
                    case 0x6:
                        Console.WriteLine($"{opcode:X4} SHR V{(opcode & 0x0F00) >> 8:X1} {{, V{(opcode & 0x00F0) >> 4:X1}}}");
                        break;
                    case 0x7:
                        Console.WriteLine($"{opcode:X4} SUBN V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                        break;
                    case 0xE:
                        Console.WriteLine($"{opcode:X4} SHL V{(opcode & 0x0F00) >> 8:X1} {{, V{(opcode & 0x00F0) >> 4:X1}}}");
                        break;
                    default:
                        Console.WriteLine($"Unknown opcode: 0x{opcode:X4}");
                        break;
                }
                break;
            }
        }
    }
}