using L_Bank_W_Backend.Core.Models;
using L_Bank.Core.Helper;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess;

public static class SeedData
{
    public static void Seed(DbContext context)
    {
        var userCount = context.Set<User>().Count();
        var ledgerCount = context.Set<Ledger>().Count();

        if (userCount < 1)
        {
            context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Users ON");
            foreach (User user in UserSeed)
            {
                context.Add(user);
            }
            context.SaveChanges();
            context.Database.ExecuteSql($"SET IDENTITY_INSERT dbo.Users OFF");

            context.Database.ExecuteSql(
                $@"CREATE PROCEDURE TransferAmount
                                @sourceId INT,
                                @targetId INT,
                                @amount DECIMAL(18, 2)
                            AS
                            BEGIN
                                SET NOCOUNT ON;
                                BEGIN TRY
                                    BEGIN TRANSACTION;

                                        IF @amount <= 0
                                    BEGIN
                                        RAISERROR('Cannot transfer amounts which aren''t greater than 0', 16, 1);
                                        ROLLBACK TRANSACTION;
                                        RETURN;
                                    END

                                    UPDATE Ledgers WITH (ROWLOCK, UPDLOCK)
                                    SET Balance = CASE 
                                                    WHEN Id = @sourceId THEN Balance - @amount
                                                    WHEN Id = @targetId THEN Balance + @amount
                                                END
                                    WHERE Id IN (@sourceId, @targetId);

                                    IF @@ROWCOUNT <> 2
                                    BEGIN
                                        RAISERROR('Source or Target Ledger does not exist', 16, 1);
                                        ROLLBACK TRANSACTION;
                                        RETURN;
                                    END

                                    DECLARE @sourceBalance DECIMAL(18, 2);
                                    SELECT @sourceBalance = Balance FROM Ledgers WHERE Id = @sourceId;

                                    IF @sourceBalance < 0
                                    BEGIN
                                        RAISERROR('Not enough money to transfer target amount', 16, 1);
                                        ROLLBACK TRANSACTION;
                                        RETURN;
                                    END


                                    -- Insert the booking record
                                    INSERT INTO Bookings (Amount, SourceId, DestinationId, Date)
                                    VALUES (@amount, @sourceId, @targetId, GETDATE());

                                    COMMIT TRANSACTION;
                                    
                                END TRY
                                BEGIN CATCH
                                    IF XACT_STATE() <> 0
                                        ROLLBACK TRANSACTION;

                                    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
                                    RAISERROR(@ErrorMessage, 16, 1);
                                END CATCH
                            END"
            );
        }

        if (ledgerCount < 1)
        {
            foreach (Ledger ledger in LedgerSeed)
            {
                context.Add(ledger);
            }
            context.SaveChanges();
        }
    }

    public static List<User> UserSeed =
    [
        new()
        {
            Id = 1,
            Username = "Admin",
            PasswordHash = PasswordHelper.HashAndSaltPassword("adminpass"),
            Role = Roles.Admin,
        },
        new()
        {
            Id = 2,
            Username = "User",
            PasswordHash = PasswordHelper.HashAndSaltPassword("userpass"),
            Role = Roles.User,
        },
    ];
    public static List<Ledger> LedgerSeed =
    [
        new()
        {
            Name = "Manitu AG",
            Balance = 1000,
            UserId = 1,
        },
        new()
        {
            Name = "Chrysalkis GmbH",
            Balance = 2000,
            UserId = 2,
        },
        new()
        {
            Name = "Smith & Co KG",
            Balance = 3000,
            UserId = 2,
        },
    ];
}
