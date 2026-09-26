using System;
using System.IO;
using Xunit;
using TicketGenerator; 

namespace TicketGenerator.Tests
{
    public class GeneratorTests
    {
        [Fact]
        public void ReadFirstName_ValidInput_ReturnsName()
        {
            // 1. Подготавливаем виртуальный ввод (как будто пользователь набрал "Иван" и нажал Enter)
            var simulatedInput = new StringReader("Иван\n");
            Console.SetIn(simulatedInput);

            // 2. Запускаем вашу функцию
            var result = Program.ReadFirstName();

            // 3. Проверяем, что функция вернула именно то, что мы ввели
            Assert.Equal("Иван", result);
        }
    }
}