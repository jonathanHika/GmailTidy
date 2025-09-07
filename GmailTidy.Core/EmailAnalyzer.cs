using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;

namespace GmailTidy.Core
{
    /// <summary>
    /// Analyzes user email interactions (read vs. unread) and creates Gmail labels and filters for those senders.
    /// </summary>
    public static class EmailAnalyzer
    {
        /// <summary>
        /// Scans a sample of recent messages, categorizes senders by whether their messages are read or unread,
        /// and creates labels and filters so future messages from those senders are automatically labelled accordingly.
        /// </summary>
        /// <param name="svc">An authenticated GmailService instance.</param>
        public static async Task AnalyzeAndCreateRulesAsync(GmailService svc)
        {
            if (svc == null) throw new ArgumentNullException(nameof(svc));

            // Fetch a limited set of recent messages. You can adjust MaxResults for deeper analysis.
            var listRequest = svc.Users.Messages.List("me");
            listRequest.Q = ""; // empty query to include all
            listRequest.MaxResults = 100; // analyze first 100 messages
            listRequest.IncludeSpamTrash = false;

            var listResponse = await listRequest.ExecuteAsync();

            var readSenders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var unreadSenders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (listResponse.Messages != null)
            {
                foreach (var msg in listResponse.Messages)
                {
                    var getRequest = svc.Users.Messages.Get("me", msg.Id);
                    getRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Metadata;
                    getRequest.MetadataHeaders = new[] { "From" };
                    var full = await getRequest.ExecuteAsync();

                    var headers = full.Payload?.Headers;
                    var from = headers?.FirstOrDefault(h => h.Name.Equals("From", StringComparison.OrdinalIgnoreCase))?.Value;
                    if (!string.IsNullOrEmpty(from))
                    {
                        var email = ExtractEmailAddress(from);
                        bool isUnread = full.LabelIds?.Contains("UNREAD") ?? false;
                        if (isUnread)
                        {
                            unreadSenders.Add(email);
                        }
                        else
                        {
                            readSenders.Add(email);
                        }
                    }
                }
            }

            // Create or fetch user labels
            string readLabelId = await EnsureLabelExistsAsync(svc, "ReadSenders");
            string unreadLabelId = await EnsureLabelExistsAsync(svc, "UnreadSenders");

            // Create filters for senders who usually read messages
            foreach (var sender in readSenders)
            {
                var filter = new Filter
                {
                    Criteria = new FilterCriteria { From = sender },
                    Action = new FilterAction { AddLabelIds = new[] { readLabelId } }
                };
                await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            }

            // Create filters for senders whose messages are often left unread
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

            var newLabel = new Label
            {
                Name = name,
                LabelListVisibility = "labelShow",
                MessageListVisibility = "show"
            };
            var created = await svc.Users.Labels.Create(newLabel, "me").ExecuteAsync();
            return created.Id;
        }
    }
}
