CREATE PROCEDURE [dbo].[GetOrderById]
    @OrderId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.IdOrder, o.IdUser, o.OrderDate, o.Status, o.TotalAmount,
        u.IdUser, u.FirstName, u.LastName, u.Email, u.Role,
        od.IdOrderDish, od.IdOrder AS OrderDishOrderId, od.IdDish AS OrderDishId, od.Quantity AS OrderDishQuantity,
        d.IdDish, d.Name AS DishName, d.IdCategory AS DishCategory, d.Price AS DishPrice, 
        d.PortionSize AS DishPortionSize, d.TotalQuantity AS DishQuantity,
        om.IdOrderMenu, om.IdOrder AS OrderMenuOrderId, om.IdMenu, om.Quantity AS OrderMenuQuantity,
        m.IdMenu AS MenuId, m.Name AS MenuName, m.IdCategory AS MenuCategory,
        md.IdMenuDish, md.IdMenu AS MenuDishMenuId, md.IdDish AS MenuDishId, md.Quantity AS MenuDishQuantity,
        d2.IdDish AS MenuDishDishId, d2.Name AS MenuDishDishName, d2.IdCategory AS MenuDishDishCategory,
        d2.Price AS MenuDishDishPrice, d2.PortionSize AS MenuDishDishPortionSize, 
        d2.TotalQuantity AS MenuDishDishQuantity
    FROM Orders o
    LEFT JOIN Users u ON o.IdUser = u.IdUser
    LEFT JOIN OrderDishes od ON o.IdOrder = od.IdOrder
    LEFT JOIN Dishes d ON od.IdDish = d.IdDish
    LEFT JOIN OrderMenus om ON o.IdOrder = om.IdOrder
    LEFT JOIN Menus m ON om.IdMenu = m.IdMenu
    LEFT JOIN MenuDishes md ON m.IdMenu = md.IdMenu
    LEFT JOIN Dishes d2 ON md.IdDish = d2.IdDish
    WHERE o.IdOrder = @OrderId;
END 