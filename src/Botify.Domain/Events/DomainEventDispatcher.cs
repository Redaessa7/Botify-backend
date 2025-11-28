using System;
using System.Collections.Generic;

namespace Botify.Domain.Events
{
    public static class DomainEventDispatcher
    {
        private static readonly List<Action<IDomainEvent>> _handlers = new();

        // Register a handler (usually in application startup)
        public static void Register(Action<IDomainEvent> handler)
        {
            _handlers.Add(handler);
        }

        // Raise an event (called inside aggregates)
        public static void Raise(IDomainEvent domainEvent)
        {
            foreach (var handler in _handlers)
            {
                handler(domainEvent);
            }
        }
    }
}
