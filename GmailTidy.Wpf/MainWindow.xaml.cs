using System;
using System.Threading.Tasks;
using System.Windows;
using GmailTidy.Core;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using System.Linq;

namespace GmailTidy.Wpf
{
    public partial class MainWindow : Window
    {
        private Google.Apis.Gmail.v1.GmailService _service;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Authorize_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Text = "Authorizing...";
            _service = GmailClient.CreateService();
            OutputBox.Text = "Authorized.";
        }

        private async void Purge_Click(object sender, RoutedEventArgs e)
        {
            if (_service == null)
            {
                OutputBox.Text = "Please authorize first.";
                return;
            }
            OutputBox.Text = "Purging Social/Promotions...";
            int count = await Cleaner.PurgeSocialPromotionsAsync(_service, true);
            OutputBox.Text = $"Purged {count} messages.";
        }

        private async void TrashFilter_Click(object sender, RoutedEventArgs e)
        {
            if (_service == null)
            {
                OutputBox.Text = "Please authorize first.";
                return;
            }
            OutputBox.Text = "Creating trash filter...";
            await Filters.CreateTrashSocialPromotionsAsync(_service, true);
            OutputBox.Text = "Trash filter created.";
        }

        private async void KeeperFilters_Click(object sender, RoutedEventArgs e)
        {
            if (_service == null)
            {
                OutputBox.Text = "Please authorize first.";
                return;
            }
            OutputBox.Text = "Creating keeper filters...";
            await Filters.CreateFinanceKeepersAsync(_service);
            try { await Filters.CreateTravelKeepersAsync(_service); } catch { }
            try { await Filters.CreateSubscriptionsKeepersAsync(_service); } catch { }
            try { await Filters.CreateTicketsKeepersAsync(_service); } catch { }
            try { await Filters.CreateShoppingKeepersAsync(_service); } catch { }
            OutputBox.Text = "Keeper filters created.";
        }

        private async void Schedule_Click(object sender, RoutedEventArgs e)
        {
            if (_service == null)
            {
                OutputBox.Text = "Please authorize first.";
                return;
            }
            OutputBox.Text = "Scheduling daily run...";
                        await Scheduler.CreateDailyTaskAsync("GmailTidy", "09:00", System.Reflection.Assembly.GetEntryAssembly().Location);

            OutputBox.Text = "Daily run scheduled.";
        }


         private async void Analyze_Click(object sender, RoutedEventArgs e)
        {
            if (_service == null)
            {
                OutputBox.Text = "Please authorize first.";
                return;
            }
            OutputBox.Text = "Analyzing engagement...";
       await EngagementAnalyAnalyzeEngagementAndCreateRulesAsync(_service)rvice;
            OutputBox.Text = "Engagement analysis complete.";
        }

        private async void ClassifyOnce_Click(object sender, RoutedEventArgs e)
            if (_service == null)
        {
            OutputBox.Text = "Please authorize first.";
            return;
        }
        OutputBox.Text = "Classifying last message...";
        var listRequest = _service.Users.Messages.List("me");
        listRequest.MaxResults = 1;
        var listResponse = await listRequest.ExecuteAsync();
        if (listResponse.Messages == null || listResponse.Messages.Count == 0)
        {
            OutputBox.Text = "No messages found.";
            return;
        }
        var messageId = listResponse.Messages[0].Id;
        var message = await _service.Users.Messages.Get("me", messageId).ExecuteAsync();
        string subject = "";
        string snippet = message.Snippet ?? "";
        string from = "";
        if (message.Payload?.Headers != null)
        {
            foreach (var header in message.Payload.Headers)
            {
                if (string.Equals(header.Name, "Subject", StringComparison.OrdinalIgnoreCase))
                    subject = header.Value;
                else if (string.Equals(header.Name, "From", StringComparison.OrdinalIgnoreCase))
                    from = header.Value;
            }
        }
        var classification = await Classifier.ClassifyAsync(subject, snippet, from);
        OutputBox.Text = $"Classification: {classification}";

            {
                OutputBox.Text = "Please authorize first.";
                return;
            }
            OutputBox.Text = "Classifying last message...";
            // Example: classify the most recent message snippet (not implemented) - placeholder
            OutputBox.Text = "Classification complete.";
        }
    }
}
