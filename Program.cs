using System;
using System.IO;
using ClosedXML.Excel;

namespace TicketGenerator
{
    class Program
    {
        const string FilePath = "journal.xlsx";

        static void Main()
        {
            Console.WriteLine("Для выхода нажмите ESC.\n");
            InitExcel();

            var random = new Random();

            while (true)
            {
                string lastName = ReadLastName();
                if (lastName == null)
                {
                    Console.WriteLine("\nРабота приложения завершена.");
                    break;
                }

                string firstName = ReadFirstName();
                int ticket = random.Next(1, 21);

                Console.WriteLine($"Билет № {ticket}\n");
                SaveStudent(lastName, firstName, ticket);
            }
        }

        static void InitExcel()
        {
            if (!File.Exists(FilePath))
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Journal");
                worksheet.Cell(1, 1).Value = "Last name";
                worksheet.Cell(1, 2).Value = "First name";
                worksheet.Cell(1, 3).Value = "Номер билета";
                worksheet.Cell(1, 4).Value = "Дата и время";
                workbook.SaveAs(FilePath);
            }
        }

        static void SaveStudent(string lastName, string firstName, int ticket)
        {
            while (true)
            {
                try
                {
                    using var workbook = new XLWorkbook(FilePath);
                    var worksheet = workbook.Worksheet(1);
                    var lastRow = worksheet.LastRowUsed().RowNumber();
                    
                    var newRow = worksheet.Row(lastRow + 1);
                    newRow.Cell(1).Value = lastName;
                    newRow.Cell(2).Value = firstName;
                    newRow.Cell(3).Value = ticket;
                    newRow.Cell(4).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    workbook.Save();
                    break; // Успешно сохранили, выходим
                }
                catch (IOException) // Перехват блокировки файла (например, открыт в Excel)
                {
                    Console.WriteLine($"\n[Ошибка] Файл '{FilePath}' открыт в другой программе.");
                    Console.WriteLine("Закройте файл и нажмите Enter для повторной попытки сохранения...");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[Системная ошибка] {ex.Message}");
                    Console.WriteLine("Нажмите Enter для повторной попытки...");
                    Console.ReadLine();
                }
            }
        }

        static string ReadLastName()
        {
            while (true)
            {
                Console.Write("Last name: ");
                var keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Escape)
                    return null;

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("\n[Ошибка] Строка не может быть пустой. Повторите ввод.\n");
                    continue;
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    Console.WriteLine("\n[Ошибка] Некорректный ввод. Повторите.\n");
                    continue;
                }

                // Печатаем первый символ и дочитываем остаток строки
                Console.Write(keyInfo.KeyChar);
                string restOfLine = Console.ReadLine();
                string fullName = (keyInfo.KeyChar + restOfLine).Trim();

                if (!string.IsNullOrWhiteSpace(fullName))
                    return fullName;

                Console.WriteLine("[Ошибка] Строка не может быть пустой. Повторите ввод.\n");
            }
        }

        static string ReadFirstName()
        {
            while (true)
            {
                Console.Write("First name: ");
                string name = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                    return name;

                Console.WriteLine("[Ошибка] Строка не может быть пустой. Повторите ввод.\n");
            }
        }
    }
}