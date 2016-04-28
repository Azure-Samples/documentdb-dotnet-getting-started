using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentDB.DbSchema
{
    /// <summary>
    /// An address class containing data attached to a Family.
    /// </summary>
    public class Address
    {
        public string State { get; set; }

        public string County { get; set; }

        public string City { get; set; }
    }
}
