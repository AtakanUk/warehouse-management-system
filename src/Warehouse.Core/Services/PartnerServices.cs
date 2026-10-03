using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Warehouse.Core.Data;
using Warehouse.Core.Domain;

namespace Warehouse.Core.Services
{
    /// <summary>Maintains customers.</summary>
    public sealed class CustomerService
    {
        private readonly WarehouseDbContext _db;

        public CustomerService(WarehouseDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IQueryable<PartnerListItem> Query(string search = null)
        {
            IQueryable<Customer> customers = _db.Customers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                customers = customers.Where(c => c.Name.Contains(term) || c.ContactName.Contains(term) || c.Email.Contains(term));
            }

            return customers
                .OrderBy(c => c.Name)
                .Select(c => new PartnerListItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactName = c.ContactName,
                    Email = c.Email,
                    Phone = c.Phone,
                    OrderCount = _db.Orders.Count(o => o.CustomerId == c.Id)
                });
        }

        public IReadOnlyList<PartnerOption> GetOptions() =>
            _db.Customers.AsNoTracking().OrderBy(c => c.Name).Select(c => new PartnerOption { Id = c.Id, Name = c.Name }).ToList();

        public Customer Find(int id) => _db.Customers.AsNoTracking().SingleOrDefault(c => c.Id == id);

        public Customer Create(PartnerDetails details)
        {
            var customer = Customer.Create(details);
            _db.Customers.Add(customer);
            _db.Commit();
            return customer;
        }

        public void Update(int id, PartnerDetails details)
        {
            var customer = _db.Customers.Find(id) ?? throw new BusinessRuleException("The customer no longer exists.");
            customer.Update(details);
            _db.Commit();
        }

        public void Delete(int id)
        {
            var customer = _db.Customers.Find(id);
            if (customer == null)
            {
                return;
            }

            if (_db.Orders.Any(o => o.CustomerId == id))
            {
                throw new BusinessRuleException($"{customer.Name} has orders and cannot be deleted.");
            }

            _db.Customers.Remove(customer);
            _db.Commit();
        }
    }

    /// <summary>Maintains carriers.</summary>
    public sealed class CarrierService
    {
        private readonly WarehouseDbContext _db;

        public CarrierService(WarehouseDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IQueryable<PartnerListItem> Query(string search = null)
        {
            IQueryable<Carrier> carriers = _db.Carriers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                carriers = carriers.Where(c => c.Name.Contains(term) || c.ContactName.Contains(term) || c.Email.Contains(term));
            }

            return carriers
                .OrderBy(c => c.Name)
                .Select(c => new PartnerListItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactName = c.ContactName,
                    Email = c.Email,
                    Phone = c.Phone,
                    OrderCount = _db.Orders.Count(o => o.CarrierId == c.Id)
                });
        }

        public IReadOnlyList<PartnerOption> GetOptions() =>
            _db.Carriers.AsNoTracking().OrderBy(c => c.Name).Select(c => new PartnerOption { Id = c.Id, Name = c.Name }).ToList();

        public Carrier Find(int id) => _db.Carriers.AsNoTracking().SingleOrDefault(c => c.Id == id);

        public Carrier Create(PartnerDetails details)
        {
            var carrier = Carrier.Create(details);
            EnsureNameIsUnique(carrier.Name, excludeId: null);
            _db.Carriers.Add(carrier);
            _db.Commit();
            return carrier;
        }

        public void Update(int id, PartnerDetails details)
        {
            var carrier = _db.Carriers.Find(id) ?? throw new BusinessRuleException("The carrier no longer exists.");
            carrier.Update(details);
            EnsureNameIsUnique(carrier.Name, excludeId: id);
            _db.Commit();
        }

        public void Delete(int id)
        {
            var carrier = _db.Carriers.Find(id);
            if (carrier == null)
            {
                return;
            }

            if (_db.Orders.Any(o => o.CarrierId == id))
            {
                throw new BusinessRuleException($"{carrier.Name} has handled shipments and cannot be deleted.");
            }

            _db.Carriers.Remove(carrier);
            _db.Commit();
        }

        private void EnsureNameIsUnique(string name, int? excludeId)
        {
            if (_db.Carriers.Any(c => c.Name == name && c.Id != excludeId))
            {
                throw new BusinessRuleException($"A carrier named {name} already exists.");
            }
        }
    }
}
