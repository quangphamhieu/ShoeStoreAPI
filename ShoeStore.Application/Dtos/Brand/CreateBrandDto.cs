using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Dtos.Brand
{
    public class CreateBrandDto
    {
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? StatusId { get; set; } // nếu null => backend có thể set default active
    }
}
