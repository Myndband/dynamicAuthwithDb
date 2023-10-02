using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DotnetAuthAndFileHandling.Models
{
    public class Customermodal
    {
        [Key]
        public long Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; } = null!;

        [StringLength(30)]
        public string? Email { get; set; }

        [StringLength(10)]
        public string? Phone { get; set; }

        public bool? IsActive { get; set; }
    }
}
