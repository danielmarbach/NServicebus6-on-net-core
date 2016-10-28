using System;
using System.Threading.Tasks;
using Imperugo.Spike.NServicebus.NetCore.Messages.Events;
using NServiceBus;

namespace Imperugo.Spike.NServicebus.NetCore.Handlers.EventHandlers
{
	public class SampleEventHandler : IHandleMessages<SampleEvent>
	{
		public Task Handle(SampleEvent message, IMessageHandlerContext context)
		{
			Console.WriteLine($"Event received: Prop1 {message.Prop1} Prop2 {message.Prop2}");

			return Task.FromResult(0);
		}
	}
}