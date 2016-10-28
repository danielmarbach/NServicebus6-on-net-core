using System;
using System.Threading.Tasks;
using Imperugo.Spike.NServicebus.NetCore.Messages.Commands;
using Imperugo.Spike.NServicebus.NetCore.Messages.Events;
using NServiceBus;

namespace Imperugo.Spike.NServicebus.NetCore.Handlers.CommandHadlers
{
	public class SampleCommandHandler : IHandleMessages<SampleCommand>
	{
		public Task Handle(SampleCommand message, IMessageHandlerContext context)
		{
			Console.WriteLine($"Command received: Prop1 {message.Prop1} Prop2 {message.Prop2}");

			context.Publish(new SampleEvent
			{
				Prop2 = message.Prop2,
				Prop1 = message.Prop1
			});

			return Task.FromResult(0);
		}
	}
}