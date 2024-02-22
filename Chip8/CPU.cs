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
    }
}
