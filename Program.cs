using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace MockGen
{
    class Program
    {
        static void Main()
        {
            string dbPath = @"C:\Users\alice\OneDrive\Desktop\recordpulse\MockContacts.db";

            if (File.Exists(dbPath)) File.Delete(dbPath);

            string[] firstNames = { "James", "Mary", "Robert", "Patricia", "John", "Jennifer", "Michael", "Linda", "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez" };
            string[] jobs = { "Systems Engineer", "Project Manager", "DevOps Specialist", "Account Executive", "Data Analyst", "Operations Director" };
            string[] cities = { "Denver, CO", "Austin, TX", "Seattle, WA", "Chicago, IL", "Phoenix, AZ", "Atlanta, GA" };
            string[] domains = { "gmail.com", "outlook.com", "proton.me", "corpnet.com" };

            using (var conn = new SqliteConnection($"Data Source={dbPath};"))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Records (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Email TEXT,
                            Phone TEXT,
                            Job TEXT,
                            Address TEXT,
                            CustomJson TEXT,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                        );";
                    cmd.ExecuteNonQuery();
                }

                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            INSERT INTO Records (Name, Email, Phone, Job, Address, CustomJson)
                            VALUES (@Name, @Email, @Phone, @Job, @Address, @CustomJson);";

                        var pName = cmd.Parameters.Add("@Name", SqliteType.Text);
                        var pEmail = cmd.Parameters.Add("@Email", SqliteType.Text);
                        var pPhone = cmd.Parameters.Add("@Phone", SqliteType.Text);
                        var pJob = cmd.Parameters.Add("@Job", SqliteType.Text);
                        var pAddress = cmd.Parameters.Add("@Address", SqliteType.Text);
                        var pCustom = cmd.Parameters.Add("@CustomJson", SqliteType.Text);

                        Random rng = new Random(42);

                        for (int i = 1; i <= 1000; i++)
                        {
                            string fName = firstNames[rng.Next(firstNames.Length)];
                            string lName = lastNames[rng.Next(lastNames.Length)];

                            pName.Value = $"{fName} {lName}";
                            pEmail.Value = $"{fName.ToLower()}.{lName.ToLower()}{rng.Next(10, 99)}@{domains[rng.Next(domains.Length)]}";
                            pPhone.Value = $"({rng.Next(200, 999)}) {rng.Next(200, 999)}-{rng.Next(1000, 9999)}";
                            pJob.Value = jobs[rng.Next(jobs.Length)];
                            pAddress.Value = $"{rng.Next(100, 9999)} Main St, {cities[rng.Next(cities.Length)]}";
                            pCustom.Value = (i % 4 == 0) ? "{\"Priority\":\"High\",\"Status\":\"Active\"}" : "{}";

                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }

            Console.WriteLine($"MockContacts.db created successfully at: {dbPath}");
        }
    }
}