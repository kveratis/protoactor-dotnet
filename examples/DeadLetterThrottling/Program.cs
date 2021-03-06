﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proto;

namespace DeadLetterThrottling
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.SetLoggerFactory(LoggerFactory.Create(l => l.AddConsole().SetMinimumLevel(LogLevel.Information)));

            var system = new ActorSystem(new ActorSystemConfig()
                .WithDeadLetterThrottleInterval(TimeSpan.FromSeconds(1))
                .WithDeadLetterThrottleCount(3)
            );

            var props = Props.FromFunc(c => Task.CompletedTask);

            //spawn an actor
            var pid = system.Root.Spawn(props);

            //stop it, so that any messages sent to it are delivered to DeadLetter stream
            await system.Root.StopAsync(pid);
            
            for (int i = 0; i < 1000; i++)
            {
                system.EventStream.Publish(new DeadLetterEvent(pid, i, null));
            }

            await Task.Delay(TimeSpan.FromSeconds(2)); //2 sec is greater than the 1 sec ThrottleInterval trigger

            for (int i = 0; i < 1000; i++)
            {
                system.EventStream.Publish(new DeadLetterEvent(pid, i, null));
            }
        }
    }
}