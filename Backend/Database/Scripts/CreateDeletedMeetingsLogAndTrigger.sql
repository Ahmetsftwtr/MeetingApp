USE MeetingAppDb;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeletedMeetingsLog')
BEGIN
    CREATE TABLE DeletedMeetingsLog (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        MeetingId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        Description NVARCHAR(MAX) NULL,
        IsCancelled BIT NOT NULL,
        CancelledAt DATETIME2 NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        DeletedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        DeletedBy NVARCHAR(100) NULL DEFAULT SYSTEM_USER
    );
    PRINT 'DeletedMeetingsLog tablosu oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'DeletedMeetingsLog tablosu zaten mevcut.';
END
GO

IF OBJECT_ID('trg_Meetings_AfterDelete', 'TR') IS NOT NULL
BEGIN
    DROP TRIGGER trg_Meetings_AfterDelete;
    PRINT 'Eski trigger silindi.';
END
GO

CREATE TRIGGER trg_Meetings_AfterDelete
ON Meetings
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO DeletedMeetingsLog (
        MeetingId,
        Title,
        StartDate,
        EndDate,
        CreatedAt,
        UpdatedAt,
        Description,
        IsCancelled,
        CancelledAt,
        UserId,
        DeletedAt,
        DeletedBy
    )
    SELECT 
        d.Id,
        d.Title,
        d.StartDate,
        d.EndDate,
        d.CreatedAt,
        d.UpdatedAt,
        d.Description,
        d.IsCancelled,
        d.CancelledAt,
        d.UserId,
        GETDATE(),
        SYSTEM_USER
    FROM deleted d;
END;
GO

PRINT 'trg_Meetings_AfterDelete trigger başarıyla oluşturuldu.';
GO

SELECT 
    name AS TriggerName,
    OBJECT_NAME(parent_id) AS TableName,
    is_disabled AS IsDisabled,
    create_date AS CreatedDate,
    modify_date AS ModifiedDate
FROM sys.triggers
WHERE name = 'trg_Meetings_AfterDelete';
GO

PRINT 'Trigger doğrulaması tamamlandı.';
GO
