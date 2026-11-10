using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TicketGeneratorUI
{
    public class WordTicketRepository : ITicketRepository
    {
        private readonly string _filePath;

        public WordTicketRepository(string filePath)
        {
            _filePath = File.Exists(filePath) ? filePath : "tickets.docx";
        }

        public List<Ticket> GetAllTickets()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Файл не найден: {_filePath}");

            using var document = WordprocessingDocument.Open(_filePath, false);
            
            // Собираем весь текст документа
            var fullText = string.Join("\n", document.MainDocumentPart?.Document.Body?
                .Descendants<Paragraph>()
                .Select(p => p.InnerText) ?? Enumerable.Empty<string>());

            // Разбиваем по слову "Билет" и цифре
            var blocks = Regex.Split(fullText, @"(?=Билет\s*№?\s*\d+)", RegexOptions.IgnoreCase);

            var tickets = new List<Ticket>();

            foreach (var block in blocks)
            {
                if (string.IsNullOrWhiteSpace(block)) continue;

                var match = Regex.Match(block, @"^Билет\s*№?\s*(\d+)", RegexOptions.IgnoreCase);
                if (!match.Success) continue;

                int number = int.Parse(match.Groups[1].Value);
                string questionsText = block.Substring(match.Length);

                // Берем только строки, которые начинаются с цифры и точки/скобки
                var questions = questionsText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(q => q.Trim())
                    .Where(q => Regex.IsMatch(q, @"^\d+[.)]")) 
                    .ToList();

                if (questions.Count > 0)
                {
                    tickets.Add(new Ticket(number, questions));
                }
            }

            return tickets.OrderBy(t => t.Number).ToList();
        }
    }

    public class ExcelStudentRepository : IStudentRepository
    {
        private readonly string _filePath;

        public ExcelStudentRepository(string filePath)
        {
            _filePath = File.Exists(filePath) ? filePath : "students.xlsx";
        }

        public List<string> GetGroups()
        {
            if (!File.Exists(_filePath)) return new List<string>();
            using var workbook = new XLWorkbook(_filePath);
            return workbook.Worksheets.Select(s => s.Name).ToList();
        }

        public List<Student> GetStudentsByGroup(string group)
        {
            var students = new List<Student>();
            if (!File.Exists(_filePath)) return students;

            using var workbook = new XLWorkbook(_filePath);
            var sheet = workbook.Worksheet(group);
            var lastRow = Math.Max(1, sheet.LastRowUsed()?.RowNumber() ?? 1);

            for (int row = 2; row <= lastRow; row++)
            {
                var lastName = sheet.Cell(row, 1).GetString();
                var firstName = sheet.Cell(row, 2).GetString();
                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    students.Add(new Student(lastName, firstName));
                }
            }
            return students;
        }
    }

    public class ExcelResultRepository : IResultRepository
    {
        private readonly string _filePath;

        public ExcelResultRepository(string filePath)
        {
            _filePath = filePath;
        }

        public int? GetPreviousTicketNumber(string group, Student student)
        {
            if (!File.Exists(_filePath)) return null;

            using var workbook = new XLWorkbook(_filePath);
            var sheet = workbook.Worksheets.FirstOrDefault();
            if (sheet == null) return null;

            var lastRow = Math.Max(1, sheet.LastRowUsed()?.RowNumber() ?? 1);
            for (var row = 2; row <= lastRow; row++)
            {
                if (sheet.Cell(row, 1).GetString() == group &&
                    sheet.Cell(row, 2).GetString() == student.LastName &&
                    sheet.Cell(row, 3).GetString() == student.FirstName)
                {
                    if (int.TryParse(sheet.Cell(row, 4).GetString(), out int ticketNum))
                        return ticketNum;
                }
            }
            return null;
        }

        public void SaveResult(string group, Student student, int ticketNumber, bool isRepeat)
        {
            using var workbook = File.Exists(_filePath) ? new XLWorkbook(_filePath) : new XLWorkbook();
            var sheet = workbook.Worksheets.FirstOrDefault() ?? workbook.AddWorksheet("Results");

            var headers = new[] { "Group", "LastName", "FirstName", "TicketNum", "DateTime", "IsRepeat" };
            if (sheet.Cell(1, 1).IsEmpty())
            {
                for (var column = 1; column <= headers.Length; column++)
                    sheet.Cell(1, column).Value = headers[column - 1];
            }

            var newRow = Math.Max(1, sheet.LastRowUsed()?.RowNumber() ?? 1) + 1;
            sheet.Cell(newRow, 1).Value = group;
            sheet.Cell(newRow, 2).Value = student.LastName;
            sheet.Cell(newRow, 3).Value = student.FirstName;
            sheet.Cell(newRow, 4).Value = ticketNumber;
            sheet.Cell(newRow, 5).Value = DateTime.Now.ToString("g");
            sheet.Cell(newRow, 6).Value = isRepeat ? "Yes" : "No";

            workbook.SaveAs(_filePath);
        }

        public void ClearHistory()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }
    }
}