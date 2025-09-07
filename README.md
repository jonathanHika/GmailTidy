# GmailTidy

GmailTidy is a Windows desktop application built with .NET (WPF) that helps you clean up your Gmail by bulk purging Social/Promotions categories and creating filters to automatically archive or trash future Social and Promotions emails. It also creates keeper filters for important categories like Finance, Travel, Subscriptions, Tickets, and Shopping. The project consists of two parts:

- **GmailTidy.Core**: a .NET library that handles Gmail API authentication via OAuth 2.0, searches for Social and Promotions messages, bulk modifies messages to Trash or Archive, and creates filters for Social/Promotions and Finance, Travel, Subscriptions, Tickets, Shopping categories. It includes optional AI classification via the OpenAI .NET SDK.

- **GmailTidy.Wpf**: a WPF application that provides a simple user interface to authorize with Gmail, purge existing Social and Promotions messages, create filters to automate the process, create keeper filters, and schedule a daily cleanup using Windows Task Scheduler.

To get started, enable the Gmail API in your Google Cloud console, download a `client_secret.json` file for a desktop application, and place it next to the executable. See `push-to-github.ps1` for instructions on pushing the repository.

## Features

- One-click purge of existing Social and Promotions messages
- Creates Gmail filters to auto-archive or auto-trash Social and Promotions
- Creates keeper filters for Finance, Travel, Subscriptions, Tickets, and Shopping categories
- Optional OpenAI classification of ambiguous messages
- WPF UI with buttons for Purge, Create Filters, Keeper Filters, and daily scheduling
- PowerShell script `push-to-github.ps1` for easy git push of the solution to GitHub

## Scheduling Settings

The application includes a scheduling settings feature that allows you to control when the daily purge and filter creation routine runs. By default, the scheduled task runs each day at 9:00 AM, but you can adjust the time in the hidden developer settings panel within the WPF app or by modifying the `appsettings.json` file. When adding an API key or toggling direct vs. server mode, the settings are also accessible. Scheduled runs are registered with Windows Task Scheduler.
