using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TicketGeneratorUI
{
    public partial class MainWindow : Window
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ITicketService _ticketService;
        private readonly IResultRepository _resultRepository;

        public MainWindow()
        {
            InitializeComponent();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Composition Root
            _studentRepository = new ExcelStudentRepository(Path.Combine(baseDir, "students.xlsx"));
            var ticketRepo = new WordTicketRepository(Path.Combine(baseDir, "tickets.docx"));
            _resultRepository = new ExcelResultRepository(Path.Combine(baseDir, "results.xlsx"));

            _ticketService = new TicketService(ticketRepo, _resultRepository);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GroupComboBox.Items.Clear();
                foreach (var group in _studentRepository.GetGroups())
                {
                    GroupComboBox.Items.Add(group);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке групп: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupComboBox.SelectedItem is not string group) return;

            StudentComboBox.Items.Clear();
            GenerateButton.IsEnabled = false;

            try
            {
                foreach (var student in _studentRepository.GetStudentsByGroup(group))
                {
                    StudentComboBox.Items.Add(student);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке студентов: {ex.Message}", "Ошибка");
            }
        }

        private void StudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateButton.IsEnabled = StudentComboBox.SelectedItem != null;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (GroupComboBox.SelectedItem is not string group ||
                StudentComboBox.SelectedItem is not Student student)
                return;

            try
            {
                var result = _ticketService.GenerateTicketForStudent(group, student);

                if (result.IsRepeat)
                {
                    MessageBox.Show(
                        $"Студент {student.DisplayName} уже тянул билет ранее!\n\n" +
                        $"Согласно правилам, ему повторно выдается его билет №{result.Ticket.Number}.",
                        "Защита от пересдачи", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                var resultWindow = new ResultWindow(
                    result.Ticket.Number.ToString(),
                    string.Join("\n", result.Ticket.Questions))
                {
                    Owner = this
                };
                resultWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сформировать билет: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _resultRepository.ClearHistory();
                MessageBox.Show("История выдачи билетов успешно очищена!", "Сброс базы", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось очистить историю: {ex.Message}", "Ошибка");
            }
        }
    }
}