using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace MyProject;

class Program
{
    static void Main(string[] args)
    {
        // Create the table if it dont exist.
        CreateTableIfNotExists();

        // Timer to run every 5 minutes to update prices
        Timer timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

        // Keep the application running
        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();
    }

    private static void TimerCallback(object? state)
    {
        Console.WriteLine($"[{DateTime.Now}] Starting to fetch prices...");

        using (MySqlConnection con = GetConnection())
        {
            con.Open();

            // Get list of symbols from database.

            List<string> symbolsList = new List<string>();

            string query = "SELECT GROUP_CONCAT(symbol SEPARATOR ',') AS symbols FROM `main`.`token`;";
            using (var command = new MySqlCommand(query, con))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Get the comma-separated values as a string
                        string? symbolsString = reader["symbols"].ToString();

                        if (symbolsString == null || symbolsString.Trim() == "")
                        {
                            Console.WriteLine($"[{DateTime.Now}] Found no symbols to fetch prices for.");
                            return;
                        };

                        // Split the string into a List<string>
                        symbolsList = symbolsString.Split(',').ToList();
                    }
                }
            }


            Console.WriteLine($"[{DateTime.Now}] Found {symbolsList.Count} symbols to fetch pricing details.");

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                // Make a request to get prices
                Console.WriteLine($"[{DateTime.Now}] Fetching data from CryptoCompare...");

                client.BaseAddress = new Uri("https://min-api.cryptocompare.com/data/");
                HttpResponseMessage response = client.GetAsync($"pricemulti?fsyms={string.Join(",", symbolsList)}&tsyms=USD").Result;
                response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result;

                Dictionary<string, Dictionary<string, decimal>>? currencyRates = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, decimal>>>(result);


                Console.WriteLine($"[{DateTime.Now}] CryptoCompare found {currencyRates.Count} prices to update.");

                // Is something went wrong with the request, don't do anything.
                if (currencyRates == null) return;

                // Prepare the update statement for updating prices.
                string update = "UPDATE `main`.`token` SET price = CASE symbol ";

                foreach (string symbol in symbolsList)
                {
                    if (currencyRates.ContainsKey(symbol))
                    {
                        update += $"WHEN '{symbol}' THEN {currencyRates[symbol]["USD"]} ";
                    }
                    else
                    {
                        update += $"WHEN '{symbol}' THEN 0.0 ";
                    }
                }

                update += $"END WHERE symbol IN ({string.Join(",", symbolsList.Select(s => $"'{s}'"))});";

                // Run the update.
                using (MySqlCommand cmd = new MySqlCommand(update, con))
                {
                    Console.WriteLine($"[{DateTime.Now}] Flushing data to database...");
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"[{DateTime.Now}] Done!");
                }
            }
        }
    }

    public static MySqlConnection GetConnection()
    {
        string connectionString = "server=localhost;port=3306;user=root;password=pass;allowPublicKeyRetrieval=true;sslmode=none;";
        return new MySqlConnection(connectionString);
    }

    /// <summary>
    /// Creates the database and the table if not exists.
    /// </summary>
    public static void CreateTableIfNotExists()
    {
        using (MySqlConnection connection = GetConnection())
        {
            connection.Open();

            string createDbQuery = "CREATE DATABASE IF NOT EXISTS `main`";

            using (MySqlCommand cmd = new MySqlCommand(createDbQuery, connection))
            {
                cmd.ExecuteNonQuery();
            }

            string createTableQuery = @"CREATE TABLE IF NOT EXISTS `main`.`token` (
                                                `id` INT(11) NOT NULL AUTO_INCREMENT,
                                                `symbol` VARCHAR(5) NOT NULL COLLATE 'utf8_general_ci',
                                                `name` VARCHAR(50) NOT NULL COLLATE 'utf8_general_ci',
                                                `total_supply` BIGINT(20) NOT NULL,
                                                `contract_address` VARCHAR(66) NOT NULL COLLATE 'utf8_general_ci',
                                                `total_holders` INT(11) NOT NULL,
                                                `price` DECIMAL(65,2) NULL DEFAULT '0.00',
                                                PRIMARY KEY (`id`) USING BTREE
                                            )
                                            COLLATE='utf8_general_ci'
                                            ENGINE=InnoDB
                                            ROW_FORMAT=DYNAMIC;";

            using (MySqlCommand cmd = new MySqlCommand(createTableQuery, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}