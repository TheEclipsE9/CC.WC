using System.Text;

namespace CC.WC.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            foreach(var arg in args)
            {
                Console.WriteLine(arg);
                Console.Beep();
            }

            Stream stdin = Console.OpenStandardInput();

            byte[] buffer = new byte[10];

            stdin.Read(buffer, 0, buffer.Length);

            var output = Encoding.UTF8.GetString(buffer);

            Console.WriteLine($"Got from std: {output}");
        }
    }
}