using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;

namespace Pulse.Models
{
    public class ProcessRow
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Memory { get; set; } = string.Empty;
        public double? CpuPercent { get; set; }
    }
}
