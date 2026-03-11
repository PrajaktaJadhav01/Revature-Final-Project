using MongoDB.Driver;
using System.Collections.Generic;

namespace CustomerManagement
{
    public class MongoCustomerService
    {
        private readonly IMongoCollection<Customer> _customers;

        public MongoCustomerService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("CustomerManagementDB");

            _customers = database.GetCollection<Customer>("Customers");
        }

        // INSERT into MongoDB
        public void AddCustomer(Customer customer)
        {
            _customers.InsertOne(customer);
        }

        // READ from MongoDB (NEW)
        public List<Customer> GetCustomers()
        {
            return _customers.Find(_ => true).ToList();
        }
    }
}