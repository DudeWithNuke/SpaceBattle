using System;
using System.Collections.Generic;

namespace PlaceableObject.Events
{
    public class ShipEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
                return;

            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }

            list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
                return;

            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
                return;

            list.Remove(handler);

            if (list.Count == 0)
                _handlers.Remove(type);
        }

        public void Publish<T>(T evt) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
                return;

            var snapshot = new List<Delegate>(list);
            foreach (var handler in snapshot)
                ((Action<T>)handler)(evt);
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
