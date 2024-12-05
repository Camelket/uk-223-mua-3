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

Ein grösserer Teil des ÜKs wurde damit verbracht, 

## Transaktionssicherheit



