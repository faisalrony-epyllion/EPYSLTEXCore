using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Exceptions
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException() : base($"Item not found!")
        {
        }

        public ItemNotFoundException(int id) : base($"Item not found with id {id}")
        {
        }

        public ItemNotFoundException(string message) : base(message)
        {
        }

        public ItemNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
