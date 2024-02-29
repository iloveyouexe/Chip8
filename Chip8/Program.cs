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
                }
            }
            
            cpu.PC = 0x200; // returns to beginning of program
            while (true)
            {
                ExecuteOpcode(cpu);
                cpu.RenderDisplay();
            }
        }

        //http://devernay.free.fr/hacks/chip8/C8TECH10.HTM

       static void ExecuteOpcode(CPU cpu)
{
    var highByte = cpu.RAM[cpu.PC];
    var lowByte = cpu.RAM[cpu.PC + 1];

    ushort opcode = (ushort)(highByte << 8 | lowByte);
    switch (opcode)
    {
        case 0x00E0:
            // CLS
            cpu.ClearScreen(opcode);
            break;
        case 0x00EE:
            // RET
            cpu.ReturnFromSubroutine(opcode);
            break;
        // Add other specific opcodes here if necessary
    }

    switch (opcode & 0xF000)
    {
        case 0x0000:
            // System
            break;
        case 0x1000:
            // JP addr
            cpu.JumpToAddress(opcode);
            break;
        case 0x2000:
            // CALL addr
            cpu.CallSubroutine(opcode);
            break;
        case 0x3000:
            // SE Vx, byte
            cpu.SkipIfVxEqualsByte(opcode);
            break;
        case 0x4000:
            // SNE Vx, byte
            cpu.SkipIfVxNotEqualsByte(opcode);
            break;
        case 0x5000:
            // SE Vx, Vy
            cpu.SkipIfVxEqualsVy(opcode);
            break;
        case 0x6000:
            // LD Vx, byte
            cpu.SetVxToByte(opcode);
            break;
        case 0x7000:
            // ADD Vx, byte
            cpu.AddByteToVx(opcode);
            break;
        case 0x8000:
            // math.lame
            switch (opcode & 0x000F)
            {
                case 0x0:
                    // LD Vx, Vy
                    cpu.SetVxToVy(opcode);
                    break;
                case 0x1:
                    // OR Vx, Vy
                    cpu.SetVxToVxOrVy(opcode);
                    break;
                case 0x2:
                    // AND Vx, Vy
                    cpu.SetVxToVxAndVy(opcode);
                    break;
                case 0x3:
                    // XOR Vx, Vy
                    cpu.SetVxToVxXorVy(opcode);
                    break;
                case 0x4:
                    // ADD Vx, Vy
                    cpu.AddVyToVx(opcode);
                    break;
                case 0x5:
                    // SUB Vx, Vy
                    cpu.SubtractVyFromVx(opcode);
                    break;
                case 0x6:
                    // SHR Vx {, Vy}
                    cpu.ShiftVxRight(opcode);
                    break;
                case 0x7:
                    // SUBN Vx, Vy
                    cpu.SetVxToVyMinusVx(opcode);
                    break;
                case 0xE:
                    // SHL Vx {, Vy}
                    cpu.ShiftVxLeft(opcode);
                    break;
            }
            break;
        case 0x9000:
            // SNE Vx, Vy
            cpu.SkipIfVxNotEqualsVy(opcode);
            break;
        case 0xA000:
            // LD I, addr
            cpu.SetIToAddress(opcode);
            break;
        case 0xB000:
            // JP V0, addr
            cpu.JumpToAddressPlusV0(opcode);
            break;
        case 0xC000:
            // RND Vx, byte
            cpu.SetVxToRandomByteAnd(opcode);
            break;
        case 0xD000:
            // DRW Vx, Vy, nibble
            cpu.DrawSprite(opcode);
            break;
        case 0xE000:
            switch (opcode & 0x00FF)
            {
                case 0x9E:
                    // SKP Vx
                    cpu.SkipIfKeyIsPressed(opcode);
                    break;
                case 0xA1:
                    // SKNP Vx
                    cpu.SkipIfKeyIsNotPressed(opcode);
                    break;
            }
            break;
        case 0xF000:
            switch (opcode & 0x00FF)
            {
                case 0x07:
                    cpu.SetVxToDelayTimer(opcode);
                    break;
                case 0x0A:
                    cpu.SkipIfKeyIsNotPressed(opcode);
                    break;
                case 0x15:
                    cpu.SetDelayTimerToVx(opcode);
                    break;
                case 0x18:
                    cpu.SetSoundTimerToVx(opcode);
                    break;
                case 0x1E:
                    cpu.AddVxToI(opcode);
                    break;
                case 0x29:
                    cpu.SetIToSpriteLocationForVx(opcode);
                    break;
                case 0x33:
                    cpu.StoreBCDOfVxAtI(opcode);
                    break;
                case 0x55:
                    cpu.StoreV0ToVxInMemoryStartingAtI(opcode);
                    break;
                case 0x65:
                    cpu.FillV0ToVxWithValuesFromMemoryStartingAtI(opcode);
                    break;
            }
            break;
          }
        cpu.PC += 2;
        }
    }
}