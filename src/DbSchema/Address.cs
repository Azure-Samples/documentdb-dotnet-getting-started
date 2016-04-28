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
