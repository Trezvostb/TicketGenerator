using System.Windows;
using System.Windows.Input;

namespace TicketGeneratorUI
{
    public partial class ResultWindow : Window
    {
        public ResultWindow(string ticketNumber, string questions)
        {
            InitializeComponent();
            
            // Связываем данные с нашими новыми красивыми элементами
            TicketTitle.Text = $"Ваш билет № {ticketNumber}";
            QuestionsText.Text = questions;
        }

        // Восстановленный обработчик нажатия ESC
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}