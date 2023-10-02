using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DotnetAuthAndFileHandling.Models
{
    public class dbImage
    {
        [Key]
        public int Id { get; set; }

        public long? CustomerId { get; set; }

        public byte[]? Customerimage { get; set; }
    }
}
