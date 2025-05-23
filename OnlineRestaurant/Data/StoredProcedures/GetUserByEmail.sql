CREATE PROCEDURE [dbo].[GetUserByEmail]
    @Email nvarchar(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT *
    FROM Users
    WHERE Email = @Email;
END 