namespace SOS.Lib.Models.Verbatim.Artportalen
{
    public class Organization
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(OrganizationId)}: {OrganizationId}, {nameof(Name)}: {Name}";
        }
    }
}
