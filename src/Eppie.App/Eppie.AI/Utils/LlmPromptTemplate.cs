namespace Eppie.AI
{

    internal class LlmPromptTemplate
    {
        public string System { get; set; }
        public string User { get; set; }
        public string Assistant { get; set; }
        public string[] Stop { get; set; }
    }
}
