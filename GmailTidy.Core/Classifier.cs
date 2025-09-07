using System;
using System.Threading.Tasks;

namespace GmailTidy.Core
{
    public static class Classifier
    {
        /// <summary>
        /// Classifies an email based on subject, snippet, and from fields into one of: NEEDS_REPLY, PROMO, KEEP.
        /// Uses simple heuristic rules instead of AI when API is unavailable.
        /// </summary>
        /// <param name="subject">Subject of the email</param>
        /// <param name="snippet">Snippet of the email body</param>
        /// <param name="from">From header</param>
        /// <returns>A classification string: NEEDS_REPLY, PROMO, or KEEP</returns>
        public static Task<string> ClassifyAsync(string subject, string snippet, string from)
        {
            // Combine subject and snippet for analysis
            var content = ((subject ?? "") + " " + (snippet ?? "")).ToLowerInvariant();

            // Keywords indicating finance/receipt/invoice that should be kept
            if (content.Contains("invoice") || content.Contains("receipt") || content.Contains("payment") || content.Contains("statement") || content.Contains("bill"))
            {
                return Task.FromResult("KEEP");
            }

            // Promotional keywords that likely indicate marketing or spam
            if (content.Contains("sale") || content.Contains("promo") || content.Contains("promotion") || content.Contains("deal") || content.Contains("discount") || content.Contains("offer") || content.Contains("coupon"))
            {
                return Task.FromResult("PROMO");
            }

            // Keywords or symbols indicating a question or request that might need a reply
            if (content.Contains("?") || content.Contains("please reply") || content.Contains("can you") || content.Contains("could you") || content.Contains("let me know") || content.Contains("kindly respond"))
            {
                return Task.FromResult("NEEDS_REPLY");
            }

            // Default classification
            return Task.FromResult("KEEP");
        }
    }
}
