using Google.Apis.Gmail.v1;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Gmail.v1.Data;

namespace GmailTidy.Core
{
    public static class Cleaner
    {
        public static async Task<int> PurgeSocialPromotionsAsync(GmailService svc, bool sendToTrash = true)
        {
            var ids = await SearchAllIdsAsync(svc, "category:social OR category:promotions");
            if (ids.Count == 0) return 0;

            var request = new BatchModifyMessagesRequest
            {
                Ids = ids,
                AddLabelIds = sendToTrash ? new[] { "TRASH" } : null,
                RemoveLabelIds = sendToTrash ? null : new[] { "INBOX" }
            };
            await svc.Users.Messages.BatchModify(request, "me").ExecuteAsync();
            return ids.Count;
        }

        public static async Task<IList<string>> SearchAllIdsAsync(GmailService svc, string query)
        {
            var ids = new List<string>();
            var req = svc.Users.Messages.List("me");
            req.Q = query;
            req.IncludeSpamTrash = false;

            ListMessagesResponse resp;
            do
            {
                resp = await req.ExecuteAsync();
                if (resp.Messages != null)
                    ids.AddRange(resp.Messages.Select(m => m.Id));
                req.PageToken = resp.NextPageToken;
            } while (!string.IsNullOrEmpty(req.PageToken));

            return ids;
        }
    }
}
