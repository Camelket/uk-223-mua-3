using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using NBomber.CSharp;
using NBomber.Http.CSharp;
using NBomber.Contracts.Stats; // Importiert die ReportFormat Definition

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
                Console.WriteLine("Generated JWT Token: " + jwt);

                // Hole Ledgers mithilfe des JWT-Tokens (normaler Abruf zum Testen)
                var ledgers = await GetAllLedgers(jwt);

                // Gebe alle Ledgers in der Konsole aus
                foreach (var ledger in ledgers)
                {
                    Console.WriteLine($"Ledger Name: {ledger.Name}, Balance: {ledger.Balance}");
                }

                // NBomber-Load-Test für den gleichen Endpoint
                using var httpClient = new HttpClient();

                var scenario = Scenario.Create("http_scenario", async context =>
                {
                    // NBomber-Anfrage erstellen und JWT-Token in den Header einfügen
                    var request = Http.CreateRequest("GET", "http://localhost:5290/api/ledgers/all")
                        .WithHeader("Authorization", $"Bearer {jwt}")
                        .WithHeader("Accept", "application/json");

                    // Logging hinzufügen, um zu überprüfen, ob der JWT-Token gesetzt ist
                    Console.WriteLine("NBomber Request: Authorization Header Set");

                    var response = await Http.Send(httpClient, request);
                    Console.WriteLine($"Response Status Code: {response.StatusCode}");

                    return response;
                })
                .WithoutWarmUp()
                .WithLoadSimulations(
                    Simulation.Inject(rate: 100,
                                      interval: TimeSpan.FromSeconds(1),
                                      during: TimeSpan.FromSeconds(30))
                );

                // NBomber-Szenario ausführen und Report generieren
                NBomberRunner
                    .RegisterScenarios(scenario)
                    .WithReportFileName("fetch_users_report")
                    .WithReportFolder("fetch_users_reports")
                    .WithReportFormats(ReportFormat.Html) // ReportFormat hinzugefügt
                    .Run();

                Console.WriteLine("Press any key to exit after load test");
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

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

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