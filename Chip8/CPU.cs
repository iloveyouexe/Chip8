using System;

namespace Chip8
{
    public class CPU
    {
        public byte[] RAM = new byte[4096];
        public byte[] Registers = new byte[16]; // V[0x0] => V[0xF} hexidecimal 0-16
        public byte DelayTimer;
        public byte SoundTimer;
        public byte Keyboard;
        public ushort I = 0;
        public ushort[] Stack = new ushort[24];
        public byte SP; // stack pointer
        public ushort PC; // program counter
        public static byte[] Display = new byte[64 * 32];
        public bool IsDirty = false;
        
        public void RenderDisplay()
        {
            Console.SetCursorPosition(0,0);
            
            for (int y = 0; y < 32; y++) 
            {
                for (int x = 0; x < 64; x++) 
                {
                    int index = x + (y * 64);
                    Console.Write(Display[index] == 1 ? "#" : " ");
                }
                Console.WriteLine(); 
            }
        }
        
        public void ClearScreen(ushort opcode)
        {
            for (int i = 0; i < Display.Length; i++) Display[i] = 0;
            PC += 2;
        }

        public void ReturnFromSubroutine(ushort opcode)
        {
            SP--;
            Stack[SP] = PC;
            PC += 2;
        }        
        public void JumpToAddress(ushort opcode)
        {
            var nnn = (opcode & 0x0FFF);
            PC = (ushort)nnn;
        }

        public void CallSubroutine(ushort opcode)
        {
            var nnn = (opcode & 0x0FFF);
            PC = Stack[SP];
            SP++;
            PC = (ushort)nnn;
        }

        public void SkipIfVxEqualsByte(ushort opcode)
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
        
        public void SkipIfVxNotEqualsByte(ushort opcode)
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
        
        public void SkipIfVxEqualsVy(ushort opcode)
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

        public void SetVxToByte(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var kk = (byte)(opcode & 0x00FF);
            Registers[x] = kk;
            PC += 2;
        }

        public void AddByteToVx(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var kk = (byte)(opcode & 0x00FF);
            Registers[x] += kk;
            PC += 2;
        }
        
        public void SetVxToVy(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] = Registers[y];
            PC += 2;
        }

        public void SetVxToVxOrVy(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] |= Registers[y];
            PC += 2;
        }

        public void SetVxToVxAndVy(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] &= Registers[y];
            PC += 2;
        }

        public void SetVxToVxXorVy(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] ^= Registers[y];
            PC += 2;
        }

        public void AddVyToVx(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            ushort sum = (ushort)(Registers[x] + Registers[y]); // for overflow
            Registers[0xF] = (byte)(sum > 255 ? 1 : 0); 
            Registers[x] = (byte)(sum & 0xFF); 
            PC += 2;
        }

        public void SubtractVyFromVx(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[0xF] = (byte)(Registers[x] > Registers[y] ? 1 : 0);
            Registers[x] -= Registers[y];
            PC += 2;
        }

        public void ShiftVxRight(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            Registers[0xF] = (byte)(Registers[x] & 0x1);
            Registers[x] >>= 1;
            PC += 2;
        }
        
        public void SetVxToVyMinusVx(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8; 
            var y = (opcode & 0x00F0) >> 4;
            Registers[0xF] = (byte)(Registers[y] > Registers[x] ? 1 : 0);
            Registers[y] -= Registers[x];
            PC += 2;
        }
        
        public void ShiftVxLeft(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            Registers[0xF] = (byte)((Registers[x] & 0x80) == 0x80 ? 1 : 0); 
            Registers[x] <<= 1; 
            PC += 2; 
        }

        public void SkipIfVxNotEqualsVy(ushort opcode)
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
        
        public void SetIToAddress(ushort opcode)
        {
            var address = (ushort)(opcode & 0x0FFF);
            I = address;
            PC += 2;
        }
        
        public void JumpToAddressPlusV0(ushort opcode)
        {
            var nnn = (ushort)(opcode & 0x0FFF);
            var newAddress = (ushort)(nnn + Registers[0]);
            PC = newAddress;
        }
        
        public void SetVxToRandomByteAnd(ushort opcode)
        {
            Random rand = new Random();
            byte randomByte = (byte)rand.Next(0, 256);
            var kk = (byte)(opcode & 0x00FF);
            var x = (ushort)(opcode & 0x0F00) >> 8;

            Registers[x] = (byte)(randomByte & kk);
            PC += 2;
        }
        
        public void DrawSprite(ushort opcode)
        {
            var x = Registers[(opcode & 0x0F00) >> 8];
            var y = Registers[(opcode & 0x00F0) >> 4];
            ushort height = (ushort)(opcode & 0x000F);
            ushort pixel;

            Registers[0xF] = 0;  // Assume no collision to start with

            for (int yLine = 0; yLine < height; yLine++)
            {
                pixel = RAM[I + yLine];
                for (int xLine = 0; xLine < 8; xLine++)
                {
                    if ((pixel & (0x80 >> xLine)) != 0)
                    {
                        IsDirty = true;
                        int index = (x + xLine + ((y + yLine) * 64));
                        
                        if (Display[index] == 1)
                        {
                            Display[index] = 0;
                            Registers[0xF] = 1;
                        }
                        else
                        {
                            Display[index] = 1;
                        }
                    }
                }
            }
        }
        
        //

        public void SkipIfKeyIsPressed(ushort opcode)
        {
            if ((Keyboard & (1 << Registers[(opcode & 0x0F00) >> 8])) != 0)
            {
                PC += 4;
            }
            else
            {
                PC += 2;
            }
        }

        public void SkipIfKeyIsNotPressed(ushort opcode)
        {
            if ((Keyboard & (1 << Registers[(opcode & 0x0F00) >> 8])) == 0)
            {
                PC += 4;
            }
            else
            {
                PC += 2;
            }
        }
        
        public void SetVxToDelayTimer(ushort opcode)
        {
            Registers[(opcode & 0x0F00) >> 8] = DelayTimer;
            PC += 2;
        }
        
        public void SetDelayTimerToVx(ushort opcode)
        {
            DelayTimer = Registers[(opcode & 0x0F00) >> 8];
            PC += 2;
        }

        public void SetSoundTimerToVx(ushort opcode)
        {
            SoundTimer = Registers[(opcode & 0x0F00) >> 8];
            PC += 2;
        }

        public void AddVxToI(ushort opcode)
        {
            I += Registers[(opcode & 0x0F00) >> 8];
            PC += 2;
        }

        public void SetIToSpriteLocationForVx(ushort opcode)
        {
            I = GetSpriteAddress(Registers[(opcode & 0x0F00) >> 8]);
            PC += 2;
        }

        public void StoreBCDOfVxAtI(ushort opcode)
        {
            int value = Registers[(opcode & 0x0F00) >> 8];
            RAM[I] = (byte)(value / 100);
            RAM[I + 1] = (byte)((value / 10) % 10);
            RAM[I + 2] = (byte)(value % 10);
            PC += 2;
        }

        public void StoreV0ToVxInMemoryStartingAtI(ushort opcode)
        {
            for (int i = 0; i <= ((opcode & 0x0F00) >> 8); ++i)
            {
                RAM[I + i] = Registers[i];
            }
            PC += 2;
        }

        public void FillV0ToVxWithValuesFromMemoryStartingAtI(ushort opcode)
        {
            for (int i = 0; i <= ((opcode & 0x0F00) >> 8); ++i)
            {
                Registers[i] = RAM[I + i];
            }
            PC += 2;
        }
        
        public ushort GetSpriteAddress(byte digit)
        {
            return (ushort)(digit * 5);
        }
    }
}
