using scr;

namespace Integration.Runtime
{
    [TestFixture]
    public class RuntimeIntegrationTests
    {
        private TextWriter _originalOut;
        private TextReader _originalIn;

        [SetUp]
        public void Setup()
        {
            _originalOut = Console.Out;
            _originalIn = Console.In;
        }

        [TearDown]
        public void TearDown()
        {
            Console.SetOut(_originalOut);
            Console.SetIn(_originalIn);
        }
        
        [Test]
        public void ChatConsoleAndSampler_CanCreateRepl()
        {
            ISampler sampler = new Sampler();
            ITextGenerator generator = new IntegratedMockModel(sampler);

            ChatRepl repl = new ChatRepl(generator);

            Assert.That(repl, Is.Not.Null);
        }

        [Test]
        public void Sampler_WithSeed_ProducesSameResult()
        {
            ISampler sampler = new Sampler();
            float[] probs = { 0.2f, 0.5f, 0.3f };

            int r1 = sampler.SampleWithSeed(probs, 1.0f, 3, 50);
            int r2 = sampler.SampleWithSeed(probs, 1.0f, 3, 50);

            Assert.That(r1, Is.EqualTo(r2));
        }
        
        [Test]
        public void Sampler_TemperatureAffectsDistribution()
        {
            ISampler sampler = new Sampler();
            float[] probs = new float[] { 0.1f, 0.8f, 0.1f };
            
            int result = sampler.SampleWithSeed(probs, 0.001f, 3, 123);
            
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void Sampler_TopK_ReturnsOnlyTopKIndices()
        {
            ISampler sampler = new Sampler();
            float[] probs = { 0.1f, 0.9f, 0.0f };
            
            int result = sampler.SampleWithSeed(probs, 1.0f, 1, 100);
                
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void ChatRepl_WithStubModel_GeneratesText()
        {
            ISampler sampler = new Sampler();
            ITextGenerator generator = new IntegratedMockModel(sampler);
            ChatRepl repl = new ChatRepl(generator);

            StringWriter outWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetIn(new StringReader("Перевірка!\n/quit\n"));
            
            repl.Run(1.0f, 5, 37);
            
            string output = outWriter.ToString();
            
            bool hasModelOutput = output.Contains("Модель>");
            Assert.That(hasModelOutput, Is.True);
        }
        
        [Test]
        public void ResetCommand_RestoresDefaultContext()
        {
            ITextGenerator generator = new BasicModel();
            ChatRepl repl = new ChatRepl(generator);

            StringWriter outWriter = new StringWriter();
            Console.SetOut(outWriter);

            Console.SetIn(new StringReader("/temp 2\nТест1\n/reset\nТест2\n/quit\n"));

            repl.Run(1.0f, 5, null);
            string output = outWriter.ToString();
            
            bool hasTempTwo = output.Contains("Поточна температура: 2");
            bool hasTempOne = output.Contains("Поточна температура: 1");

            Assert.That(hasTempTwo, Is.True);
            Assert.That(hasTempOne, Is.True);
        }

        [Test]
        public void UnknownCommand_ShowsErrorMessage()
        {
            ITextGenerator generator = new BasicModel();
            ChatRepl repl = new ChatRepl(generator);

            StringWriter outWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetIn(new StringReader("/invalid_command\n/quit\n"));

            repl.Run(1.0f, 5, null);
            string output = outWriter.ToString();

            bool hasError = output.Contains("Невідома команда");
            Assert.That(hasError, Is.True);
        }

        [Test]
        public void HelpCommand_DisplaysCommandList()
        {
            ITextGenerator generator = new BasicModel();
            ChatRepl repl = new ChatRepl(generator);

            StringWriter outWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetIn(new StringReader("/help\n/quit\n"));

            repl.Run(1.0f, 5, null);
            string output = outWriter.ToString();

            bool hasHelpHeader = output.Contains("Доступні команди:");
            bool hasTempCommand = output.Contains("/temp");
            bool hasQuitCommand = output.Contains("/quit");

            Assert.That(hasHelpHeader, Is.True);
            Assert.That(hasTempCommand, Is.True);
            Assert.That(hasQuitCommand, Is.True);
        }

        [Test]
        public void EmptyInput_IsIgnoredAndDoesNotGenerate()
        {
            ITextGenerator generator = new BasicModel();
            ChatRepl repl = new ChatRepl(generator);

            StringWriter outWriter = new StringWriter();
            Console.SetOut(outWriter);

            Console.SetIn(new StringReader("   \n\n\nСлово\n/quit\n"));

            repl.Run(1.0f, 5, null);
            string output = outWriter.ToString();

            string target = "Модель>";
            int count = 0;
            int index = output.IndexOf(target);

            while (index != -1)
            {
                count++;
                index = output.IndexOf(target, index + target.Length);
            }

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void ModelAgnostic_PassesPromptCorrectly()
        {
            ITextGenerator generator = new BasicModel();
            ChatRepl repl = new ChatRepl(generator);

            StringWriter outWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetIn(new StringReader("Перевірка!\n/quit\n"));

            repl.Run(1.0f, 5, null);
            string output = outWriter.ToString();

            bool hasPrompt = output.Contains("Перевірка!");
            Assert.That(hasPrompt, Is.True);
        }
    }
}