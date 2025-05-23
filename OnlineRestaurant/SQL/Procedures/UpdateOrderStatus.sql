CREATE OR ALTER PROCEDURE UpdateOrderStatus
    @OrderId uniqueidentifier,
    @NewStatus int
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if order exists
        IF NOT EXISTS (SELECT 1 FROM Orders WHERE IdOrder = @OrderId)
        BEGIN
            RAISERROR ('Order not found.', 16, 1);
            RETURN;
        END

        -- Validate the new status value
        IF @NewStatus NOT IN (0, 1, 2, 3, 4)
        BEGIN
            RAISERROR ('Invalid status value. Must be between 0 and 4.', 16, 1);
            RETURN;
        END

        -- Convert numeric status to string representation
        DECLARE @StatusString nvarchar(20)
        SET @StatusString = 
            CASE @NewStatus
                WHEN 0 THEN 'registered'
                WHEN 1 THEN 'preparing'
                WHEN 2 THEN 'out for delivery'
                WHEN 3 THEN 'delivered'
                WHEN 4 THEN 'canceled'
            END

        -- Update the order status
        UPDATE Orders
        SET Status = @StatusString
        WHERE IdOrder = @OrderId;

        -- Return the updated order with user info but avoid column name collisions
        SELECT 
            o.IdOrder,
            o.OrderDate,
            o.Status,
            o.TotalAmount,
            o.IdUser,
            u.Email as UserEmail,
            u.FirstName as UserFirstName,
            u.LastName as UserLastName,
            u.Phone as UserPhone,
            u.DeliveryAddress as UserDeliveryAddress,
            u.Role as UserRole
        FROM Orders o
        LEFT JOIN Users u ON o.IdUser = u.IdUser
        WHERE o.IdOrder = @OrderId;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrorMessage, 16, 1);
    END CATCH
END 