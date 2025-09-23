using System.Text.RegularExpressions;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Användning: <antal tärningar>d<antal sidor> (t.ex. 2d6 eller 3D20)");
            return 1;
        }

        var träff = Regex.Match(args[0], @"^(\d+)[dD](\d+)$");
        if (!träff.Success ||
            !int.TryParse(träff.Groups[1].Value, out int antalTärningar) ||
            !int.TryParse(träff.Groups[2].Value, out int antalSidor))
        {
            Console.WriteLine("Felaktiga argument. Ange t.ex. 2d6 eller 3D20");
            return 1;
        }

        if (antalTärningar < 1 || antalSidor < 2)
        {
            Console.WriteLine("Ogiltig inmatning");
            return 1;
        }

        var random = new Random();
        int summa = 0;
        for (int i = 0; i < antalTärningar; i++)
        {
            int kast = random.Next(1, antalSidor + 1);
            summa += kast;
        }
        Console.WriteLine(summa);
        return 0;       
    }
}

