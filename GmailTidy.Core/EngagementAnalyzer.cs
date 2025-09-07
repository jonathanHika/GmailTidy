using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;

namespace GmailTidy.Core
{
    /// <summary>
    /// Analyzes user engagement with messages (responded, read, unread) and creates Gmail labels and filters
    /// for those senders so future messages are automatically categorized.
    /// </summary>
    public static class EngagementAnalyzer
    {
        /// <summary>
        /// Scans a sample of recent messages, categorizes senders based on whether the user has responded,
        /// read without responding, or left unread, and then creates labels and filters so new messages
        /// from those senders are labelled accordingly.
        /// </summary>
        /// <param name="svc">An authenticated GmailService instance.</param>
        public static async Task AnalyzeEngagementAndCreateRulesAsync(GmailService svc)
        {
            if (svc == null) throw new ArgumentNullException(nameof(svc));

            // Fetch a limited set of recent messages. Adjust MaxResults as needed.
            var listRequest = svc.Users.Messages.List("me");
            listRequest.Q = ""; // empty query to include all
            listRequest.MaxResults = 100;
            listRequest.IncludeSpamTrash = false;
            var listResponse = await listRequest.ExecuteAsync();

            var respondedSenders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var readSenders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var unreadSenders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (listResponse.Messages != null)
            {
                foreach (var msg in listResponse.Messages)
                {
                    // Get full message to access headers and labels
                    var full = await svc.Users.Messages.Get("me", msg.Id).ExecuteAsync();
                    var fromHeader = full.Payload?.Headers?.FirstOrDefault(h => h.Name.Equals("From", StringComparison.OrdinalIgnoreCase))?.Value;
                    if (string.IsNullOrEmpty(fromHeader))
                    {
                        continue;
                    }
                    var email = ExtractEmailAddress(fromHeader);
                    bool isUnread = full.LabelIds?.Contains("UNREAD") ?? false;

                    bool responded = false;
                    // Determine if user responded within the thread by checking for a SENT message
                    if (!string.IsNullOrEmpty(full.ThreadId))
                    {
                        var thread = await svc.Users.Threads.Get("me", full.ThreadId).ExecuteAsync();
                        if (thread.Messages != null)
                        {
                            responded = thread.Messages.Any(m => m.LabelIds != null && m.LabelIds.Contains("SENT"));
                        }
                    }

                    if (responded)
                    {
                        respondedSenders.Add(email);
                    }
                    else if (isUnread)
                    {
                        unreadSenders.Add(email);
                    }
                    else
                    {
                        readSenders.Add(email);
                    }
                }
            }

            // Create or ensure labels exist
            string respondedLabelId = await EnsureLabelExistsAsync(svc, "RespondedSenders");
            string readLabelId = await EnsureLabelExistsAsync(svc, "ReadSenders");
            string unreadLabelId = await EnsureLabelExistsAsync(svc, "UnreadSenders");

            // Create filters for each category
            foreach (var sender in respondedSenders)
            {
                var filter = new Filter
                {
                    Criteria = new FilterCriteria { From = sender },
                    Action = new FilterAction { AddLabelIds = new[] { respondedLabelId } }
                };
                await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            }

            foreach (var sender in readSenders)
            {
                var filter = new Filter
                {
                    Criteria = new FilterCriteria { From = sender },
                    Action = new FilterAction { AddLabelIds = new[] { readLabelId } }
                };
                await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            }

            foreach (var sender in unreadSenders)
            {
                var filter = new Filter
                {
                    Criteria = new FilterCriteria { From = sender },
                    Action = new FilterAction { AddLabelIds = new[] { unreadLabelId } }
                };
                await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            }
        }

        /// <summary>
        /// Extracts an email address from a From header. If no angle brackets are present, returns the original string.
        /// </summary>
        private static string ExtractEmailAddress(string fromHeader)
        {
            int start = fromHeader.IndexOf('<');
            int end = fromHeader.IndexOf('>');
            if (start >= 0 && end > start)
            {
                return fromHeader.Substring(start + 1, end - start - 1).Trim();
            }
            return fromHeader.Trim();
        }

        /// <summary>
        /// Ensures a user-defined label exists. If it does not, it will be created.
        /// Returns the label ID.
        /// </summary>
        private static async Task<string> EnsureLabelExistsAsync(GmailService svc, string name)
        {
            var labels = await svc.Users.Labels.List("me").ExecuteAsync();
            var existing = labels.Labels?.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return existing.Id;
            }

            var created = await svc.Users.Labels.Create(new Label
            {
                Name = name,
                LabelListVisibility = "labelShow",
                MessageListVisibility = "show"
            }, "me").ExecuteAsync();
            return created.Id;
        }
    }
}
