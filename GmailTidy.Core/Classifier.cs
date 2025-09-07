using System.Threading.Tasks;

namespace GmailTidy.Core
{
    public static class Classifier
    {
        public static Task<string> ClassifyAsync(string subject, string snippet, string from)
        {
            // Always return KEEP in this stub implementation
            return Task.FromResult("KEEP");
        }
    }
}
