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
            Console.WriteLine("CLS: Clearing the screen... ");
            for (int i = 0; i < Display.Length; i++) Display[i] = 0;
            PC += 2;
        }

        public void ReturnFromSubroutine(ushort opcode)
        {
            Console.WriteLine("RET: Returning from subroutine... ");
            SP--;
            Stack[SP] = PC;
            PC += 2;
        }

        public void CallSubroutine(ushort opcode)
        {
            var nnn = (opcode & 0x0FFF);
            Console.WriteLine("CALL: Calling a subroutine... ");
            Stack[SP] = PC;
            SP++;
            PC = (ushort)nnn;
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
    }
}
