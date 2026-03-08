USE CustomerManagementDB;

-- 1. Check all customers
PRINT '--- ALL CUSTOMERS ---';
SELECT * FROM Customer;

-- 2. Check customer phone
PRINT '--- CUSTOMER PHONE CHECK ---';
SELECT CustomerId, CustomerName, Phone
FROM Customer;

-- 3. Filter customers (AccountValue > 50000)
PRINT '--- FILTER CUSTOMERS ACCOUNTVALUE > 50000 ---';
SELECT CustomerId, CustomerName, AccountValue
FROM Customer
WHERE AccountValue > 50000;

-- 4. Sort customers by name
PRINT '--- SORT CUSTOMERS BY NAME ---';
SELECT CustomerId, CustomerName, AccountValue
FROM Customer
ORDER BY CustomerName;

-- 5. Join Customers and Orders
PRINT '--- JOIN CUSTOMERS & ORDERS ---';
SELECT c.CustomerId, c.CustomerName, o.OrderId, o.TotalAmount
FROM Customer c
LEFT JOIN Orders o ON c.CustomerId = o.CustomerId;

-- 6. Grouping and aggregation
PRINT '--- TOTAL ORDER AMOUNT PER CUSTOMER ---';
SELECT c.CustomerName, SUM(o.TotalAmount) AS TotalSpent
FROM Customer c
LEFT JOIN Orders o ON c.CustomerId = o.CustomerId
GROUP BY c.CustomerName;

-- 7. Projection example
PRINT '--- CUSTOMER PROJECTION (NAME + EMAIL ONLY) ---';
SELECT CustomerName, Email
FROM Customer;