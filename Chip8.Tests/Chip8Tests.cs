using Xunit;
using Chip8; 

namespace Chip8.Tests
{
    public class OpcodeTests
    {
        private readonly CPU cpu;

        public OpcodeTests()
        {
            cpu = new CPU();
            cpu.PC = 0x200;
        }
        
        [Fact]
        public void ClearScreen_00E0_ClearsDisplayAndSetsIsDirty()
        {
            // Arrange
            for (int i = 0; i < cpu.Display.Length; i++)
            {
                cpu.Display[i] = 1;
            }
            ushort opcode = 0x00E0; 

            // Act
            cpu.ClearScreen_00E0(opcode);

            // Assert
            Assert.All(cpu.Display, pixel => Assert.Equal(0, pixel)); 
            Assert.True(cpu.IsDirty); 
            Assert.Equal(0x202, cpu.PC); 
        }
        
        [Fact]
        public void ReturnFromSubroutine_00EE_ReturnsToCaller()
        {
            // Arrange
            ushort returnAddress = 0x300; 
            cpu.Stack[0] = returnAddress; 
            cpu.SP = 1; 

            ushort opcode = 0x00EE; 

            // Act
            cpu.ReturnFromSubroutine_00EE(opcode);

            // Assert
            Assert.Equal(returnAddress, cpu.PC); 
            Assert.Equal(0, cpu.SP); 
        }
        
        [Fact]
        public void JumpToAddress_1NNN_SetsPCToNNN()
        {
            // Arrange
            ushort opcode = 0x1234; 
            ushort expectedAddress = 0x0234; 

            // Act
            cpu.JumpToAddress_1NNN(opcode);

            // Assert
            Assert.Equal(expectedAddress, cpu.PC); 
        }
        
        

    }
}