# 223-ma-app

> Projektmitglieder: Tahir Krasniqi, Olivier König, Nico Heinimann

## Fallbeispiel

Unsere Applikation implementiert das Feature, dass Geld von einem Konto auf ein anderes überwiesen werden kann. Alle erfolgten Transaktionen werden auf der Datenbank als `Bookings` gespeichert.

Die Implementation erfolgt nach dem Muster:

- Controller / Endpoint (`POST /api/bookings`)
- BankService für die Implementation der Logik
- Repository für die Interaktion mit der Datenbank über EF-Core

## Aufbau der Applikation

- L-Bank.Core: Abbildung der Domain-Entitäten
- L-Bank.DbAccess: Abbildung der Domain-Entitäten auf die Datenbank und Logik zu deren Abfrage (über EF-Core)
- L-Bank.Api: Definition der REST-Api-Schnittstelle in Controllern und Umsetzung der Business-Logik über Services (BankService und AuthService) sowie Sicherung der Endpoints

- L-Bank.Cli: Enthält das alte, erste CLI-Projekt
- LBank.Cli2: Enthält ein neues CLI-Projekt als POC mit sauber implementiertem DI-Provider

- L-Bank.Tests: Enthält Unit-Tests für die Applikation
- L-Bank.LoadTests: Enthält LoadTests für die Applikation

## Zusätzliche Features

Für die Applikation wurden unterschiedliche zusätzliche Features und Erweiterungen entwickelt.
- Backend: Deposits und Whitdrawls
- Backend: Einbindung von Stored-Procedures
- Backend: Performance-Optimierungen für Transaktionen bei hoher Last über `QueueTransactionProcessingV2`
- Frontend: Deposits und Withdrawls ausführen
- Frontend: Übersicht der eigenen Konten als Liste
- Frontend: Übersicht der erfolgten Bookings (Transaktions-History)
- Frontend: Übersicht der erfolgten Deposits und Withdrawls (Deposit-History)

Das Feature "Deposits und Whitdrawls" erlaubt es Usern Geld in das System einzuführen (Einzahlung) oder von der Bank abzuheben. Alle erfolgten Deposits sowie Withdrawls werden analog zu den Bookings festgehalten und in der Datenbank persistiert. Die Ausführung von Deposits und Withdrawls erfolgt in einer Transaktion. Dadurch wird sichergestellt, dass der Betrag (Balance eines Kontos) nur abgezogen / erhöht werden kann, insofern dieser Vorgang als Eintrag in der Deposits-Tabelle festgehalten wird. Ebenfalls wird bei Withdrawls überprüft, ob genügend Geld vorhanden ist. Bei Transient-Errors (wenn z.B. mehrere User gleichzeitig vom gleichen Konto Geld abheben möchten und die eine Transaktion blockiert wird) wird die Operation mittels konfigurierter Retry-Strategy neu gestartet. 

Die Methode `_DepositOrWithdrawl` implementiert die konkrete Logik - die Transaktion sowie Retry-Strategie wird übergeordnet von der Methode `MakeDeposit` verwaltet. Es wurden ein Positiv-Test und drei Negativ-Tets für `_DepositOrWithdrawl` geschrieben. Mit den Negativ-Tests wird die Transaktionssicherheit der Deposits getestet. Bei einem unerfolgreichen Resultat geschieht ein Rollback der Transaktion in der übergeordneten Methode. 

Im Frontend können Deposits und Withdrawls über ein einfaches Formular für eigene Konten (Feld mit Auto-Complete) angefragt werden - dies erfolgt nach gleichem Aufbau wie bei den Bookings. Bei erfolgreicher Operation (Deposit oder Booking) werden die Transaktions- und Deposit-History neu geladen und aktualisiert. 

Die weiteren Backend-Features werden unter Transaktionssicherheit ausführlicher erläutert.

## Transaktionssicherheit

In einer einfachen Betrachtungsweise bedeutet Transaktionssicherheit, dass keine ungewünschten Nebeneffekte bei der Ausführung einer Buchung auftreten. Ungewünschte Nebeneffekte haben zur Folge, dass der State der Applikation nicht mehr stabil ist. Dies äussert sich, wenn vor der Transaktion weniger oder mehr Geld im ganzen System vorhanden ist oder wenn Überweisungen getätigt wurden, ohne in der Bookings-Tabelle festgehalten zu sein. Diese Nebeneffekte entstehen, wenn sich mehrere Überweisungen teilweise überschreiben (Lost-Updates) oder nicht komplett abgeschlossen werden (z.B. Geld wird auf Konto A überwiesen, Ausführung scheitert beim Abzug des Gelds von Konto B, da dieses nicht genug enthätl). Weitere Ursachen sind Verbindungs- oder Netzwerkprobleme mit der Datenbank. 

All diese Problematiken sind einfach aufgefangen indem alle notwendigen Operationen in einer gemeinsamen Transaktion mit Isolationslevel "Serializable" ausgeführt werden. So werden alle oder keine Änderungen ausgeführt. Unsere BusinessLogik zur Ausführung von Bookings in der Methode "_Book" überprüft ob alle notwendigen Bedingungen für die Transaktion erfüllt sind. Die Methode wurde erfolgreich mit UnitTests getestet. In der übergeordneten Methode wird die Transaktion gestartet und bei Problemen erfolgt ein Rollback. Mittels von Lasttests kann über etliche Buchungen in kleinem Zeitraum einerseits die Fehler/Erfolgs-Quote getestet werden. Andererseits kann überprüft werden, dass vor und nach den erfolgten Buchungen gleich viel Geld im System ist. Dies ist ein guter Indikator, dass die Transaktionssicherheit eingehalten wurde. 

Bei den Lasttests hat es sich gezeigt, dass die erste Implementation der Buchungen nicht zufriedenstellend ist. Sie war zu bei vielen Requests pro Sekunde bedeutend zu langsam und die Fehlerquote war hoch. So war zwar die Sicherheit gewährleistet (es gab keine Nebeneffekte), jedoch war das System nicht resilient genug. Unter anderem entstanden viele Konflikte (Transaktionen konnten nicht ausgeführt werden, da bereits andere Buchungen im Gang waren) und die Retries haben eine grosse Langsamkeit in das System eingeführt. Aus diesem Grund wurde ein grösserer Teil des ÜKs damit verbracht geeignetere Möglichkeiten zu finden und umzusetzen. 

### Stored Procedures
Stored-Procedures sind SQL-Statements, welche direkt auf der Datenbank hinterlegt sind. Dadurch sind sie bedeutend performanter und das ganze System dadurch bei hoher Last resilienter. Die Stored-Procedures werden über `database update` mit dem Seeder auf der Datenbank gespeichert. Die konkrete Logik sowie Überprüfung inklusive Transaktion und Retry-Strategy bei Konflikten bleibt gleich. 

### QueueTransactionProcessing
Transaktionen mit Table-Locks oder Isolationslevel "Serializable" sind einfach umzusetzen habe jedoch das Problem, dass mehr Konflikte entstehen. Die Queue probiert einen ausgeklügerten Ansatz zu verfolgen und Konflikte / Retries zu verhindern. Dies geschieht über Analyse der betroffenen Entitäten (Konten). 

1. Die Queue nimmt Transaktionsanfragen entgegen.
2. Die Queue analysiert die Transaktionsanfragen und deren betroffenen Entitäten.
3. Die Queue stellt geeignete Batches an Transaktionen zusammen, welche gemeinsam und ohne sich gegenseitig in die Quere zu kommen ausgeführt werden können.
4. Die Queue nimmt die gruppierten Transaktionsanfragen (Batches) und führt sie seriell aus. 



