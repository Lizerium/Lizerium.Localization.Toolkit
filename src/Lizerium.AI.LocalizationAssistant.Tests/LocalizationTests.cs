/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 мая 2026 06:52:43
 * Version: 1.0.6
 */

using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Services;

using Xunit.Abstractions;

namespace Lizerium.AI.LocalizationAssistant.Tests
{
    public class LocalizationTests
    {
        private readonly ITestOutputHelper _output;

        public LocalizationTests(ITestOutputHelper output) => _output = output;

        #region Helpers

        private void AssertValid(LocalizationResult? result, string input)
        {
            Assert.NotNull(result);

            Assert.False(string.IsNullOrWhiteSpace(result.En), "EN is empty");
            _output.WriteLine(result.En);
            Assert.False(string.IsNullOrWhiteSpace(result.Ru), "RU is empty");
            _output.WriteLine(result.Ru);

            if (input.Contains("{0}"))
            {
                Assert.Contains("{0}", result.En);
                Assert.Contains("{0}", result.Ru);
            }
        }

        private static void AssertWithErrors(LocalizationResult result, ITestOutputHelper output)
        {
            Assert.NotNull(result);

            output.WriteLine("=== RESULT ===");
            output.WriteLine(result.ToString());

            if (result.LocErrors.Count == 0)
            {
                output.WriteLine("LLM: perfect (no errors)");
            }
            else
            {
                output.WriteLine($"LLM errors: {result.LocErrors.Count}");

                foreach (var err in result.LocErrors)
                {
                    output.WriteLine($" - {err.Type}: {err.Text}");
                }
            }

            // базовые проверки
            Assert.False(string.IsNullOrWhiteSpace(result.En));
            Assert.False(string.IsNullOrWhiteSpace(result.Ru));
        }

        #endregion

        [Fact]
        public async Task ProcessAsync_Should_Parse_Valid_Response()
        {
            var aiClient = new FakeAiClient();
            var service = new AILocalizationService(aiClient, new PromtConfig());

            var result = await service.ProcessAsync(
                "Directory not found");

            Assert.NotNull(result);

            Assert.Equal("Directory not found", result.En);
            Assert.Equal("Тест", result.Ru);
            AssertWithErrors(result, _output);
        }

        [Theory]
        [InlineData("Russian", "Russian", "Русский")]
        [InlineData("Русский", "Russian", "Русский")]
        public async Task ProcessAsync_Should_Preserve_Source_Language_Value(string input, string expectedEn, string expectedRu)
        {
            var aiClient = new FakeAiClient();
            var service = new AILocalizationService(aiClient, new PromtConfig());

            var result = await service.ProcessAsync(input);

            Assert.NotNull(result);
            Assert.Equal(expectedEn, result.En);
            Assert.Equal(expectedRu, result.Ru);
        }

        [Fact]
        public async Task ProcessAsync_Should_Use_Known_Ui_Glossary_For_Language_Labels()
        {
            var aiClient = new ConfusedLanguageAiClient();
            var service = new AILocalizationService(aiClient, new PromtConfig());

            var result = await service.ProcessAsync("Russian");

            Assert.NotNull(result);
            Assert.Equal("Russian", result.En);
            Assert.Equal("Русский", result.Ru);
        }

        /// <summary>
        /// Smoke test
        /// </summary>
        [Theory]
        [InlineData("File not found")]
        [InlineData("Invalid name")]
        [InlineData("Access denied")]

        public async Task ProcessAsync_Should_Work_With_Different_Inputs(string input)
        {
            var aiClient = new FakeAiClient();
            var service = new AILocalizationService(aiClient, new PromtConfig());

            var result = await service.ProcessAsync(
                input);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.En));
            Assert.False(string.IsNullOrWhiteSpace(result.Ru));

            AssertWithErrors(result, _output);
        }


        [Fact]
        public async Task ProcessAsync_Should_Handle_Invalid_Json()
        {
            var aiClient = new BrokenAiClient();
            var service = new AILocalizationService(aiClient, new PromtConfig());

            var result = await service.ProcessAsync(
                "test");

            Assert.Null(result);
        }

        [Theory]
        [InlineData("Directory not found: {0}")]
        [InlineData("File not found: {0}")]
        [InlineData("Access denied: {0}")]
        [InlineData("Invalid file name: {0}")]
        [InlineData("User does not have permission")]
        public async Task LLM_Should_Handle_Multiple_Inputs(string input)
        {
            var ollama = new OllamaClient("http://localhost:11434");
            var service = new AILocalizationService(ollama, new PromtConfig());

            var result = await service.ProcessAsync(
                input);

            AssertValid(result, input);
        }

        [Fact]
        public async Task LLM_Should_Use_Context_In_Key()
        {
            var ollama = new OllamaClient("http://localhost:11434");
            var service = new AILocalizationService(ollama, new PromtConfig());

            var result = await service.ProcessAsync(
                "Directory not found: {0}");

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.En));
            Assert.False(string.IsNullOrWhiteSpace(result.Ru));

            // проверяем, что контекст попал в ключ
            AssertWithErrors(result, _output);
        }

        [Fact]
        public async Task LLM_Should_Respect_Category()
        {
            var ollama = new OllamaClient("http://localhost:11434");
            var service = new AILocalizationService(ollama, new PromtConfig());

            var result = await service.ProcessAsync(
                "Directory not found: {0}");

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.En));
            Assert.False(string.IsNullOrWhiteSpace(result.Ru));

            AssertWithErrors(result, _output);
        }

        [Theory]
        [InlineData("File not found: {0}")]
        [InlineData("Запуск процесса сканирования файла: {0}")]
        [InlineData("Сканирую файл: {0}")]
        public async Task LLM_Should_Generate_Valid_Localization(string inputText)
        {
            var ollama = new OllamaClient("http://localhost:11434");

            var service = new AILocalizationService(ollama, new PromtConfig());

            var result = await service.ProcessAsync(
                inputText);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.En));
            Assert.False(string.IsNullOrWhiteSpace(result.Ru));

            // ключ
            Assert.False(string.IsNullOrWhiteSpace(result.En), "EN is empty");
            Assert.False(string.IsNullOrWhiteSpace(result.Ru), "RU is empty");

            // переводы
            Assert.False(string.IsNullOrWhiteSpace(result.En));
            Assert.False(string.IsNullOrWhiteSpace(result.Ru));

            // placeholder не потерян
            Assert.Contains("{0}", result.En);
            Assert.Contains("{0}", result.Ru);
            AssertWithErrors(result, _output);
        }
    }
}
