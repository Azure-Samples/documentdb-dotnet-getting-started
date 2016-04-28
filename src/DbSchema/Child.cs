namespace DocumentDB.DbSchema
{
    /// <summary>
    /// A child class used within Family
    /// </summary>
    public class Child
    {
        public string FamilyName { get; set; }

        public string FirstName { get; set; }

        public string Gender { get; set; }

        public int Grade { get; set; }

        public Pet[] Pets { get; set; }
    }
}
