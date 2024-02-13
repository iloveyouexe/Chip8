using System;
using System.IO;
using System.Collections.Generic;

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
            Console.WriteLine("4. for a Maze demo. ");

            if (int.TryParse(Console.ReadLine(), out var option))
            {
                switch (option)
                {
                    case 1:
                        filePath = @"Roms\Landing.ch8";
                        ;
                        break;
                    case 2:
                        filePath = @"Roms\Guess";
                        break;
                    case 3:
                        filePath = @"Roms\IBMLogo.ch8";
                        break;
                    case 4:
                        filePath = @"Roms\Maze.ch8";
                        break;
                    default:
                        Console.WriteLine("Select an actual option");
                        break;
                }
            }
            
            var cpu = new CPU();
            cpu.PC = 0x200; // start loading stuff into ram
            
            
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    cpu.RAM[cpu.PC] = reader.ReadByte();
                    cpu.PC++;
                    
                    // Console.WriteLine($"the opcode is {opcode:X4}");
                    // ExecuteOpcode(opcode, cpu);
                }
            }
            
            cpu.PC = 0x200; // returns to beginning of program
            while (true)
            {
                ExecuteOpcode(cpu);
            }
        }

        //http://devernay.free.fr/hacks/chip8/C8TECH10.HTM

        static void ExecuteOpcode(CPU cpu)
        {
            var highByte = cpu.RAM[cpu.PC];
            var lowByte = cpu.RAM[cpu.PC+1];
            
            ushort opcode = (ushort)(highByte << 8 | lowByte);
            switch (opcode)
            {
                case 0x00E0:
                {
                    cpu.ClearScreen(opcode);
                    break;
                }
                case 0x00EE:
                {
                    cpu.ReturnFromSubroutine(opcode);
                    break;
                }
            }

            switch (opcode & 0xF000)
            {
                case 0x1000:
                    Console.WriteLine($"{opcode:X4} JP ${(opcode & 0x0FFF):X3}");
                    break;
                case 0x2000:
                    Console.WriteLine($"{opcode:X4} CALL ${(opcode & 0x0FFF):X3}");
                    cpu.CallSubroutine(opcode);
                    break;
                case 0x3000:
                    Console.WriteLine($"{opcode:X4} SE V{(opcode & 0x0F00) >> 8:X1}, ${(opcode & 0x00FF):X2}");
                    break;
                case 0x4000:
                    Console.WriteLine($"{opcode:X4} SNE V{(opcode & 0x0F00) >> 8:X1}, ${(opcode & 0x00FF):X2}");
                    break;
                case 0x5000:
                    Console.WriteLine($"{opcode:X4} SE V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                    break;
                case 0x6000:
                    Console.WriteLine($"{opcode:X4} LD V{(opcode & 0x0F00) >> 8:X1}, ${(opcode & 0x00FF):X2}");
                    break;
                case 0x7000:
                    Console.WriteLine($"{opcode:X4} ADD V{(opcode & 0x0F00) >> 8:X1}, ${(opcode & 0x00FF):X2}");
                    break;
                case 0x8000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0:
                            Console.WriteLine(
                                $"{opcode:X4} LD V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                            break;
                        case 0x1:
                            Console.WriteLine(
                                $"{opcode:X4} OR V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                            break;
                        case 0x2:
                            Console.WriteLine(
                                $"{opcode:X4} AND V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                            break;
                        case 0x3:
                            Console.WriteLine(
                                $"{opcode:X4} XOR V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                            break;
                        case 0x4:
                            Console.WriteLine(
                                $"{opcode:X4} ADD V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                            break;
                        case 0x5:
                            Console.WriteLine(
                                $"{opcode:X4} SUB V{(opcode & 0x0F00) >> 8:X1}, V{(opcode & 0x00F0) >> 4:X1}");
                            break;
                        case 0x6:
                            Console.WriteLine(
                                $"{opcode:X4} SHR V{(opcode & 0x0F00) >> 8:X1} {{, V{(opcode & 0x00F0) >> 4:X1}}}");
                            break;
                    }
                    break;
            }

            cpu.PC += 2; // remove later, prevents being stuck in a loop 
        }
    }
}