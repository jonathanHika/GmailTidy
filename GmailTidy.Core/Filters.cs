using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;

namespace GmailTidy.Core
{
    public static class Filters
    {
        public static async Task<string> CreateTrashSocialPromotionsAsync(GmailService svc, bool trashInsteadOfArchive = true)
        {
            var filter = new Filter
            {
                Criteria = new FilterCriteria { Query = "category:social OR category:promotions" },
                Action = trashInsteadOfArchive
                    ? new FilterAction { AddLabelIds = new[] { "TRASH" } }
                    : new FilterAction { RemoveLabelIds = new[] { "INBOX" } }
            };

            var created = await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            return created.Id;
        }

        public static async Task<string> CreateFinanceKeepersAsync(GmailService svc)
        {
            var financeLabelId = await EnsureUserLabelAsync(svc, "Finance");
            var filter = new Filter
            {
                Criteria = new FilterCriteria
                {
                    Query = "(zelle OR receipt OR invoice OR statement OR sdge OR \"your payment\" OR chase OR paypal OR stripe)"
                },
                Action = new FilterAction
                {
                    AddLabelIds = new[] { financeLabelId },
                    RemoveLabelIds = new[] { "SPAM" }
                }
            };
            var created = await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            return created.Id;
        }

        public static async Task<string> CreateTravelKeepersAsync(GmailService svc)
        {
            var labelId = await EnsureUserLabelAsync(svc, "Travel");
            var filter = new Filter
            {
                Criteria = new FilterCriteria
                {
                    Query = "(boarding pass OR itinerary OR delta OR united OR southwest OR aa.com OR alaskaair OR booking.com OR airbnb OR expedia OR trip OR reservation)"
                },
                Action = new FilterAction
                {
                    AddLabelIds = new[] { labelId },
                    RemoveLabelIds = new[] { "SPAM" }
                }
            };
            var created = await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            return created.Id;
        }

        public static async Task<string> CreateSubscriptionsKeepersAsync(GmailService svc)
        {
            var labelId = await EnsureUserLabelAsync(svc, "Subscriptions");
            var filter = new Filter
            {
                Criteria = new FilterCriteria
                {
                    Query = "(renewal OR subscription OR trial OR unsubscribe)"
                },
                Action = new FilterAction
                {
                    AddLabelIds = new[] { labelId },
                    RemoveLabelIds = new[] { "SPAM" }
                }
            };
            var created = await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            return created.Id;
        }

        public static async Task<string> CreateTicketsKeepersAsync(GmailService svc)
        {
            var labelId = await EnsureUserLabelAsync(svc, "Tickets");
            var filter = new Filter
            {
                Criteria = new FilterCriteria
                {
                    Query = "(ticket OR qr code OR admission OR pass OR eventbrite)"
                },
                Action = new FilterAction
                {
                    AddLabelIds = new[] { labelId },
                    RemoveLabelIds = new[] { "SPAM" }
                }
            };
            var created = await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            return created.Id;
        }

        public static async Task<string> CreateShoppingKeepersAsync(GmailService svc)
        {
            var labelId = await EnsureUserLabelAsync(svc, "Shopping");
            var filter = new Filter
            {
                Criteria = new FilterCriteria
                {
                    Query = "(order confirmation OR shipped OR amazon OR etsy OR ebay OR best buy OR homedepot)"
                },
                Action = new FilterAction
                {
                    AddLabelIds = new[] { labelId },
                    RemoveLabelIds = new[] { "SPAM" }
                }
            };
            var created = await svc.Users.Settings.Filters.Create(filter, "me").ExecuteAsync();
            return created.Id;
        }

        private static async Task<string> EnsureUserLabelAsync(GmailService svc, string name)
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
            var createdLabel = await svc.Users.Labels.Create(newLabel, "me").ExecuteAsync();
            return createdLabel.Id;
        }
    }
}
