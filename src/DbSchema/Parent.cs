using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentDB.DbSchema
{
    /// <summary>
    /// A parent class used within Family
    /// </summary>
    public class Parent
    {
        public string FamilyName { get; set; }

        public string FirstName { get; set; }
    }
}
