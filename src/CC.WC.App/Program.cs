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
                default:
                    System.Console.WriteLine(0);
                    break;
            }
        }

        public static int CountBytes(Stream stream)
        {
            int count = 0;

            byte[] buffer = new byte[10];

            int readedCount = 0;
            do
            {
                readedCount = stream.Read(buffer, 0, buffer.Length);
                count += readedCount;
            }
            while(readedCount != 0);

            return count;
        }
    }
}