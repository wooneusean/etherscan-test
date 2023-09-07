using Google.Protobuf.Collections;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Etherscan_Coding_Test
{
    public static class DatabaseManager
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["EtherscanDatabase"].ConnectionString;

        public static List<Token> GetTokens()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string query = "SELECT *, (total_supply / (SELECT SUM(total_supply) FROM `main`.`token`)) * 100 as total_supply_percentage, RANK() OVER(ORDER BY total_supply DESC) `rank` FROM `main`.`token`";

                using (var command = new MySqlCommand(query, con))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var tokens = new List<Token>();
                        while (reader.Read())
                        {
                            var token = new Token
                            {
                                Id = reader.GetInt32(0),
                                Symbol = reader.GetString(1),
                                Name = reader.GetString(2),
                                TotalSupply = reader.GetInt64(3),
                                ContractAddress = reader.GetString(4),
                                TotalHolders = reader.GetInt32(5),
                                Price = reader.GetDecimal(6),
                                TotalSupplyPercentage = reader.GetDecimal(7),
                                Rank = reader.GetInt32(8),
                            };
                            tokens.Add(token);
                        }
                        return tokens;
                    }
                }
            }
        }

        public static PaginationDetails GetPaginationDetails(int perPage)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string query = "SELECT COUNT(*) FROM `main`.`token`"; // Replace with your table name

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    // ExecuteScalar returns the first column of the first row as an object
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return new PaginationDetails
                        {
                            totalPages = (int)Math.Ceiling(Convert.ToInt32(result) / (decimal)perPage),
                            totalRecords = Convert.ToInt32(result)
                        };
                    }
                    else
                    {
                        return new PaginationDetails { totalRecords = 0, totalPages = 0 };
                    }
                }
            }
        }

        public static bool UpsertToken(Token token)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlCommand cmd;
                string query;
                if (token.Id < 0)
                {
                    query = "INSERT INTO `main`.`token` (symbol, name, total_supply, contract_address, total_holders) " +
                        "VALUES(@Symbol, @Name, @TotalSupply, @ContractAddress, @TotalHolders); ";

                    cmd = new MySqlCommand(query, connection);

                    cmd.Parameters.AddWithValue("@Symbol", token.Symbol);
                    cmd.Parameters.AddWithValue("@Name", token.Name);
                    cmd.Parameters.AddWithValue("@TotalSupply", token.TotalSupply);
                    cmd.Parameters.AddWithValue("@ContractAddress", token.ContractAddress);
                    cmd.Parameters.AddWithValue("@TotalHolders", token.TotalHolders);
                }
                else
                {
                    query = "UPDATE `main`.`token` SET symbol = @Symbol, name = @Name, total_supply = @TotalSupply, " +
                                "contract_address = @ContractAddress, total_holders = @TotalHolders " +
                                "WHERE id = @Id";

                    cmd = new MySqlCommand(query, connection);

                    cmd.Parameters.AddWithValue("@Id", token.Id);
                    cmd.Parameters.AddWithValue("@Symbol", token.Symbol);
                    cmd.Parameters.AddWithValue("@Name", token.Name);
                    cmd.Parameters.AddWithValue("@TotalSupply", token.TotalSupply);
                    cmd.Parameters.AddWithValue("@ContractAddress", token.ContractAddress);
                    cmd.Parameters.AddWithValue("@TotalHolders", token.TotalHolders);
                }

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static Token GetTokenBySymbol(string symbol)
        {
            if (symbol.Trim() != "")
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM `main`.`token` WHERE symbol = @Symbol";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    cmd.Parameters.AddWithValue("@Symbol", symbol);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Token
                            {
                                Id = reader.GetInt32(0),
                                Symbol = reader.GetString(1),
                                Name = reader.GetString(2),
                                TotalSupply = reader.GetInt64(3),
                                ContractAddress = reader.GetString(4),
                                TotalHolders = reader.GetInt32(5),
                                Price = reader.GetDecimal(6)
                            };
                        }
                    }
                }
            }
            return new Token();
        }

        public static void CreateTableIfNotExists()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
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
}