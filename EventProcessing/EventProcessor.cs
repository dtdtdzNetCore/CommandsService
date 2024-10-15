using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }
        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch (eventType)
            {
                case EventType.PlatformPublished:
                    addPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            System.Console.WriteLine("--> Determining Event");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
            
            switch(eventType.Event)
            {
                case "Platform_Published":
                    System.Console.WriteLine("--> Platfrom Published Event Detected");
                    return EventType.PlatformPublished;
                default:
                System.Console.WriteLine("--> Could noit determine the event type");
                return EventType.Undeterminded;
            }
        }

        private void addPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope. ServiceProvider.GetRequiredService<ICommandRepo>();

                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try
                {
                    var plat = _mapper.Map<Platform>(platformPublishedDto);
                    if (!repo.ExteramlPlatformExist(plat.ExternalId))
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChange();
                        System.Console.WriteLine("--> Platform added!");
                    }
                    else
                    {
                        System.Console.WriteLine("--> Platform already exisits...");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"--> Could not add platform to DB {ex.Message}");
                }
            }
        }
    }
    
    enum EventType
    {
        PlatformPublished,
        Undeterminded
    }
}