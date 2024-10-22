using System.ComponentModel.DataAnnotations;

namespace ATMManagementApplication.Models{
public class Customer{
[Key]  // Annotation:Primary key
public int CustomerId {get; set;}
[Required]
public string Name { get; set; }
public string Password { get; set; }
public decimal Balance {get; set;}


}
}