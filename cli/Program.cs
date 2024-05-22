
using System.Security.Policy;
using Stripe;

namespace Ecommerce
{
    class Program
    {
        // Ideally extract to a configuration file, or an environment file.
        static string API_KEY_SECRET = "sk_test_51PEwawBYQqea3LZFnARTWx0YOFopVH6NyFXp8ZOf0yBTH3ROGTyKt7ZtzpSTP9wRZsCGk9J5IhvFUxCDnf8MHEoV009sKRnoQD";
        static List<string> SUPPORTED_CURRENCIES = new List<string>{
            "USD",
            "CAD"
        };

        static void Main(string[] args)
        {
            StripeConfiguration.ApiKey = API_KEY_SECRET;

            if (args.Length <= 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("To use the CLI, add the path of your CSV to the command\n\nExample:\ndotnet run <path-of-csv>\n...");
                return;
            }

            if (!Path.Exists(args[0]))
            {
                Console.WriteLine("ERROR: The provided path is either invalid or the file doesn't exists...");
                return;
            }

            Console.WriteLine($"Opening CSV file \"{args[0]}\" for extraction...");

            var fileLines = System.IO.File.ReadAllLines(args[0]);
            var failedLines = new List<string>();
            var progressionCount = 1;
            var isFirstLine = true;

            foreach (var line in fileLines)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                Console.WriteLine($"Processing line ({progressionCount}/{fileLines.Count() - 1})");

                try
                {
                    var isSuccessful = ProcessCsvProductLine(line);

                    if (!isSuccessful)
                    {
                        failedLines.Add(line);
                    }
                }
                catch(Exception exception)
                {
                    Console.WriteLine($"ERROR: {exception}...");

                    failedLines.Add(line);
                }
                finally
                {
                    progressionCount++;
                }
            }

            if (failedLines.Count > 0)
            {
                Console.WriteLine("\nFailed to process the following lines...\n");
                failedLines.ForEach(line => Console.WriteLine($"{line}\n"));
            }

            Console.WriteLine("Process Completed!");
        }

        private static bool ProcessCsvProductLine(string line)
        {
            var data = line.Split(";");
            var id = data[0];
            var name = data[1];
            var description = data[2];
            var price = data[3];
            var currency = data[4];
            var active = data[5];
            var images = data
                .Skip(6)
                .Take(8)
                .Where(image => !string.IsNullOrWhiteSpace(image))
                .Where(image => Uri.IsWellFormedUriString(image, UriKind.Absolute))
                .ToList();

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine($"\nThe product with id {id} has no specified name, enter a name and press enter:");

                name = Console.ReadLine();

                Console.WriteLine();

                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }
            }

            if (!int.TryParse(price, out var unitPrice))
            {
                Console.WriteLine($"\nThe product with id {id} has an invalid price value, please enter a new one:");

                price = Console.ReadLine();

                if (!int.TryParse(price, out unitPrice))
                {
                    return false;
                }
            }

            if (unitPrice < 100)
            {
                Console.WriteLine($"\nThe product with id {id} has a price value under 1$, please confirm the amount by typing {price}:");

                price = Console.ReadLine();

                if (!int.TryParse(price, out unitPrice))
                {
                    return false;
                }
            }

            if (!SUPPORTED_CURRENCIES.Contains(currency))
            {
                Console.WriteLine(
                    $"\nThe product with id {id} has an invalid currency value, supported values are: [{string.Join(", ", SUPPORTED_CURRENCIES)}], please enter a new one:");

                currency = Console.ReadLine();

                if (!SUPPORTED_CURRENCIES.Contains(currency ?? string.Empty))
                {
                    return false;
                }
            }

            var service = new ProductService();
            
            service.Create(new ProductCreateOptions{
                Name = name,
                DefaultPriceData = new ProductDefaultPriceDataOptions {
                    Currency = currency,
                    UnitAmount = unitPrice,
                },
                Active = active == "TRUE",
                Description = description,
                Images = images
            });

            return true;
        }
    }
}