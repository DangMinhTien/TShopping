using System.Runtime.CompilerServices;

namespace TShopping.ViewModels
{
    public class VnPaymentRequestModel
    {
        public int OrderId { get; set; }
        public string FullName { get; set; }
        public string Decription { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
