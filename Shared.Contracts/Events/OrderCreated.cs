using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Events
{
    public record OrderCreated(int OrderId, string CustomerName, DateTime CreatedAt);
}
