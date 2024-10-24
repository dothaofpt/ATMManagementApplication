
using System.ComponentModel.DataAnnotations;

namespace ATMManagementApplication.Models {
  public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public decimal Balance { get; set; }
    public string Email { get; set; } // Thêm trường Email
}

}
