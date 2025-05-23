CREATE PROCEDURE [dbo].[VerifyUniqueEmail]
    @Email nvarchar(255),
    @UserId int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @UserId IS NULL
        BEGIN
            -- Case for new user registration
            SELECT CASE 
                WHEN EXISTS (SELECT 1 FROM Users WHERE Email = @Email) 
                THEN 0   -- False (email exists)
                ELSE 1   -- True (email is unique)
            END AS IsUnique;
        END
    ELSE
        BEGIN
            -- Case for existing user update
            SELECT CASE 
                WHEN EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND IdUser != @UserId) 
                THEN 0   -- False (email exists for different user)
                ELSE 1   -- True (email is unique or belongs to current user)
            END AS IsUnique;
        END
END 