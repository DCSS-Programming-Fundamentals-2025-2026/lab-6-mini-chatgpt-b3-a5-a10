class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Консольне тестування системи Mini-ChatGPT.");

        BasicModel model = new BasicModel();

        ChatRepl chat = new ChatRepl(model);

        chat.Run(1.0f, 10, null);
    }
}