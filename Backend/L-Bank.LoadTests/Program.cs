using System.Net.Http.Headers;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Newtonsoft.Json;

const string ENDPOINT_QUEUE = "/api/bookings";
const string ENDPOINT_PROCEDURE = "/api/bookings/procedure";
const string ENDPOINT_BOTH = "/api/bookings/procedure/queue";

Console.WriteLine("Load-Testing L-Bank.Api...");
Console.WriteLine();

try
{
    using var client = new HttpClient();
    client.BaseAddress = new("http://localhost:5290");
    client.DefaultRequestHeaders.Add("Accept", "application/json");

    Console.WriteLine("Getting Inputs\n");
    Console.WriteLine("!! IMPORTANT !!");
    Console.WriteLine("Set Log-Level in API (appsettings) to Error\n");

    Console.Write("Generate Test-Data: (y/n) ");
    string? answer = Console.ReadLine();
    bool generate = answer == "y" || answer == "Y";

    Console.Write("Enter target RPS: ");
    string? rpsString = Console.ReadLine();
    int rps = Int32.Parse(rpsString);

    Console.Write("Enter target Timespan in Seconds: ");
    string? timespanString = Console.ReadLine();
    int time = Int32.Parse(timespanString);

    Console.WriteLine("Preparing for Load-Tests");
    Console.WriteLine("...\n");

    Console.WriteLine("Getting Access-Token with Admin-Permissions");
    Console.WriteLine("...\n");

    var token = await GetAdminAccessToken(client);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

    TestScenarioData data;
    if (generate)
    {
        Console.WriteLine("Creating Data for Test-Scenario");
        Console.WriteLine("...\n");

        data = await PrepareTestData(client);
    }
    else
    {
        Console.WriteLine("Getting Data for Test-Scenario");
        Console.WriteLine("...\n");

        data = await GetTestData(client);
    }

    Console.WriteLine("Getting Total Money in System before Load-Tests");
    Console.WriteLine("...\n");

    var preTotalMoney = await GetTotalMoneyInSystem(client);

    Console.WriteLine("Starting NBomber Load-Tests");
    Console.WriteLine("...\n");

    foreach (
        string endpoint in new List<string>() { ENDPOINT_QUEUE, ENDPOINT_PROCEDURE, ENDPOINT_BOTH }
    )
    {
        NBomberRunner
            .RegisterScenarios(_CreateScenario(client, data, rps, time, endpoint))
            .WithReportFileName($"reports-{endpoint.Split("/").Last()}")
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

static async Task<TestScenarioData> GetTestData(HttpClient client)
{
    List<LedgerResponse> ledgers = [];

    var existingResponse = await client.GetAsync("/api/ledgers");
    var requestedledgers = JsonConvert.DeserializeObject<LedgerResponse[]>(
        await existingResponse.Content.ReadAsStringAsync()
    );

    ledgers.AddRange(requestedledgers);

    return new TestScenarioData() { ledgers = ledgers };
}

static async Task<TestScenarioData> PrepareTestData(HttpClient client)
{
    List<LedgerResponse> ledgers = [];

    Console.WriteLine("Creating random Amount of Ledgers (100 - 1000)");
    Console.WriteLine("...\n");

    for (int i = 0; i < Randomizer.GetRandom().Next(100, 1000); i++)
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

    Console.WriteLine("Depositing random Amount of Money to each Ledger (1000 - 100000)");
    Console.WriteLine("...\n");

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

    int sourceId = data.ledgers[random.Next(0, data.ledgers.Count - 1)].Id;
    int targetId = 0;

    do
    {
        targetId = data.ledgers[random.Next(0, data.ledgers.Count - 1)].Id;
    } while (sourceId == targetId);

    return new BookingRequest()
    {
        SourceId = sourceId,
        TargetId = targetId,
        Amount = random.Next(1, 500),
    };
}

static async Task<decimal> GetTotalMoneyInSystem(HttpClient client)
{
    var response = await client.GetAsync("/api/ledgers/total");
    response.EnsureSuccessStatusCode();

    return JsonConvert.DeserializeObject<decimal>(await response.Content.ReadAsStringAsync());
}

static ScenarioProps _CreateScenario(
    HttpClient client,
    TestScenarioData data,
    int rps,
    int time,
    string endpoint
)
{
    return Scenario
        .Create(
            $"request_transaction-{endpoint}",
            async _ =>
            {
                var payload = RandomBookingRequest(data);
                var request = Http.CreateRequest("POST", endpoint).WithBody(ToHttpContent(payload));

                var response = await Http.Send(client, request);
                return response;
            }
        )
        .WithoutWarmUp()
        // .WithWarmUpDuration(TimeSpan.FromSeconds(10))
        .WithLoadSimulations(
            Simulation.Inject(
                rate: rps,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(time)
            )
        );
    ;
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
