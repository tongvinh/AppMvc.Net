using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Contacts
{
  public class Contact
  {
    [Key]
    public int Id { get; set; }

    [Column(TypeName="nvarchar")]
    [StringLength(50)]
    [Required(ErrorMessage ="Phải nhập {0}")]
    [DisplayName("Họ tên")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Phải nhập {0}")]
    [EmailAddress(ErrorMessage = "Phải lại {0}")]
    [StringLength(100)]
    [DisplayName("Địa chỉ email")]
    public string Email { get; set; }

    [DisplayName("Ngày gửi")]
    public DateTime DateSent { get; set; }

    [DisplayName("Nội dung")]
    public string Message { get; set; }

    [StringLength(50)]
    [DisplayName("Số điện thoại")]
    [Phone(ErrorMessage ="Phải là {0}")]
    public string Phone { get; set; }
  }
}