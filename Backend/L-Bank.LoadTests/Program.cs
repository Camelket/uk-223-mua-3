using System.Net.Http.Headers;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Newtonsoft.Json;

Console.WriteLine("Load-Testing L-Bank.Api...");
Console.WriteLine();

try
{
    using var client = new HttpClient();
    client.BaseAddress = new("http://localhost:5290");
    client.DefaultRequestHeaders.Add("Accept", "application/json");

    Console.WriteLine("Preparing for Load-Tests");
    Console.WriteLine("...\n");

    Console.WriteLine("Getting Access-Token with Admin-Permissions");
    Console.WriteLine("...\n");

    var token = await GetAdminAccessToken(client);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

    Console.WriteLine("Creating Data for Test-Scenario");
    Console.WriteLine("...\n");

    var data = await PrepareTestData(client);

    Console.WriteLine("Getting Total Money in System before Load-Tests");
    Console.WriteLine("...\n");

    var preTotalMoney = await GetTotalMoneyInSystem(client);

    Console.WriteLine("Starting NBomber Load-Tests");
    Console.WriteLine("...\n");

    var scenario = CreateScenario(client, data);
    NBomberRunner
        .RegisterScenarios(scenario)
        .WithReportFileName("reports")
        .WithReportFolder("reports")
        .WithReportFormats(ReportFormat.Html)
        .Run();

    Console.WriteLine("Getting Total Money in System after Load-Tests");
    Console.WriteLine("...\n");

    var postTotalMoney = await GetTotalMoneyInSystem(client);

    Console.WriteLine(
        $"Starting Money-Total: {preTotalMoney} :: Ending Money-Total: {postTotalMoney} :: Difference: {preTotalMoney - postTotalMoney}"
    );
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred while calling the API:");
    Console.WriteLine(ex.Message);
}

Console.WriteLine("Press any key to exit");
Console.ReadKey();
return;

static string RandomString(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    return new string(
        Enumerable
            .Repeat(chars, length)
            .Select(s => s[Randomizer.GetRandom().Next(s.Length)])
            .ToArray()
    );
}

static HttpContent ToHttpContent<T>(T data)
{
    var payload = JsonConvert.SerializeObject(data);
    var content = new StringContent(payload.ToString());
    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    return content;
}

static async Task<string> GetAdminAccessToken(HttpClient client)
{
    var payload = new LoginRequest() { Username = "Admin", Password = "adminpass" };
    var response = await client.PostAsync("/api/auth/login", ToHttpContent(payload));

    response.EnsureSuccessStatusCode();
    var login = JsonConvert.DeserializeObject<LoginResponse>(
        await response.Content.ReadAsStringAsync()
    );
    return login?.Token ?? "";
}

static async Task<TestScenarioData> PrepareTestData(HttpClient client)
{
    // Create Test-Ledgers
    List<LedgerResponse> ledgers = [];

    for (int i = 0; i < Randomizer.GetRandom().Next(5, 40); i++)
    {
        var payload = new LedgerRequest() { Name = RandomString(10) };
        var response = await client.PostAsync("/api/ledgers", ToHttpContent(payload));

        var ledger = JsonConvert.DeserializeObject<LedgerResponse>(
            await response.Content.ReadAsStringAsync()
        );
        if (ledger != null)
        {
            ledgers.Add(ledger);
        }
    }

    // Deposit random amount of money
    foreach (var ledger in ledgers)
    {
        var payload = new DepositRequest()
        {
            Amount = Randomizer.GetRandom().Next(1000000, 100000000),
            LedgerId = ledger.Id,
        };
        var response = await client.PostAsync("/api/deposits", ToHttpContent(payload));
    }

    return new TestScenarioData() { ledgers = ledgers };
}

static BookingRequest RandomBookingRequest(TestScenarioData data)
{
    Random random = Randomizer.GetRandom();

    int sourceId = data.ledgers[random.Next(0, data.ledgers.Count)].Id;
    int targetId = 0;

    do
    {
        targetId = data.ledgers[random.Next(0, data.ledgers.Count)].Id;
    } while (sourceId == targetId);

    return new BookingRequest()
    {
        SourceId = sourceId,
        TargetId = targetId,
        Amount = random.Next(1, 10),
        // Amount = random.NextDecimal(decimal.Zero + 0.001M, 432),
    };
}

static async Task<decimal> GetTotalMoneyInSystem(HttpClient client)
{
    var response = await client.GetAsync("/api/ledgers/total");
    response.EnsureSuccessStatusCode();

    return JsonConvert.DeserializeObject<decimal>(await response.Content.ReadAsStringAsync());
}

static ScenarioProps CreateScenario(HttpClient client, TestScenarioData data)
{
    return Scenario
        .Create(
            "request_transaction",
            async _ =>
            {
                var payload = RandomBookingRequest(data);
                var request = Http.CreateRequest("POST", "/api/bookings")
                    .WithBody(ToHttpContent(payload));

                var response = await Http.Send(client, request);
                return response;
            }
        )
        .WithoutWarmUp()
        // .WithWarmUpDuration(TimeSpan.FromSeconds(10))
        .WithLoadSimulations(
            Simulation.Inject(
                rate: 800,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(30)
            )
        );
}

class Randomizer
{
    private static Random? random;

    private Randomizer() { }

    public static Random GetRandom()
    {
        random ??= new Random();
        return random;
    }
}

static class RandomExtensions
{
    public static decimal NextDecimal(this Random rnd, decimal from, decimal to)
    {
        byte fromScale = new System.Data.SqlTypes.SqlDecimal(from).Scale;
        byte toScale = new System.Data.SqlTypes.SqlDecimal(to).Scale;

        byte scale = (byte)(fromScale + toScale);
        if (scale > 28)
            scale = 28;

        decimal r = new decimal(rnd.Next(), rnd.Next(), rnd.Next(), false, scale);
        if (Math.Sign(from) == Math.Sign(to) || from == 0 || to == 0)
            return decimal.Remainder(r, to - from) + from;

        bool getFromNegativeRange =
            (double)from + rnd.NextDouble() * ((double)to - (double)from) < 0;
        return getFromNegativeRange ? decimal.Remainder(r, -from) + from : decimal.Remainder(r, to);
    }
}

class TestScenarioData
{
    public List<LedgerResponse> ledgers = [];
}

class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

class DepositRequest
{
    public decimal Amount { get; set; }
    public int LedgerId { get; set; }
}

class BookingRequest
{
    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public decimal Amount { get; set; }
}

class LedgerRequest
{
    public string? Name { get; set; }
}

class LoginResponse
{
    public string? Token { get; set; }
}

class LedgerResponse
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Balance { get; set; }
}

class BookingResponse
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public string? SourceName { get; set; }
    public int TargetId { get; set; }
    public string? TargetName { get; set; }
    public decimal TransferedAmount { get; set; }

    public DateTime Date { get; set; }
}

class DepositResponse
{
    public int DepositId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int LedgerId { get; set; }
    public string? LedgerName { get; set; }
}
