using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Pay1193.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace Pay1193.Models
{
    public class PayIndexViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        [Display(Name = "Name")]
        public string FullName { get; set; }
        public Employee Employee { get; set; }
        [Display(Name = "Pay Date")]
        public DateTime DatePay { get; set; }
        [Display(Name = "Month")] 
        public string MonthPay { get; set; }
        public int TaxYearId { get; set; }
        public string TaxYear { get; set; }
        [Display(Name = "Total Earnings")]
        public decimal TotalEarnings { get; set; }
        [Display(Name = "Total Deductions")]
        public decimal TotalDeduction { get; set; }
        [Display(Name = "Net")]
        public decimal NetPayment { get; set; }
    }
}
