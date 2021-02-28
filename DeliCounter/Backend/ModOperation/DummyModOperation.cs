using System;
using System.Threading.Tasks;

namespace DeliCounter.Backend.ModOperation
{
    public class DummyModOperation : ModOperation
    {
        private readonly Func<Task> _action;
        
        public DummyModOperation(Func<Task> action) : base(null, null)
        {
            _action = action;
        }

        internal override async Task Run()
        {
            await _action();
        }
    }
}