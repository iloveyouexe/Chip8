using System;
using System.Text;

namespace Chip8
{
    public class CPU
    {
        public byte[] RAM = new byte[4096];
        public byte[] Registers = new byte[16]; // V[0x0] => V[0xF} hexidecimal 0-16
        public byte DelayTimer; //TODO
        public byte SoundTimer; //TODO 
        public ushort Keyboard;
        public ushort I = 0;
        public ushort[] Stack = new ushort[24];
        public byte SP; // stack pointer
        public ushort PC; // program counter
        public byte[] Display = new byte[64 * 32];
        public bool IsDirty = false;
        private Random random = new Random();
        private bool isAlsoFlagged = false;
        
        public void RenderDisplay()
        {
            StringBuilder displayBuffer = new StringBuilder();
            Console.SetCursorPosition(0, 0);  
    
            for (int y = 0; y < 32; y++) 
            {
                for (int x = 0; x < 64; x++) 
                {
                    int index = x + (y * 64); 
                    displayBuffer.Append(Display[index] == 1 ? "#" : "."); 
                }
                displayBuffer.AppendLine(); 
            }
            Console.Write(displayBuffer.ToString());  
        }
        
        public void ClearScreen_00E0(ushort opcode)
        {
            Display = new byte[64 * 32];
            PC += 2;
            IsDirty = true;
        }

        public void ReturnFromSubroutine_00EE(ushort opcode)
        {
            SP--; 
            PC = Stack[SP];
        }  
        
        public void JumpToAddress_1NNN(ushort opcode)
        {
            var nnn = (opcode & 0x0FFF);
            PC = (ushort)nnn;
        }

        public void CallSubroutine_2NNN(ushort opcode)
        {
            Stack[SP] = PC; 
            SP++; 
            var nnn = (opcode & 0x0FFF); 
            PC = (ushort)nnn; 
        }

        public void SkipIfVxEqualsByte_3XKK(ushort opcode)
        {
            var nn = (opcode & 0x00FF);
            var x = (opcode & 0x0F00) >> 8;
            if (Registers[x] == nn)
            {
                PC += 4;
            }
            else
            {
                PC += 2;
            }
        }
        
        public void SkipIfVxNotEqualsByte_4XKK(ushort opcode)
        {
            var nn = (opcode & 0x00FF);
            var x = (opcode & 0x0F00) >> 8;
            if (Registers[x] != nn)
            {
                PC += 4;
            }
            else
            {
                PC += 2;
            }
        }
        
        public void SkipIfVxEqualsVy_5XY0(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            if (Registers[x] == Registers[y])
            {
                PC += 4;
            }
            else
            {
                PC += 2;
            }
        }

        public void SetVxToByte_6XKK(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var kk = (byte)(opcode & 0x00FF);
            Registers[x] = kk;
            PC += 2;
        }

        public void AddByteToVx_7XKK(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var kk = (byte)(opcode & 0x00FF);
            Registers[x] += kk;
            PC += 2;
        }
        
        public void SetVxToVy_8XY0(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] = Registers[y];
            PC += 2;
        }

