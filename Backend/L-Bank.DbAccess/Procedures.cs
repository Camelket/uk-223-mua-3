using System;

namespace L_Bank.Api;

public class Procedures
{
    public static readonly string BookingWithoutLockProcedureName = "TransferAmountWithoutLock";

    public static readonly string BookingWithLockProcedureName = "TransferAmountWithLock";
    public static readonly FormattableString BookingWithoutLock =
        $@"CREATE PROCEDURE TransferAmountWithoutLock
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

                                    UPDATE Ledgers 
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
                            END";
    public static readonly FormattableString BookingWithLock =
        $@"CREATE PROCEDURE TransferAmountWithLock
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
                            END";
}
