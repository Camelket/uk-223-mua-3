using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LBank.Tests.Loadtest.Cli
{
    class Program
    {
        // Erstelle einen HttpClient für die API-Anfragen
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            try
            {
                // Einloggen und JWT-Token erhalten
                string jwt = await Login("admin", "adminpass");

                // Hole Ledgers mithilfe des JWT-Tokens
                var ledgers = await GetAllLedgers(jwt);

                // Gebe alle Ledgers in der Konsole aus
                foreach (var ledger in ledgers)
                {
                    Console.WriteLine($"Ledger Name: {ledger.Name}, Balance: {ledger.Balance}");
                }

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Login-Methode, um das JWT-Token zu erhalten
        private static async Task<string> Login(string username, string password)
        {
            var url = "http://localhost:5290/api/auth/login"; // Verwende Port 5290 für HTTP

            var requestContent = new StringContent(
                JsonSerializer.Serialize(new { Username = username, Password = password }),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await client.PostAsync(url, requestContent);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(jsonResponse);
                return jsonDoc.RootElement.GetProperty("token").GetString(); // Extrahiere das JWT-Token
            }

            throw new Exception($"Login failed with status code: {response.StatusCode}\nError response: {await response.Content.ReadAsStringAsync()}");
        }

        // Methode zum Abrufen aller Ledgers
        private static async Task<LedgerDto[]> GetAllLedgers(string jwt)
        {
            var url = "http://localhost:5290/api/ledgers/all"; // Verwende Port 5290 für HTTP

            client.DefaultRequestHeaders.Add("Authorization", "Bearer "+jwt);
           

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LedgerDto[]>(jsonResponse) ?? Array.Empty<LedgerDto>();
            }
            else
            {
                Console.WriteLine($"Failed to retrieve ledgers with status code: {response.StatusCode}");
                string errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error response: {errorResponse}");
            }

            throw new Exception("Failed to retrieve ledgers");
        }

        // Definition der LedgerDto-Klasse um Ledgers korrekt zu deserialisieren
        public class LedgerDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty; // Leerer Wert, um Nullable-Warnung zu vermeiden
            public decimal Balance { get; set; }
        }
    }
}
