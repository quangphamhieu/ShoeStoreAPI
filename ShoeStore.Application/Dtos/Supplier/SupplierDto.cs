using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Dtos.Supplier
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string? ContactInfo { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
    }
}
