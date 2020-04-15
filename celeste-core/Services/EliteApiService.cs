﻿using EliteAPI;
using Somfic.Logging;
using Somfic.Logging.Handlers;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Celeste.Services
{
    public class EliteApiService
    {
        private readonly EliteDangerousAPI _eliteAPI;
        private readonly SettingsService _settings;

        public event EventHandler<EliteAPI.Events.EventBase> OnNewEvent;

        public EliteApiService(SettingsService settings)
        {
            _settings = settings;
            _settings.OnSettingsChange += OnSettingsChange;
            
            _eliteAPI = new EliteDangerousAPI();

            SetupEliteApi();

            _eliteAPI.Start();
        }

        private void OnSettingsChange(object sender, Models.Settings e)
        {
            Console.WriteLine("[EliteApiService] Detected Settings Change");
            DirectoryInfo journalDirectory = new DirectoryInfo(_settings.Get().JournalDirectory);
            _eliteAPI.ChangeJournal(journalDirectory);
        }

        private void SetupEliteApi() 
        { 
            Logger.AddHandler(new LogFileHandler(Directory.GetCurrentDirectory(), "EliteAPI"));
            Logger.AddHandler(new ConsoleHandler(), Severity.Info, Severity.Verbose, Severity.Warning, Severity.Error, Severity.Success);

            _eliteAPI.OnReset += (s, e) => {
                _eliteAPI.Events.CommanderEvent += HandleCommanderInfo;
                _eliteAPI.Events.AllEvent += HandleEvent;
            };
        }

        private void HandleCommanderInfo(object sender, EliteAPI.Events.Startup.CommanderInfo e)
        {
            Console.WriteLine($"Commander detected is: {e.Name}");
        }

        private void HandleEvent(object sender, dynamic ev)
        {
            // Upload to all Inara accounts
            // Upload to all EDSM accounts
            // Upload to EDDN?

            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-");
            if (ev is EliteAPI.Events.EventBase)
            {
                var eventBase = ev as EliteAPI.Events.EventBase;
                Console.WriteLine($"Event Handled: {eventBase}");
                OnNewEvent?.Invoke(this, eventBase);
            }
            else if (ev is Newtonsoft.Json.Linq.JObject)
            {
                var json = ev as Newtonsoft.Json.Linq.JObject;
                Console.WriteLine($"Raw event: {json}");
            }
            else
            {
                var t = ev.GetType();
                Console.WriteLine($"Unknown: {t.ToString()}");
            }
        }
    }
}