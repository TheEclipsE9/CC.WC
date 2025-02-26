using System.Diagnostics;
using System.Text;

namespace CC.WC.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var param = args.Length != 0 ? args[0] : string.Empty;
            Stream stdin = Console.OpenStandardInput();
            switch(param)
            {
                case "-c":
                    System.Console.WriteLine(CountBytes(stdin));
                    break;
                case "-l":
                    System.Console.WriteLine(CountLines(stdin));
                    break;
                case "-w":
                    System.Console.WriteLine(CountWords(stdin));
                    break;
                case "-m":
                    System.Console.WriteLine(CountCharacters(stdin));
                    break;
                default:
                    System.Console.WriteLine(0);
                    break;
            }
        }

        public static int CountBytes(Stream stream)
        {
            int count = 0;

            byte[] buffer = new byte[1_024];

            int readedCount = 0;
            do
            {
                readedCount = stream.Read(buffer, 0, buffer.Length);
                count += readedCount;
            }
            while(readedCount != 0);

            return count;
        }

        public static int CountLines(Stream stream)
        {
            int count = 0;

            byte[] buffer = new byte[1_024];

            int readedCount = 0;
            do
            {
                readedCount = stream.Read(buffer, 0, buffer.Length);
                
                for(int i = 0; i < readedCount; i++)
                {
                    var c = (char)buffer[i];
                    if(Char.IsControl(c) && c == '\n')//only unix handling, but for win its '\r\n', so anyway should work< cause \n is last? 
                    {
                        count++;
                    }
                }
            }
            while(readedCount != 0);

            return count;
        }

        public static int CountWords(Stream stream)
        {
            int count = 0;
            bool isPrevLetter = false;

            byte[] buffer = new byte[1_024];

            int readedCount = 0;
            do
            {
                readedCount = stream.Read(buffer, 0, buffer.Length);
                
                for(int i = 0; i < readedCount; i++)
                {
                    var curChar = (char)buffer[i];
                    if(Char.IsWhiteSpace(curChar) || 
                       Char.IsSeparator(curChar) ||
                       (Char.IsControl(curChar) && curChar == '\n'))//unix only, in win is \r\n for new line
                    {
                        if (isPrevLetter) count++;

                        isPrevLetter = false;
                    }
                    else
                    {
                        isPrevLetter = true;
                    }
                }
            }
            while(readedCount != 0);

            if (isPrevLetter) count++;

            return count;
        }
    
        public static int CountCharacters(Stream stream)
        {
            int count = 0;

            byte[] buffer = new byte[1_024];

            int readedCount = 0;
            do
            {
                readedCount = stream.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < readedCount; )
                {
                    var curbyte = buffer[i];
                    var bytesCountPerChar = GetBytesCountForChar(curbyte);
                    if (bytesCountPerChar == -1)//continuation bytes check when updating buffer
                    {
                        i++;
                        continue;
                    }
                    count++;
                    i += bytesCountPerChar;
                }
            }
            while(readedCount != 0);

            return count;
        }

        private static int GetBytesCountForChar(byte firstByte)
        {
            if ((byte)(firstByte | (byte)UTF8BitMask.OneBit) == (byte)UTF8BitPattern.OneBit) return 1;
            if ((byte)(firstByte | (byte)UTF8BitMask.TwoBits) == (byte)UTF8BitPattern.TwoBits) return 2;
            if ((byte)(firstByte | (byte)UTF8BitMask.ThreeBits) == (byte)UTF8BitPattern.ThreeBits) return 3;
            if ((byte)(firstByte | (byte)UTF8BitMask.FourBits) == (byte)UTF8BitPattern.FourBits) return 4;
            if ((byte)(firstByte | (byte)UTF8BitMask.ContinuationByte) == (byte)UTF8BitPattern.ContinuationByte) return -1;
             
            throw new SystemException("Incorrect usage of bit mask!");
        }
    }

    [Flags]
    internal enum UTF8BitMask : byte
    {
        OneBit = 0b01111111,//0xxxxxxx
        TwoBits = 0b00011111,//110xxxxx
        ThreeBits = 0b00001111,//1110xxxx
        FourBits = 0b00000111,//11110xxx

        ContinuationByte = 0b00111111//10xxxxxx
    }

    [Flags]
    internal enum UTF8BitPattern : byte
    {
        OneBit = 0b01111111,//0xxxxxxx
        TwoBits = 0b11011111,//110xxxxx
        ThreeBits = 0b11101111,//1110xxxx
        FourBits = 0b11110111,//11110xxx

        ContinuationByte = 0b10111111,//10xxxxxx
    }
}