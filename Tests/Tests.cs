namespace Tests
{
    [TestClass]
    public sealed class Tests
    {
        private string RollRaw(string[] args, int expectedReturnCode)
        {
            var consoleOut = Console.Out;

            using var stdout = new StringWriter();
            stdout.NewLine = "\n";
            Console.SetOut(stdout);

            try
            {
                var returnCode = Program.Main(args);
                Assert.AreEqual(
                    expectedReturnCode,
                    returnCode,
                    $"Returkod {expectedReturnCode} förväntades för argumenten \"{string.Join(' ', args)}\""
                );
                stdout.Flush();
                return stdout.ToString();
            }
            finally
            {
                Console.SetOut(consoleOut);
            }
        }

        private int Roll(string[] args, int expectedReturnCode)
        {
            var rawOutput = RollRaw(args, expectedReturnCode);
            if (string.IsNullOrWhiteSpace(rawOutput))
            {
                Assert.Fail("Programmet förväntades skriva en rad med resultatet till skärmen");
                return -1;
            }
            var output = rawOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (output.Length != 1)
            {
                Assert.Fail($"Programmet förväntades skriva en rad med resultatet till skärmen, men {output.Length} rader skrevs ut istället");
                return -1;
            }
            if (!int.TryParse(output[0], out var result))
            {
                Assert.Fail($"Ett resultat föräntades skrivas till skärmen, \"{output[0]}\" gick inte att tolka som en siffra");
                return -1;
            }
            return result;
        }

        private void ValidateRoll(int numberOfDice, int size, char d = 'D')
        {
            var roll = $"{numberOfDice}{d}{size}";
            var allResults = new int[50000];
            for (int i = 0; i < allResults.Length; i++)
            {
                var result = Roll([roll], 0);
                allResults[i] = result;
            }
            var minResult = numberOfDice;
            var maxResult = numberOfDice * size;
            var invalidResults = allResults
                .Distinct()
                .Where(x => x < numberOfDice || x > maxResult)
                .ToArray()
                ;
            if (invalidResults.Length > 0)
            {
                var values = string.Join("\n", [.. invalidResults.Select(x => x.ToString())]);
                Assert.Fail($"Rullningen \"{roll}\" skrev ut följande ogiltiga värden:\n{values}");
            }

            var grouped = allResults
                .GroupBy(x => x)
                .Select(x => new
                {
                    Value = x.Key,
                    Count = x.Count(),
                })
                .OrderBy(x => x.Value)
                .ToArray();

            int numberOfPossibleValues = (maxResult - minResult) + 1;
            if (numberOfDice == 1)
            {
                var center = (float)allResults.Length / numberOfPossibleValues;
                var min = Math.Round(center * 0.8f);
                var max = Math.Round(center / 0.8f);

                foreach (var group in grouped)
                {
                    if (group.Count < min || group.Count > max)
                    {
                        Assert.Fail($"{group.Value} förväntades dyka upp mellan {min} och {max} gånger, men dök upp {group.Count} gånger för rullningen \"{roll}\"");
                    }
                }
            }
            else
            {
                int minNumberOfValues = Math.Max(10, numberOfPossibleValues / 2);
                if (grouped.Length < minNumberOfValues)
                {
                    Assert.Fail($"Spridningen på värden var för liten för rullningen \"{roll}\", åtminstone {minNumberOfValues} unika värden borde ha dykt upp");
                }
                var mean = 0f;
                foreach (var r in allResults) mean += r;
                mean /= allResults.Length;
                var expectedMean = minResult + (maxResult * 0.5f);
                var min = expectedMean * 0.7f;
                var max = expectedMean / 0.7f;
                if (mean < min || mean > max)
                {
                    Assert.Fail($"Snittresultatet för \"{roll}\" förväntades vara mellan {min} och {max}, men det blev {mean}");
                }
            }
        }

        [TestMethod("Returkod 1 om inga argument anges")]
        public void No_args_returns_1()
        {
            RollRaw([], 1);
        }

        [TestMethod("Returkod 1 för ett ogiltigt argument")]
        public void Invalid_argument_returns_1()
        {
            RollRaw([""], 1);
            RollRaw(["inte en tärning"], 1);
            RollRaw(["felaktigt"], 1);
            RollRaw(["ej korrekt"], 1);
            RollRaw(["nope"], 1);
        }

        [TestMethod("Returkod 1 för inkompletta tärningar")]
        public void Invalid_roll_returns_1()
        {
            RollRaw(["1D"], 1);
            RollRaw(["D4"], 1);
            RollRaw(["D100"], 1);
            RollRaw(["100D"], 1);
            RollRaw(["D"], 1);
        }

        [TestMethod("Returkod 1 om mer än ett argument anges")]
        public void Invalid_arguments_returns_1()
        {
            RollRaw(["2D20", "2D20", "2D20"], 1);
        }

        [TestMethod("En tärning")]
        public void Roll_one()
        {
            for (int i = 2; i < 100; i++)
            {
                ValidateRoll(1, i);
            }
        }

        [TestMethod("Många tärningar")]
        public void Roll_many()
        {
            for (int i = 2; i < 100; i++)
            {
                ValidateRoll(10, i);
            }
        }

        [TestMethod("Stora och små bokstäver")]
        public void Upper_and_lowercase()
        {
            ValidateRoll(1, 20, 'D');
            ValidateRoll(1, 20, 'd');
        }
    }
}
