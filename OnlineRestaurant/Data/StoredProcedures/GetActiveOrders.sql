CREATE PROCEDURE [dbo].[GetActiveOrders]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT o.IdOrder, o.IdUser, o.OrderDate, o.Status, o.TotalAmount
    FROM Orders o
    WHERE o.Status != 'delivered' AND o.Status != 'canceled'
    ORDER BY o.OrderDate DESC;
END 