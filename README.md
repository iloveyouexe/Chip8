utilizing http://devernay.free.fr/hacks/chip8/C8TECH10.HTM

# CHIP-8 Emulator in C#

This project is a detailed look into one of my favorite projects: a CHIP-8 emulator built in C#. This project was started under the recommendation of my work-mentor, Jason Pike.

## What is CHIP-8?

CHIP-8 is a simple, interpreted programming language that was initially created for programming video games on the COSMAC VIP and other early microcomputers in the mid-1970s. Despite its age, CHIP-8 remains a popular platform for learning about emulation and low-level programming due to its simplicity and well-documented architecture.

## Why Build a CHIP-8 Emulator?

Building an emulator is a fantastic way to deepen your understanding of computer architecture, machine language, and the inner workings of vintage systems. For me, the CHIP-8 project was an opportunity to:
- Explore low-level programming concepts.
- Improve my skills in C#.
- Tackle the challenge of accurately emulating a historical computing system.

## The Architecture of CHIP-8

CHIP-8 programs run on a virtual machine with the following components:
- **Memory:** 4KB of RAM where the system reserves the first 512 bytes for the interpreter, leaving 3584 bytes for the program.
- **Registers:** 16 general-purpose 8-bit registers (V0 to VF) and a 16-bit register called I used for memory addresses.
- **Stack:** Used for storing return addresses when subroutines are called.
- **Timers:** Delay and sound timers that decrement at a rate of 60Hz.
- **Keyboard:** A 16-key hexadecimal keypad.
- **Display:** A 64x32 monochrome display where each pixel is either on or off.

## Building the Emulator

### Setting Up the Project

I started by setting up a C# console application using .NET Core. The project structure includes separate classes for the CPU, memory, display, and input handling to maintain modularity and readability.

### Implementing the CPU

The CPU class is the heart of the emulator. It fetches, decodes, and executes instructions. Here's a snippet of the fetch-decode-execute cycle:

```csharp
public void EmulateCycle()
{
    // Fetch
    ushort opcode = FetchOpcode();
    
    // Decode and Execute
    switch (opcode & 0xF000)
    {
        case 0x0000:
            switch (opcode & 0x00FF)
            {
                case 0x00E0: // Clear the display
                    ClearScreen_00E0(opcode);
                    break;
                case 0x00EE: // Return from subroutine
                    ReturnFromSubroutine_00EE(opcode);
                    break;
                // More cases...
            }
            break;
        // More cases...
    }
}
```

## Handling Graphics and Input
The display class manages the 64x32 pixel screen. I used a simple array to represent the screen state and a method to draw sprites. Input handling was achieved by mapping the 16-key keypad to the keyboard inputs.

## Testing and Debugging
Testing involved running various CHIP-8 ROMs and verifying that they behaved correctly. Debugging required a deep dive into the opcode implementations and ensuring that the state transitions in the CPU were accurate.

## Adding Sound and Timers
Finally, I implemented the sound and delay timers, which decrement at a rate of 60Hz. The sound timer produces a beep when it reaches zero, adding an auditory element to the emulation.

## Key Code Components
Here’s a glimpse into the structure and code of the CHIP-8 emulator:

Program.cs
csharp
Copy code
using System;
using System.Threading;

```csharp
namespace Chip8
{
    class Program
    {
        static void Main(string[] args)
        {
            if (RomReader.SelectRomFile(out var cpu)) return;
            
            while (true)
            {
                cpu.ExecuteOpcode(cpu);
                cpu.CheckIfIsDirty(cpu);
                Thread.Sleep(1000 / 60);
            }
        }

        public static void DecodeAndExecute(CPU cpu, ushort opcode)
        {
            switch (opcode & 0xF000)
            {
                // Implementations for each opcode category
                case 0x1000:
                    cpu.JumpToAddress_1NNN(opcode);
                    break;
                // More cases...
            }
        }
    }
}
```
```csharp
CPU.cs
csharp
Copy code
using System;
using System.Text;

namespace Chip8
{
    public class CPU
    {
        // CPU components and state
        public byte[] RAM = new byte[4096];
        public byte[] Registers = new byte[16];
        public byte DelayTimer;
        public byte SoundTimer;
        public ushort Keyboard;
        public ushort I = 0;
        public ushort[] Stack = new ushort[24];
        public byte SP;
        public ushort PC = 0x200; // Program starts at 0x200
        public byte[] Display = new byte[64 * 32];
        public bool IsDirty = false;
        private Random random = new Random();
        
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
        
        public void ExecuteOpcode(CPU cpu)
        {
            ushort opcode = FetchOpcode(cpu);
            Program.DecodeAndExecute(cpu, opcode);
        }
        
        public static ushort FetchOpcode(CPU cpu)
        {
            var highByte = cpu.RAM[cpu.PC];
            var lowByte = cpu.RAM[cpu.PC + 1];
            return (ushort)(highByte << 8 | lowByte);
        }

        // Opcode implementations...
        public void JumpToAddress_1NNN(ushort opcode)
        {
            var address = (ushort)(opcode & 0x0FFF);
            PC = address;
        }

        // More opcode methods...
    }
}
```

## Challenges and Learnings
Building the CHIP-8 emulator was not without its challenges. The most significant hurdles were:

- Ensuring accurate timing and synchronization between the CPU cycles and timers.
- Troubleshooting issues regarding the drawing of sprites after reading placement.
- Debugging opcode implementations to ensure they matched the CHIP-8 specification.

Through this project, I gained a deeper appreciation for the intricacies of emulation and the elegance of the CHIP-8 architecture. It was a rewarding experience that strengthened my skills in C# and low-level programming.
