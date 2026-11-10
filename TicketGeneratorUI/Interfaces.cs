using System.Collections.Generic;

namespace TicketGeneratorUI
{
    public interface ITicketRepository
    {
        List<Ticket> GetAllTickets();
    }

    public interface IStudentRepository
    {
        List<string> GetGroups();
        List<Student> GetStudentsByGroup(string group);
    }

    public interface IResultRepository
    {
        int? GetPreviousTicketNumber(string group, Student student);
        void SaveResult(string group, Student student, int ticketNumber, bool isRepeat);
        void ClearHistory();
    }

    public interface ITicketService
    {
        (Ticket Ticket, bool IsRepeat) GenerateTicketForStudent(string group, Student student);
    }
}