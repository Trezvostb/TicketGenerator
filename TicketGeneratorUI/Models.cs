using System.Collections.Generic;

namespace TicketGeneratorUI
{
    public sealed record Student(string LastName, string FirstName)
    {
        public string DisplayName => $"{LastName} {FirstName}";
        public override string ToString() => DisplayName;
    }

    public sealed record Ticket(int Number, List<string> Questions);
}