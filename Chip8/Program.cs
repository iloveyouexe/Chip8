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

            if (int.TryParse(Console.ReadLine(), out var option))
            {
                switch (option)
                {
                    case 1:
                         filePath = "D:\\Source\\Chip8\\Chip8\\bin\\Debug\\Landing.ch8";
                        break;
                    case 2:
                         filePath = "D:\\Source\\Chip8\\Chip8\\bin\\Debug\\Guess.ch8";
                        break;
                    case 3:
                        filePath = "D:\\Source\\Chip8\\Chip8\\bin\\Debug\\IBMLogo.ch8";
                        break;
                    default:
                        Console.WriteLine("Select an actual option");
                        return;
                }
            }
            
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var opcode = reader.ReadUInt16();
                    Console.WriteLine($"{opcode.ToString($"X")}");
                }
            }
            ConvertToBytes();
        }
        
        static void ConvertToBytes()
        {
            Console.WriteLine("Enter your Op Code (e.g 0x8034) ");
            var opCode = Console.ReadLine();
            
            Console.WriteLine($"You have entered Op Code {opCode}.");
            var hexBytes = opCode?.Replace("0x", "");
            int intValue = int.Parse(hexBytes!, System.Globalization.NumberStyles.HexNumber);
            
            var lsb = intValue & 0x00FF;
            Console.WriteLine($"The least significant byte is 0x{lsb:X2}.");
            
            var msb = (intValue & 0xFF00) >> 8;
            Console.WriteLine($"The most significant byte is 0x{msb:X2}");
        }
    }
}
