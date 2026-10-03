using System.Web.UI;
using Warehouse.Core.Domain;

namespace Warehouse.Web.Controls
{
    /// <summary>Contact form shared by the customer and carrier screens.</summary>
    public partial class PartnerForm : UserControl
    {
        public PartnerDetails Details
        {
            get => new PartnerDetails
            {
                Name = NameBox.Text,
                ContactName = ContactBox.Text,
                Email = EmailBox.Text,
                Phone = PhoneBox.Text,
                Address = AddressBox.Text
            };
            set
            {
                NameBox.Text = value.Name;
                ContactBox.Text = value.ContactName;
                EmailBox.Text = value.Email;
                PhoneBox.Text = value.Phone;
                AddressBox.Text = value.Address;
            }
        }
    }
}
