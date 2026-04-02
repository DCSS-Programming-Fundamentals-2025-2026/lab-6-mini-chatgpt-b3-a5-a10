using NUnit.Framework;
using System;
using System.IO;
using scr;

namespace Integration.Runtime
{
    [TestFixture]
    public class RuntimeIntegrationTests
    {
        [Test]
        public void Test1()
        {
            ISampler sampler = new Sampler();
            ITextGenerator generator = new IntegratedMockModel(sampler);

            ChatRepl repl = new ChatRepl(generator);

            Assert.That(repl, Is.Not.Null);
        }

        [Test]
        public void Test2()
        {
            var repl = new ChatRepl(new BasicModel());

            Console.SetIn(new StringReader("/quit\n"));
            Console.SetOut(new StringWriter());

            repl.Run(1.0f, 5, null);

            Assert.Pass();
        }

        [Test]
        public void Test3()
        {
            ISampler sampler = new Sampler();
            float[] probs = { 0.2f, 0.5f, 0.3f };

            int r1 = sampler.Sample(probs, 1.0f, 3, 50);
            int r2 = sampler.Sample(probs, 1.0f, 3, 50);

            Assert.That(r1, Is.EqualTo(r2));
        }

        [Test]
        public void Test4()
        {
            ISampler sampler = new Sampler();
            float[] probs = { 0.1f, 0.9f, 0.0f };

            for (int i = 0; i < 20; i++)
            {
                int result = sampler.Sample(probs, 1.0f, 1, 100);
                Assert.That(result, Is.EqualTo(1));
            }
        }

        [Test]
        public void Test5()
        {
            ISampler sampler = new Sampler();
            float[] probs = { 0.1f, 0.2f, 0.7f };

            int low = sampler.Sample(probs, 0.3f, 3, 1);
            int high = sampler.Sample(probs, 2.0f, 3, 1);

            Assert.That(low, Is.InRange(0, 2));
            Assert.That(high, Is.InRange(0, 2));
        }
    }
}