        public void SetVxToVxOrVy_8XY1(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] |= Registers[y];
            PC += 2;
        }

        public void SetVxToVxAndVy_8XY2(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] &= Registers[y];
            PC += 2;
        }

        public void SetVxToVxXorVy_8XY3(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] ^= Registers[y];
            PC += 2;
        }

        public void AddVyToVx_8XY4(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            ushort sum = (ushort)(Registers[x] + Registers[y]); 
            Registers[0xF] = (byte)(sum > 255 ? 1 : 0); 
            Registers[x] = (byte)(sum & 0xFF); 
            PC += 2;
        }

        public void SubtractVyFromVx_8XY5(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[0xF] = (byte)(Registers[x] > Registers[y] ? 1 : 0);
            Registers[x] -= Registers[y];
            PC += 2;
        }

        public void ShiftVxRight_8XY6(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            Registers[0xF] = (byte)(Registers[x] & 0x1);
            Registers[x] >>= 1;
            PC += 2;
        }
        
        public void SetVxToVyMinusVx_8XY7(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8; 
            var y = (opcode & 0x00F0) >> 4;
            Registers[0xF] = (byte)(Registers[y] >= Registers[x] ? 1 : 0);  
            Registers[x] = (byte)(Registers[y] - Registers[x]);
            PC += 2;
        }
        
        public void ShiftVxLeft_8XYE(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            Registers[0xF] = (byte)((Registers[x] & 0x80) == 0x80 ? 1 : 0); 
            Registers[x] <<= 1; 
            PC += 2; 
        }

        public void SkipIfVxNotEqualsVy_9XY0(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            if (Registers[x] != Registers[y])
            {
                PC += 4;
            }
            else
            {
                PC += 2;
            }
        }
        
        public void SetIToAddress_ANNN(ushort opcode)
        {
            var address = (ushort)(opcode & 0x0FFF);
            I = address;
            PC += 2;
        }
        
        public void JumpToAddressPlusV0_BNNN(ushort opcode)
        {
            var nnn = (ushort)(opcode & 0x0FFF);
            var newAddress = (ushort)(nnn + Registers[0]);
            PC = newAddress;
        }
        
        public void SetVxToRandomByteAnd_CNNN(ushort opcode)
        {
            byte randomByte = (byte)random.Next(0, 256); 
            var kk = (byte)(opcode & 0x00FF);
            var x = (ushort)((opcode & 0x0F00) >> 8);

            Registers[x] = (byte)(randomByte & kk);
            PC += 2;
        }
        
        public void DrawSprite_DXYN(ushort opcode)
        {
            var x = Registers[(opcode & 0x0F00) >> 8]; 
            var y = Registers[(opcode & 0x00F0) >> 4]; 
            ushort height = (ushort)(opcode & 0x000F);  
            ushort pixel;

            Registers[0xF] = 0;  

            for (int yLine = 0; yLine < height; yLine++)
            {
                pixel = RAM[I + yLine];  
                for (int xLine = 0; xLine < 8; xLine++)
                {
                    if ((pixel & (0x80 >> xLine)) != 0) 
                    {
                        int xCoord = (x + xLine) % 64; 
                        int yCoord = (y + yLine) % 32;  
                        int index = xCoord + (yCoord * 64);  

                        if (Display[index] == 1) 
                        {
                            Registers[0xF] = 1;
                        }
                        Display[index] ^= 1; 
                        IsDirty = true;  
                    }
                }
            }
            PC += 2;
            // System.IO.File.AppendAllText("debug_log.txt", $"Drew sprite at x: {x}, y: {y}, height: {height}\n");  // Log the operation
        }
        
        public void SkipIfKeyIsPressed_EX9E(ushort opcode)
        {
            int registerIndex = (opcode & 0x0F00) >> 8;
            if ((Keyboard & (1 << Registers[registerIndex])) != 0)
            {
                PC += 4;
            }
            else
            {
                PC += 2;
            }
        }
        
        public void SkipIfKeyIsNotPressed_EXA1(ushort opcode)
        {
            int registerIndex = (opcode & 0x0F00) >> 8;
            if ((Keyboard & (Registers[registerIndex])) == 0)
            {
                PC += 4; 
            }
            else
            {
                PC += 2; 
            }
        }
        
        public void SetVxToDelayTimer_FX07(ushort opcode)
        {
            Registers[(opcode & 0x0F00) >> 8] = DelayTimer;
            PC += 2;
        }

        public void StoreKeyPressValueToVx_FX0A(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;

            bool keyPressDetected = false;
            for (int i = 0; i < 16; ++i)
            {
                if ((Keyboard & (1 << i)) != 0)
                {
                    Registers[x] = (byte)i;  
                    keyPressDetected = true;
                    break;  
                }
            }
            if (!keyPressDetected)
            {
                return;
            }
            PC += 2; 
        }
        
        public void SetDelayTimerToVx_FX15(ushort opcode)
        {
            DelayTimer = Registers[(opcode & 0x0F00) >> 8];
            PC += 2;
        }

        public void SetSoundTimerToVx_FX18(ushort opcode)
        {
            SoundTimer = Registers[(opcode & 0x0F00) >> 8];
            PC += 2;
        }

        public void AddVxToI_FX1E(ushort opcode)
        {
            I += Registers[(opcode & 0x0F00) >> 8];
            PC += 2;
        }

        public void SetIToSpriteLocationForVx_FX29(ushort opcode)
        {
            I = GetSpriteAddress(Registers[(opcode & 0x0F00) >> 8]);
            PC += 2;
        }
        
        public ushort GetSpriteAddress(byte digit)
        {
            return (ushort)(digit * 5);
        }

        public void StoreBCDOfVxAtI_FX33(ushort opcode)
        {
            int value = Registers[(opcode & 0x0F00) >> 8];
            RAM[I] = (byte)(value / 100);
            RAM[I + 1] = (byte)((value / 10) % 10);
            RAM[I + 2] = (byte)(value % 10);
            PC += 2;
        }

        public void StoreV0ToVxInMemoryStartingAtI_FX55(ushort opcode)
        {
            for (int i = 0; i <= ((opcode & 0x0F00) >> 8); ++i)
            {
                RAM[I + i] = Registers[i];
            }
            PC += 2;
        }

        public void FillV0ToVxWithValuesFromMemoryStartingAtI_FX65(ushort opcode)
        {
            for (int i = 0; i <= ((opcode & 0x0F00) >> 8); ++i)
            {
                Registers[i] = RAM[I + i];
            }
            PC += 2;
        }
        
        public void HandleUnrecognizedOpcode(CPU cpu, ushort opcode)
        {
            Console.WriteLine($"Unrecognized opcode {opcode:X4} at PC: {cpu.PC:X4}");
            // Environment.Exit(1);
            cpu.PC += 2;
        }
        
        public void Initialize()
        {
            RAM = new byte[4096];
            Registers = new byte[16];
            Display = new byte[64 * 32];
            PC = 0x200; 
            SP = 0;
            I = 0;
            Keyboard = 0;
            DelayTimer = 0;
            SoundTimer = 0;
            IsDirty = true; 
        }
        
        public void CheckIfIsDirty(CPU cpu)
        {
            if (cpu.IsDirty)
            {
                Console.Clear();
                cpu.RenderDisplay();
                cpu.IsDirty = false;
            }
        }
        
        public void ExecuteOpcode(CPU cpu)
        {
            ushort opcode = FetchOpcode(cpu);
            System.IO.File.AppendAllText("debug_log.txt", $"Executing opcode at PC: {cpu.PC}, Opcode: {opcode:X4}\n");
            Program.DecodeAndExecute(cpu, opcode);
            System.IO.File.AppendAllText("debug_log.txt", $"PC after execution: {cpu.PC}\n");
        }
        
        public static ushort FetchOpcode(CPU cpu)
        {
            var highByte = cpu.RAM[cpu.PC];
            var lowByte = cpu.RAM[cpu.PC + 1];
            return (ushort)(highByte << 8 | lowByte);
        }
    }
}