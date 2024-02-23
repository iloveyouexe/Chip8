using System;

namespace Chip8
{
    public class CPU
    {
        //build
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

        public void CallSubroutine(ushort opcode)
        {
            var nnn = (opcode & 0x0FFF);
            Stack[SP] = PC;
            SP++;
            PC = (ushort)nnn;
        }

        public void SkipNextInstructionOnEquals(ushort opcode)
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
        
        public void SkipNextInstructionNotOnEquals(ushort opcode)
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
        
        public void SkipIfEqual(ushort opcode)
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

        public void LoadRegisterByte(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var kk = (byte)(opcode & 0x00FF);
            Registers[x] = kk;
            PC += 2;
        }

        public void AddRegisterByte(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var kk = (byte)(opcode & 0x00FF);
            Registers[x] += kk;
            PC += 2;
        }
        
        public void LoadRegister(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] = Registers[y];
            PC += 2;
        }

        public void OrRegister(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] |= Registers[y];
            PC += 2;
        }

        public void AndRegister(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] &= Registers[y];
            PC += 2;
        }

        public void XorRegister(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[x] ^= Registers[y];
            PC += 2;
        }

        public void AddRegister(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            ushort sum = (ushort)(Registers[x] + Registers[y]); // for overflow
            Registers[0xF] = (byte)(sum > 255 ? 1 : 0); 
            Registers[x] = (byte)(sum & 0xFF); 
            PC += 2;
        }

        public void SubtractRegister(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            var y = (opcode & 0x00F0) >> 4;
            Registers[0xF] = (byte)(Registers[x] > Registers[y] ? 1 : 0);
            Registers[x] -= Registers[y];
            PC += 2;
        }

        public void ShiftRightRegister(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            Registers[0xF] = (byte)(Registers[x] & 0x1);
            Registers[x] >>= 1;
            PC += 2;
        }
        
        public void SubtractNRegister(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8; 
            var y = (opcode & 0x00F0) >> 4;
            Registers[0xF] = (byte)(Registers[y] > Registers[x] ? 1 : 0);
            Registers[y] -= Registers[x];
            PC += 2;
        }
        
        public void ShiftLeftRegister(ushort opcode)
        {
            var x = (opcode & 0x0F00) >> 8;
            Registers[0xF] = (byte)((Registers[x] & 0x80) == 0x80 ? 1 : 0); 
            Registers[x] <<= 1; 
            PC += 2; 
        }

        public void ReadRegistersV0ToVxFromMemory(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void StoreRegistersV0ToVxInMemory(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void StoreBCDOfVx(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetIToSpriteAddressInVx(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void AddVxToI(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetSoundTimerToVx(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetDelayTimerToVx(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void WaitForKeyAndStoreInVx(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetVxToDelayTimer(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SkipIfKeyInVxNotPressed(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SkipIfKeyInVxPressed(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void DrawSprite(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetVxToRandomByteAnd(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void JumpToAddressPlusV0(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetIToAddress(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SkipIfVxNotEqualsVy(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void ShiftVxLeft(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetVxToVyMinusVx(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void ShiftVxRight(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void JumpToAddress(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SkipIfVxEqualsByte(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SkipIfVxNotEqualsByte(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SkipIfVxEqualsVy(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetVxToByte(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void AddByteToVx(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetVxToVy(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetVxToVxAndVy(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetVxToVxOrVy(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SetVxToVxXorVy(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void AddVyToVx(ushort opcode)
        {
            throw new NotImplementedException();
        }

        public void SubtractVyFromVx(ushort opcode)
        {
            throw new NotImplementedException();
        }
    }
}
