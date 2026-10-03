namespace Warehouse.Core.Domain
{
    /// <summary>Contact data shared by customers and carriers.</summary>
    public class PartnerDetails
    {
        public const int NameMaxLength = 100;
        public const int ContactNameMaxLength = 100;
        public const int PhoneMaxLength = 32;
        public const int AddressMaxLength = 250;

        public string Name { get; set; }

        public string ContactName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        /// <summary>Returns a trimmed, validated copy of the details.</summary>
        internal PartnerDetails Normalize()
        {
            return new PartnerDetails
            {
                Name = Guard.Required(Name, "Name", NameMaxLength),
                ContactName = Guard.Optional(ContactName, "Contact name", ContactNameMaxLength),
                Email = Guard.OptionalEmail(Email),
                Phone = Guard.Optional(Phone, "Phone", PhoneMaxLength),
                Address = Guard.Optional(Address, "Address", AddressMaxLength)
            };
        }
    }

    /// <summary>A company that places outbound orders.</summary>
    public class Customer
    {
        protected Customer()
        {
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public string ContactName { get; private set; }

        public string Email { get; private set; }

        public string Phone { get; private set; }

        public string Address { get; private set; }

        public static Customer Create(PartnerDetails details)
        {
            var customer = new Customer();
            customer.Update(details);
            return customer;
        }

        public void Update(PartnerDetails details)
        {
            var valid = (details ?? throw new System.ArgumentNullException(nameof(details))).Normalize();
            Name = valid.Name;
            ContactName = valid.ContactName;
            Email = valid.Email;
            Phone = valid.Phone;
            Address = valid.Address;
        }
    }

    /// <summary>A transport company that collects shipped orders.</summary>
    public class Carrier
    {
        protected Carrier()
        {
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public string ContactName { get; private set; }

        public string Email { get; private set; }

        public string Phone { get; private set; }

        public string Address { get; private set; }

        public static Carrier Create(PartnerDetails details)
        {
            var carrier = new Carrier();
            carrier.Update(details);
            return carrier;
        }

        public void Update(PartnerDetails details)
        {
            var valid = (details ?? throw new System.ArgumentNullException(nameof(details))).Normalize();
            Name = valid.Name;
            ContactName = valid.ContactName;
            Email = valid.Email;
            Phone = valid.Phone;
            Address = valid.Address;
        }
    }
}
