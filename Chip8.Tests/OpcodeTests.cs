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
            cpu.RAM = new byte[4096];
            cpu.Registers = new byte[16];
            cpu.Display = new byte[64 * 32];
            cpu.PC = 0x200;
            cpu.SP = 0;
            cpu.I = 0;
            cpu.Keyboard = 0;
            cpu.DelayTimer = 0;
            cpu.SoundTimer = 0;
            cpu.IsDirty = true;
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
        
        [Fact]
        public void SetVxToVxOrVy_8XY1_PerformsBitwiseOrAndStoresInVx()
        {
            // Arrange
            ushort opcode = 0x8AB1; 
            cpu.Registers[0xA] = 0x0C; 
            cpu.Registers[0xB] = 0x03; 

            byte expectedValue = 0x0C | 0x03; 

            // Act
            cpu.SetVxToVxOrVy_8XY1(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(0x202, cpu.PC); 
        }
    
        [Fact]
        public void SetVxToVxAndVy_8XY2_PerformsBitwiseAndAndStoresInVx()
        {
            // Arrange
            ushort opcode = 0x8AB2; 
            cpu.Registers[0xA] = 0x0F; 
            cpu.Registers[0xB] = 0x03;

            byte expectedValue = 0x0F & 0x03; 

            // Act
            cpu.SetVxToVxAndVy_8XY2(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(0x202, cpu.PC); 
        }
        
        [Fact]
        public void SetVxToVxXorVy_8XY3_PerformsBitwiseXorAndStoresInVx()
        {
            // Arrange
            ushort opcode = 0x8AB3; 
            cpu.Registers[0xA] = 0x0C; 
            cpu.Registers[0xB] = 0x0F; 

            byte expectedValue = 0x0C ^ 0x0F; 

            // Act
            cpu.SetVxToVxXorVy_8XY3(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(0x202, cpu.PC);
        }

        [Fact]
        public void AddVyToVx_8XY4_HandlesOverflowCorrectly()
        {
            // Arrange
            ushort opcode = 0x8AB4; 
            cpu.Registers[0xA] = 0xFF; 
            cpu.Registers[0xB] = 0x02;

            byte expectedValue = (0xFF + 0x02) & 0xFF; 

            // Act
            cpu.AddVyToVx_8XY4(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(1, cpu.Registers[0xF]); 
            Assert.Equal(0x202, cpu.PC);
        }
        
        [Fact]
        public void SubtractVyFromVx_8XY5_SubtractsCorrectlyWithoutBorrow()
        {
            // Arrange
            ushort opcode = 0x8AB5;
            cpu.Registers[0xA] = 0x20;
            cpu.Registers[0xB] = 0x10; // avoid borrow

            byte expectedValue = 0x20 - 0x10; // without borrow

            // Act
            cpu.SubtractVyFromVx_8XY5(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(1, cpu.Registers[0xF]); // 1 (no borrow)
            Assert.Equal(0x202, cpu.PC); 
        }

        [Fact]
        public void SubtractVyFromVx_8XY5_HandlesBorrowCorrectly()
        {
            // Arrange
            ushort opcode = 0x8AB5;
            cpu.Registers[0xA] = 0x10;
            cpu.Registers[0xB] = 0x20; // cause borrow

            byte expectedValue = unchecked((byte)(0x10 - 0x20)); // with borrow

            // Act
            cpu.SubtractVyFromVx_8XY5(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]);
            Assert.Equal(0, cpu.Registers[0xF]); // 0 (borrow occurred)
            Assert.Equal(0x202, cpu.PC);
        }
        
        [Fact]
        public void ShiftVxRight_8XY6_ShiftsVxRightAndSetsVF()
        {
            // Arrange
            ushort opcode = 0x8A06; 
            cpu.Registers[0xA] = 0x0B; 

            byte expectedValue = 0x0B >> 1; 
            byte expectedVF = 0x0B & 0x1;  

            // Act
            cpu.ShiftVxRight_8XY6(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(expectedVF, cpu.Registers[0xF]);    
            Assert.Equal(0x202, cpu.PC);                    
        }

        [Fact]
        public void SetVxToVyMinusVx_8XY7_SubtractsVyFromVxCorrectlyWithoutBorrow()
        {
            // Arrange
            ushort opcode = 0x8AB7; 
            cpu.Registers[0xA] = 0x10;
            cpu.Registers[0xB] = 0x20; // avoid borrow

            byte expectedValue = (byte)(cpu.Registers[0xB] - cpu.Registers[0xA]); 

            // Act
            cpu.SetVxToVyMinusVx_8XY7(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(1, cpu.Registers[0xF]); // 1 (no borrow)
            Assert.Equal(0x202, cpu.PC); 
        }

        [Fact]
        public void SetVxToVyMinusVx_8XY7_HandlesBorrowCorrectly()
        {
            // Arrange
            ushort opcode = 0x8AB7; 
            cpu.Registers[0xA] = 0x30; 
            cpu.Registers[0xB] = 0x10; // cause borrow

            byte expectedValue = (byte)(cpu.Registers[0xB] - cpu.Registers[0xA]); 

            // Act
            cpu.SetVxToVyMinusVx_8XY7(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(0, cpu.Registers[0xF]); // 0 (borrow occurred)
            Assert.Equal(0x202, cpu.PC); 
        }

        [Fact]
        public void ShiftVxLeft_8XYE_ShiftsVxLeftAndSetsVF()
        {
            // Arrange
            ushort opcode = 0x8ABE; 
            cpu.Registers[0xA] = 0x85; 

            byte expectedValue = 0x0A; 
            byte expectedVF = 1; 

            // Act
            cpu.ShiftVxLeft_8XYE(opcode);

            // Assert
            Assert.Equal(expectedValue, cpu.Registers[0xA]); 
            Assert.Equal(expectedVF, cpu.Registers[0xF]);    
            Assert.Equal(0x202, cpu.PC);                    
        }
        
        [Fact]
        public void SkipIfVxNotEqualsVy_9XY0_SkipsNextInstructionWhenNotEqual()
        {
            // Arrange
            ushort opcode = 0x9AB0; 
            cpu.Registers[0xA] = 0x01; 
            cpu.Registers[0xB] = 0x02; 

            // Act
            cpu.SkipIfVxNotEqualsVy_9XY0(opcode);

            // Assert
            Assert.Equal(0x204, cpu.PC); // 4 when not equal
        }
        
        [Fact]
        public void SetAddressToI_ANNN_SetsIToNNN()
        {
            // Arrange
            ushort opcode = 0xA123; 
            
            // Act
            cpu.SetIToAddress_ANNN(opcode); 

            // Assert
            Assert.Equal(0x123, cpu.I); 
            Assert.Equal(0x202, cpu.PC); 
        }

        [Fact]
        public void JumpToAddressPlusV0_Bnnn_SetsPCToNNNPlusV0()
        {
            // Arrange
            ushort opcode = 0xB123;
            cpu.Registers[0] = 0x05;
            ushort expectedAddress = (ushort)(0x123 + cpu.Registers[0]); // Cast result to ushort

            // Act
            cpu.JumpToAddressPlusV0_BNNN(opcode);

            // Assert
            Assert.Equal(expectedAddress, cpu.PC);
        }

        [Fact]
        public void SetVxToRandomByteAnd_CNNN_SetsRegisterWithinRangeAndIncrementsPC()
        {
            // Arrange
            ushort opcode = 0xC0FF; 
            ushort initialPC = cpu.PC;

            // Act
            cpu.SetVxToRandomByteAnd_CNNN(opcode);

            // Assert
            byte registerValue = cpu.Registers[0];
            Assert.InRange(registerValue, byte.MinValue, byte.MaxValue); 

            Assert.Equal(initialPC + 2, cpu.PC); 
        }

        [Fact]
        public void DrawSprite_DXYN_UpdatesDisplayAndSetsCollisionFlagIfNecessary()
        {
            // Arrange
            cpu.Registers[0] = 0; // x position
            cpu.Registers[1] = 0; // y position
            cpu.I = 0x200; // Starting address of the sprite in RAM

            // 8x2 masterpeice
            // first row is all 1s, second row is alternating 1s and 0s
            cpu.RAM[0x200] = 0xFF; 
            cpu.RAM[0x201] = 0xAA; 

            ushort opcode = 0xD012; // X=0,
                                    // Y=1,
                                    // N=2 (draw 2-byte sprite from I at position V0,V1)

            // Act
            cpu.DrawSprite_DXYN(opcode);

            // Assert
            // row 1 (0xFF)
            for (int i = 0; i < 8; i++)
            {
                Assert.Equal(1, cpu.Display[i]); // Each pixel in the first row should be set
            }

            // row 2 (0xAA)
            for (int i = 64; i < 72; i++) 
            {
                int expected = (i % 2 == 0) ? 1 : 0; // Expect 1 on even indices, 0 on odd
                Assert.Equal(expected, cpu.Display[i]);
            }
            
            Assert.Equal(0, cpu.Registers[0xF]);
            Assert.True(cpu.IsDirty);
        }

        [Fact]
        public void SkipIfKeyIsPressed_EX9E_IncrementsPCBy4IfKeyPressed()
        {
            // Arrange
            ushort opcode = 0xE09E; 
            cpu.Registers[0] = 0xA; 
            cpu.Keyboard = (ushort)(1 << cpu.Registers[0]); // pressing key here
            ushort initialPC = cpu.PC;

            // Act
            cpu.SkipIfKeyIsPressed_EX9E(opcode);

            // Assert
            Assert.Equal(initialPC + 4, cpu.PC); 
        }
        
        [Fact]
        public void SkipIfKeyIsPressed_EX9E_IncrementsPCBy2IfKeyNotPressed()
        {
            // Arrange
            ushort opcode = 0xE09E; 
            cpu.Registers[0] = 0xA; 
            cpu.Keyboard = 0x0; 
            ushort initialPC = cpu.PC;

            // Act
            cpu.SkipIfKeyIsPressed_EX9E(opcode);

            // Assert
            Assert.Equal(initialPC + 2, cpu.PC); 
        }

        [Fact]
        public void SkipIfKeyIsNotPressed_EXA1_SkipsNextInstructionIfKeyNotPressed()
        {
            // Arrange
            ushort opcode = 0xEA1; 
            cpu.Registers[0xA] = 0x5; 
            cpu.Keyboard = 0x0; 
            ushort initialPC = cpu.PC;

            // Act
            cpu.SkipIfKeyIsNotPressed_EXA1(opcode);

            // Assert
            Assert.Equal(initialPC + 4, cpu.PC); 
        }
        
        [Fact]
        public void SkipIfKeyIsNotPressed_EXA1_DoesNotSkipNextInstructionIfKeyPressed()
        {
            // Arrange
            ushort opcode = 0xEA1;
            cpu.Registers[0xA] = 0x5;
            cpu.Keyboard = (ushort)(1 << 5); // Key 5 is pressed
            ushort initialPC = cpu.PC;

            // Act
            cpu.SkipIfKeyIsNotPressed_EXA1(opcode);

            // Assert
            Assert.Equal(initialPC + 2, cpu.PC); 
        }

    }
}