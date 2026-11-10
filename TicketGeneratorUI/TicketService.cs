using System;
using System.Linq;

namespace TicketGeneratorUI
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IResultRepository _resultRepository;

        public TicketService(ITicketRepository ticketRepository, IResultRepository resultRepository)
        {
            _ticketRepository = ticketRepository;
            _resultRepository = resultRepository;
        }

        public (Ticket Ticket, bool IsRepeat) GenerateTicketForStudent(string group, Student student)
        {
            var allTickets = _ticketRepository.GetAllTickets();
            if (allTickets.Count == 0)
                throw new InvalidOperationException("Нет доступных билетов для генерации. Проверьте файл tickets.docx.");

            var previousTicketNum = _resultRepository.GetPreviousTicketNumber(group, student);

            Ticket selectedTicket;
            bool isRepeat = false;

            if (previousTicketNum.HasValue)
            {
                selectedTicket = allTickets.FirstOrDefault(t => t.Number == previousTicketNum.Value) ?? allTickets[0];
                isRepeat = true;
            }
            else
            {
                int randomIndex = Random.Shared.Next(allTickets.Count);
                selectedTicket = allTickets[randomIndex];
                _resultRepository.SaveResult(group, student, selectedTicket.Number, isRepeat: false);
            }

            return (selectedTicket, isRepeat);
        }
    }
}