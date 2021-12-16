using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lingya.Pagination.Tests.Mock
{
    public class Account {
        [Key]
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength (50)]
        public string Name { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public double UnitConst { get; set; }

        [Required]
        public double Price { get; set; }

        public float? Rank { get; set; }
    }
}