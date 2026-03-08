using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement
{
    class Program
    {
        static void Main()
        {
            using var db = new AppDbContext();

            while (true)
            {
                Console.WriteLine("\n=================================");
                Console.WriteLine("   CUSTOMER MANAGEMENT SYSTEM");
                Console.WriteLine("=================================");
                Console.WriteLine("1. Customer Management");
                Console.WriteLine("2. Address Management");
                Console.WriteLine("3. Contact Person Management");
                Console.WriteLine("4. Customer Interactions");
                Console.WriteLine("5. Customer Analytics");
                Console.WriteLine("6. Deleted Records");
                Console.WriteLine("7. Exit");

                Console.Write("\nEnter choice: ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                    continue;

                switch (choice)
                {
                    case 1: CustomerMenu(db); break;
                    case 2: AddressMenu(db); break;
                    case 3: ContactMenu(db); break;
                    case 4: InteractionMenu(db); break;
                    case 5: AnalyticsMenu(db); break;
                    case 6: DeletedCustomers(db); break;
                    case 7: return;
                }
            }
        }

        // ===============================
        // CUSTOMER MANAGEMENT
        // ===============================

        static void CustomerMenu(AppDbContext db)
        {
            while (true)
            {
                Console.WriteLine("\nCustomer Management");
                Console.WriteLine("1 View All Customers");
                Console.WriteLine("2 Add Customer");
                Console.WriteLine("3 Update Customer");
                Console.WriteLine("4 Delete Customer");
                Console.WriteLine("5 Search Customer");
                Console.WriteLine("6 Return");

                Console.Write("Enter choice: ");
                if (!int.TryParse(Console.ReadLine(), out int choice))
                    continue;

                // VIEW CUSTOMERS
                if (choice == 1)
                {
                    var customers = db.Customers
                        .Where(c => !c.IsDeleted)
                        .ToList();

                    foreach (var c in customers)
                    {
                        Console.WriteLine("\n-----------------------------");
                        Console.WriteLine($"Name          : {c.CustomerName}");
                        Console.WriteLine($"Email         : {c.Email}");
                        Console.WriteLine($"Phone         : {c.Phone}");
                        Console.WriteLine($"Website       : {c.Website}");
                        Console.WriteLine($"Industry      : {c.Industry}");
                        Console.WriteLine($"Company Size  : {c.CompanySize}");
                        Console.WriteLine($"Type          : {c.Type}");
                        Console.WriteLine($"Classification: {c.Classification}");
                        Console.WriteLine($"SegmentId     : {c.SegmentId}");
                        Console.WriteLine($"AccountValue  : {c.AccountValue}");
                        Console.WriteLine($"HealthScore   : {c.HealthScore}");
                    }
                }

                // ADD CUSTOMER
                else if (choice == 2)
                {
                    Console.Write("Customer Name: ");
                    string name = Console.ReadLine() ?? "";

                    Console.Write("Email: ");
                    string email = Console.ReadLine() ?? "";

                    // CHECK DUPLICATE EMAIL
                    if (db.Customers.Any(c => c.Email == email))
                    {
                        Console.WriteLine("❌ Email already exists. Use different email.");
                        continue;
                    }

                    Console.Write("Phone: ");
                    string phone = Console.ReadLine() ?? "";

                    Console.Write("Website: ");
                    string website = Console.ReadLine() ?? "";

                    Console.Write("Industry: ");
                    string industry = Console.ReadLine() ?? "";

                    Console.Write("Company Size: ");
                    string companySize = Console.ReadLine() ?? "";

                    // VALIDATE TYPE
                    string type;
                    while (true)
                    {
                        Console.Write("Type (Business/Individual): ");
                        type = Console.ReadLine() ?? "";

                        if (type.Equals("Business", StringComparison.OrdinalIgnoreCase) ||
                            type.Equals("Individual", StringComparison.OrdinalIgnoreCase))
                        {
                            type = char.ToUpper(type[0]) + type.Substring(1).ToLower();
                            break;
                        }

                        Console.WriteLine("Invalid Type! Enter Business or Individual.");
                    }

                    // VALIDATE CLASSIFICATION
                    string[] validClassifications = { "Prospect", "Active", "Inactive", "VIP", "At-Risk" };
                    string classification;

                    while (true)
                    {
                        Console.Write("Classification (Prospect/Active/Inactive/VIP/At-Risk): ");
                        classification = Console.ReadLine() ?? "";

                        if (validClassifications.Contains(classification, StringComparer.OrdinalIgnoreCase))
                        {
                            classification = validClassifications
                                .First(x => x.Equals(classification, StringComparison.OrdinalIgnoreCase));
                            break;
                        }

                        Console.WriteLine("Invalid Classification! Try again.");
                    }

                    // SAFE NUMERIC INPUTS
                    int segmentId;
                    Console.Write("SegmentId: ");
                    while (!int.TryParse(Console.ReadLine(), out segmentId))
                        Console.Write("Enter valid numeric SegmentId: ");

                    decimal accountValue;
                    Console.Write("Account Value: ");
                    while (!decimal.TryParse(Console.ReadLine(), out accountValue))
                        Console.Write("Enter valid numeric Account Value: ");

                    int healthScore;
                    Console.Write("Health Score: ");
                    while (!int.TryParse(Console.ReadLine(), out healthScore))
                        Console.Write("Enter valid numeric Health Score: ");

                    var customer = new Customer
                    {
                        CustomerName = name,
                        Email = email,
                        Phone = phone,
                        Website = website,
                        Industry = industry,
                        CompanySize = companySize,
                        Type = type,
                        Classification = classification,
                        SegmentId = segmentId,
                        AccountValue = accountValue,
                        HealthScore = healthScore,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };

                    db.Customers.Add(customer);
                    db.SaveChanges();

                    Console.WriteLine("✅ Customer Added Successfully");
                }

                // UPDATE CUSTOMER
                else if (choice == 3)
                {
                    Console.Write("Customer ID: ");
                    if (!int.TryParse(Console.ReadLine(), out int id))
                        continue;

                    Console.Write("New Name: ");
                    string name = Console.ReadLine() ?? "";

                    Console.Write("New Email: ");
                    string email = Console.ReadLine() ?? "";

                    db.Database.ExecuteSqlRaw(
                        "UPDATE Customer SET CustomerName = {0}, Email = {1} WHERE CustomerId = {2}",
                        name, email, id);

                    Console.WriteLine("✅ Customer Updated Successfully");
                }

                // DELETE CUSTOMER (TRIGGER SAFE)
                else if (choice == 4)
                {
                    Console.Write("Customer ID: ");
                    if (!int.TryParse(Console.ReadLine(), out int id))
                        continue;

                    db.Database.ExecuteSqlRaw(
                        "DELETE FROM Customer WHERE CustomerId = {0}", id);

                    Console.WriteLine("✅ Customer Soft Deleted Successfully");
                }

                // SEARCH
                else if (choice == 5)
                {
                    Console.Write("Enter Customer Name: ");
                    string name = Console.ReadLine() ?? "";

                    var result = db.Customers
                        .Where(c => c.CustomerName.Contains(name) && !c.IsDeleted)
                        .ToList();

                    foreach (var c in result)
                        Console.WriteLine($"{c.CustomerName} | {c.Email} | {c.Phone}");
                }

                else if (choice == 6)
                    return;
            }
        }

        static void AddressMenu(AppDbContext db)
        {
            foreach (var a in db.CustomerAddresses.ToList())
                Console.WriteLine($"{a.CustomerId} | {a.Street} | {a.City}");
        }

        static void ContactMenu(AppDbContext db)
        {
            foreach (var c in db.ContactPersons.ToList())
                Console.WriteLine($"{c.Name} | {c.Email} | {c.Phone}");
        }

        static void InteractionMenu(AppDbContext db)
        {
            foreach (var i in db.CustomerInteractions.ToList())
                Console.WriteLine($"{i.CustomerId} | {i.InteractionType} | {i.Subject}");
        }

        static void AnalyticsMenu(AppDbContext db)
        {
            Console.WriteLine($"Total Customers: {db.Customers.Count()}");
            Console.WriteLine($"Active Customers: {db.Customers.Count(c => !c.IsDeleted)}");
        }

        static void DeletedCustomers(AppDbContext db)
        {
            foreach (var c in db.Customers.Where(c => c.IsDeleted))
                Console.WriteLine($"{c.CustomerName} (Deleted)");
        }
    }
}