using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

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
            Console.WriteLine("5. for a Space Invaders thing ");

            if (int.TryParse(Console.ReadLine(), out var option))
            {
                switch (option)
                {
                    case 1:
                        filePath = @"Roms\Landing.ch8";
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
                    case 5:
                        filePath = @"Roms\INVADERS";
                        break;
                    default:
                        Console.WriteLine("Select an actual option");
                        break;
                }
            }
            var cpu = new CPU();
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                int startAddress = 0x200;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    cpu.RAM[startAddress++] = reader.ReadByte();
                }
            }
            cpu.PC = 0x200; 
            
            while (true)
            {
                ExecuteOpcode(cpu);

                if (cpu.IsDirty)
                {
                    cpu.RenderDisplay();
                    cpu.IsDirty = false;
                }

                Thread.Sleep(1000 / 60);
            }
        }

        static void ExecuteOpcode(CPU cpu)
        {
            var highByte = cpu.RAM[cpu.PC];
            var lowByte = cpu.RAM[cpu.PC + 1];

            ushort opcode = (ushort)(highByte << 8 | lowByte);

            switch (opcode)
            {
                case 0x00E0:
                    // CLS
                    cpu.ClearScreen_00E0(opcode);
                    break;
                case 0x00EE:
                    // RET
                    cpu.ReturnFromSubroutine_00EE(opcode);
                    break;
                default:
                    break;
            }

            switch (opcode & 0xF000)
            {
                case 0x0000:
                    // System
                    break;
                case 0x1000:
                    // JP addr
                    cpu.JumpToAddress_1NNN(opcode);
                    break;
                case 0x2000:
                    // CALL addr
                    cpu.CallSubroutine_2NNN(opcode);
                    break;
                case 0x3000:
                    // SE Vx, byte
                    cpu.SkipIfVxEqualsByte_3XKK(opcode);
                    break;
                case 0x4000:
                    // SNE Vx, byte
                    cpu.SkipIfVxNotEqualsByte_4XKK(opcode);
                    break;
                case 0x5000:
                    // SE Vx, Vy
                    cpu.SkipIfVxEqualsVy_5XY0(opcode);
                    break;
                case 0x6000:
                    // LD Vx, byte
                    cpu.SetVxToByte_6XKK(opcode);
                    break;
                case 0x7000:
                    // ADD Vx, byte
                    cpu.AddByteToVx_7XKK(opcode);
                    break;
                case 0x8000:
                    // math.lame
                    switch (opcode & 0x000F)
                    {
                        case 0x0:
                            // LD Vx, Vy
                            cpu.SetVxToVy_8XY0(opcode);
                            break;
                        case 0x1:
                            // OR Vx, Vy
                            cpu.SetVxToVxOrVy_8XY1(opcode);
                            break;
                        case 0x2:
                            // AND Vx, Vy
                            cpu.SetVxToVxAndVy_8XY2(opcode);
                            break;
                        case 0x3:
                            // XOR Vx, Vy
                            cpu.SetVxToVxXorVy_8XY3(opcode);
                            break;
                        case 0x4:
                            // ADD Vx, Vy
                            cpu.AddVyToVx_8XY4(opcode);
                            break;
                        case 0x5:
                            // SUB Vx, Vy
                            cpu.SubtractVyFromVx_8XY5(opcode);
                            break;
                        case 0x6:
                            // SHR Vx {, Vy}
                            cpu.ShiftVxRight_8XY6(opcode);
                            break;
                        case 0x7:
                            // SUBN Vx, Vy
                            cpu.SetVxToVyMinusVx_8XY7(opcode);
                            break;
                        case 0xE:
                            // SHL Vx {, Vy}
                            cpu.ShiftVxLeft_8XYE(opcode);
                            break;
                    }

                    break;
                case 0x9000:
                    // SNE Vx, Vy
                    cpu.SkipIfVxNotEqualsVy_9XY0(opcode);
                    break;
                case 0xA000:
                    // LD I, addr
                    cpu.SetIToAddress_ANNN(opcode);
                    break;
                case 0xB000:
                    // JP V0, addr
                    cpu.JumpToAddressPlusV0_BNNN(opcode);
                    break;
                case 0xC000:
                    // RND Vx, byte
                    cpu.SetVxToRandomByteAnd_CNNN(opcode);
                    break;
                case 0xD000:
                    // DRW Vx, Vy, nibble
                    cpu.DrawSprite_DXYN(opcode);
                    break;
                case 0xE000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x9E:
                            // SKP Vx
                            cpu.SkipIfKeyIsPressed_EX9E(opcode);
                            break;
                        case 0xA1:
                            // SKNP Vx
                            cpu.SkipIfKeyIsNotPressed_EXA1(opcode);
                            break;
                    }
                    break;
                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x07:
                            cpu.SetVxToDelayTimer_FX07(opcode);
                            break;
                        case 0x0A:
                            cpu.StoreKeyPressValueToVx_FX0A(opcode);
                            break;
                        case 0x15:
                            cpu.SetDelayTimerToVx_FX15(opcode);
                            break;
                        case 0x18:
                            cpu.SetSoundTimerToVx_FX18(opcode);
                            break;
                        case 0x1E:
                            cpu.AddVxToI_FX1E(opcode);
                            break;
                        case 0x29:
                            cpu.SetIToSpriteLocationForVx_FX29(opcode);
                            break;
                        case 0x33:
                            cpu.StoreBCDOfVxAtI_FX33(opcode);
                            break;
                        case 0x55:
                            cpu.StoreV0ToVxInMemoryStartingAtI_FX55(opcode);
                            break;
                        case 0x65:
                            cpu.FillV0ToVxWithValuesFromMemoryStartingAtI_FX65(opcode);
                            break;
                    }
                    break;
                default:
                    cpu.HandleUnrecognizedOpcode(cpu, opcode);
                    break;
            }
        }
    }
}