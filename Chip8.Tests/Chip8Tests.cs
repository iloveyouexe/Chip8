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
        
        [Fact]
        public void CallSubroutine_2NNN_PushesPCToStackAndJumpsToNNN()
        {
            // Arrange
            cpu.PC = 0x200; 
            ushort opcode = 0x2ABC; 
            ushort expectedAddress = 0x0ABC; 
            
            // Act
            cpu.CallSubroutine_2NNN(opcode);

            // Assert
            Assert.Equal(1, cpu.SP); 
            Assert.Equal(expectedAddress, cpu.PC); 
            Assert.Equal(0x200, cpu.Stack[0]); 
        }
        
        [Fact]
        public void SkipIfVxEqualsByte_3XKK_SkipsNextInstructionWhenEqual()
        {
            // Arrange
            ushort opcode = 0x3A01; 
            cpu.Registers[0xA] = 0x01; 

            // Act
            cpu.SkipIfVxEqualsByte_3XKK(opcode);

            // Assert
            Assert.Equal(0x204, cpu.PC); // 4 when equal
        }

        [Fact]
        public void SkipIfVxEqualsByte_3XKK_DoesNotSkipNextInstructionWhenNotEqual()
        {
            // Arrange
            ushort opcode = 0x3A02; 
            cpu.Registers[0xA] = 0x01; 

            // Act
            cpu.SkipIfVxEqualsByte_3XKK(opcode);

            // Assert
            Assert.Equal(0x202, cpu.PC); // 2 when not equal
        }
        
        [Fact]
        public void SkipIfVxNotEqualsByte_4XKK_SkipsNextInstructionWhenNotEqual()
        {
            // Arrange
            ushort opcode = 0x4A01; 
            cpu.Registers[0xA] = 0x02; 

            // Act
            cpu.SkipIfVxNotEqualsByte_4XKK(opcode);

            // Assert
            Assert.Equal(0x204, cpu.PC); // 4 when not equal
        }

        [Fact]
        public void SkipIfVxNotEqualsByte_4XKK_DoesNotSkipNextInstructionWhenEqual()
        {
            // Arrange
            ushort opcode = 0x4A02; 
            cpu.Registers[0xA] = 0x02; 

            // Act
            cpu.SkipIfVxNotEqualsByte_4XKK(opcode);

            // Assert
            Assert.Equal(0x202, cpu.PC); // 2 when equal
        }

        [Fact]
        public void SkipIfVxEqualsVy_5XY0_SkipsNextInstructionWhenEqual()
        {
            // Arrange
            ushort opcode = 0x5AB0; 
            cpu.Registers[0xA] = 0x01; 
            cpu.Registers[0xB] = 0x01; 

            // Act
            cpu.SkipIfVxEqualsVy_5XY0(opcode);

            // Assert
            Assert.Equal(0x204, cpu.PC); // 4 when equal
        }

        [Fact]
        public void SkipIfVxEqualsVy_5XY0_DoesNotSkipNextInstructionWhenNotEqual()
        {
            // Arrange
            ushort opcode = 0x5AB0; 
            cpu.Registers[0xA] = 0x01; 
            cpu.Registers[0xB] = 0x00; 

            // Act
            cpu.SkipIfVxEqualsVy_5XY0(opcode);

            // Assert
            Assert.Equal(0x202, cpu.PC); // 2 when not equal
        }

        [Fact]
        public void SetVxToByte_6XKK_SetsRegisterXToKK()
        {
            // Arrange
            ushort opcode = 0x6A02;
            byte expectedValue = 0x02;

            // Act
            cpu.SetVxToByte_6XKK(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(0x202, cpu.PC); 
        }

        [Fact]
        public void AddByteToVx_7XKK_AddsKKToRegisterX()
        {
            // Arrange
            ushort opcode = 0x7A01; 
            cpu.Registers[0xA] = 0x00; 
            byte expectedValue = 0x01; 

            // Act
            cpu.AddByteToVx_7XKK(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]);
            Assert.Equal(0x202, cpu.PC); 
        }
        
        [Fact]
        public void AddByteToVx_7XKK_HandlesOverflow()
        {
            // Arrange
            ushort opcode = 0x7AFF; 
            cpu.Registers[0xA] = 0x02; // cause overflow
            byte expectedValue = 0x01; // only 0x01 is stored due to overflow

            // Act
            cpu.AddByteToVx_7XKK(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(0x202, cpu.PC); 
        }

        [Fact]
        public void SetVxToVy_8XY0_SetsRegisterXToValueOfRegisterY()
        {
            // Arrange
            ushort opcode = 0x8AB0;
            cpu.Registers[0xB] = 0x1F; 

            // Act
            cpu.SetVxToVy_8XY0(opcode);

            // Assert
            Assert.Equal(cpu.Registers[0xB], cpu.Registers[0xA]); 
            Assert.Equal(0x202, cpu.PC); 
        }

    }
}