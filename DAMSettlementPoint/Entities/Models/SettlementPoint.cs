
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DAMSettlementPoint.Entities.Models
{
    public class SettlementPoint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime DeliveryDate { get; set; }

        [Required]
        public TimeSpan HourEnding { get; set; }

        [Required]
        [StringLength(50)]
        public string SettlementPointName { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SettlementPointPrice { get; set; }

        [Required]
        public bool DSTFlag { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
