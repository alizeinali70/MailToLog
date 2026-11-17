namespace MailToLog.Domain.Entities
{
    public class Contact
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public string NormalizedName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }
    }
}
