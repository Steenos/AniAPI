﻿using Microsoft.Extensions.Hosting;
using SyncService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Workers
{
    public class AnimeScraperWorker : BackgroundService
    {
        private IService _service = new AnimeScraperService();

        public bool HasDoneFirstRound { get; private set; } = false;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _service.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _service.Work();
                    HasDoneFirstRound = true;

                    _service.Wait();
                }
                catch(Exception ex)
                {
                    _service.Stop(ex);
                }
            }
        }
    }
}
