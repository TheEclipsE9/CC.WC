using System.Diagnostics;
using System.Text;

namespace CC.WC.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var param = args.Length == 0 ? String.Empty : args[0];
            var fileName = args.Length == 2 ? args[1] : String.Empty;

            Stream stream = null;
            
            if (!string.IsNullOrEmpty(fileName))
            {
                if (!File.Exists(fileName)) throw new ArgumentException("File doesn't exist in the current directory.");

                stream = File.OpenRead(fileName);
            }
            else
            {
                stream = Console.OpenStandardInput();
            }

            var context = new Context();
            switch(param)
            {
                case "-c":
                    context.SetCounter(new BytesCounter());
                    System.Console.WriteLine(context.Count(stream));
                    break;
                case "-l":
                    context.SetCounter(new LinesCounter());
                    System.Console.WriteLine(context.Count(stream));
                    break;
                case "-w":
                    context.SetCounter(new WordsCounter());
                    System.Console.WriteLine(context.Count(stream));
                    break;
                case "-m":
                    context.SetCounter(new CharactersCounter());
                    System.Console.WriteLine(context.Count(stream));
                    break;
                default:
                    var result = context.CountDefault(stream);
                    System.Console.WriteLine($"{result.Item1} {result.Item2} {result.Item3}");
                    break;
            }
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

    public class Context
    {
        private ICounter _counter;

        public void SetCounter(ICounter counter) => _counter = counter;

        public int Count(Stream stream)
        {
            if (_counter is null) throw new Exception("_counter is null.");

            int count = 0;
            int readedCount = 0;
            byte[] buffer = new byte[1_024];
            
            do
            {
                readedCount = stream.Read(buffer, 0, buffer.Length);
                count += _counter.Count(buffer, readedCount);
            }
            while(readedCount != 0);

            var wordsCounter = _counter as WordsCounter;
            if (wordsCounter is not null)
            {
                if (wordsCounter.IsPrevLetter) count++;
            }

            return count;
        }

        public (int, int, int) CountDefault(Stream stream)
        {
            int linesCount = 0;
            int wordsCount = 0;
            int bytesCount = 0;
            
            int readedCount = 0;
            byte[] buffer = new byte[1_024];

            var linesCounter = new LinesCounter();
            var wordsCounter = new WordsCounter();
            var bytesCounter = new BytesCounter();
            
            do
            {
                readedCount = stream.Read(buffer, 0, buffer.Length);
                linesCount += linesCounter.Count(buffer, readedCount);
                wordsCount += wordsCounter.Count(buffer, readedCount);
                bytesCount += bytesCounter.Count(buffer, readedCount);
            }
            while(readedCount != 0);

            if (wordsCounter.IsPrevLetter) wordsCount++;

            return (linesCount, wordsCount, bytesCount);
        }
    }

    public interface ICounter
    {
        int Count(byte[] buffer, int readedCount);
    }

    public class BytesCounter : ICounter
    {
        public int Count(byte[] buffer, int readedCount)
        {
            return readedCount;
        }
    }

    public class LinesCounter : ICounter
    {
        public int Count(byte[] buffer, int readedCount)
        {
            int count = 0;

            for(int i = 0; i < readedCount; i++)
            {
                var c = (char)buffer[i];
                if(Char.IsControl(c) && c == '\n')//only unix handling, but for win its '\r\n', so anyway should work< cause \n is last? 
                {
                    count++;
                }
            }

            return count;
        }
    }

    public class WordsCounter : ICounter
    {
        public bool IsPrevLetter => _isPrevLetter;
        private bool _isPrevLetter = false;

        public int Count(byte[] buffer, int readedCount)
        {
            int count = 0;
                
            for(int i = 0; i < readedCount; i++)
            {
                var curChar = (char)buffer[i];
                if(Char.IsWhiteSpace(curChar) || 
                    Char.IsSeparator(curChar) ||
                    (Char.IsControl(curChar) && curChar == '\n'))//unix only, in win is \r\n for new line
                {
                    if (_isPrevLetter) count++;

                    _isPrevLetter = false;
                }
                else
                {
                    _isPrevLetter = true;
                }
            }

            return count;
        }
    }

    public class CharactersCounter : ICounter
    {
        public int Count(byte[] buffer, int readedCount)
        {
            int count = 0;
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
}