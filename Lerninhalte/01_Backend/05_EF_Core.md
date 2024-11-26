# EF Core

EF (EntityFramework Core) ist ein Object-Relational-Mapper (ORM) für .NET. Es ermöglicht die Interaktion mit einer Datenbank, ohne SQL schreiben zu müssen. EF Core ist eine leichtgewichtige, modulare Version von EF, die in .NET Core Anwendungen verwendet wird.

Unsere Applikation verwendet derzeit noch kein EF Core, sondern arbeitet mit ADO.NET. In diesem Auftrag werden wir EF Core in die Applikation integrieren.

Bauen sie die bestehende Applikation so um, dass sie EF Core verwendet.

[Installieren Sie EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)


[Erste Schritte mit EF Core](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli)

### Teilaufgabe 1: Anfragen anpassen

Versucht anhand dieses Tutorials die Anfragen in der Applikation so umzubauen, dass sie EF Core verwenden. -> [EF Core Querying](https://learn.microsoft.com/en-us/ef/core/querying/)

Wichtig: Bisher wurden die Balances neu geladen durch die Methode Ledger.GetBalance(). EF kann seine Objekte jederzeit neu laden.
context.Entry(from).Reload()

### Teilaufgabe 2: Transaktionen

Zuletzt muss auch hier eine Transaktion eingebaut werden: https://learn.microsoft.com/de-de/ef/core/saving/transactions

Wichtig zum Einbau der Transaktion: Damit ein Isolationslevel(Serializable, ReadCommited,…) angegeben werden kann, braucht es das NuGet-Package Microsoft.EntityFrameworkCore.Relational. Fügt dies eurem Projekt hinzu. Entweder über das Context-Menu bei Abhängigkeiten
Oder via Konsole mit dem Befehl:

dotnet add package Microsoft.EntityFrameworkCore.Relational