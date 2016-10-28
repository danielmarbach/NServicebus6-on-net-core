using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Imperugo.Spike.NServicebus.NetCore.Messages;
using Imperugo.Spike.NServicebus.NetCore.Messages.Commands;
using NServiceBus;

namespace Imperugo.Spike.NServicebus.NetCore
{
	public class Program
	{
		public static void Main(string[] args)
		{
			MainAsync().GetAwaiter().GetResult();
		}

		static async Task MainAsync()
		{
			var endpointConfiguration = new EndpointConfiguration("imperugo.dev.distributor");
			endpointConfiguration.SendFailedMessagesTo("imperugo.dev.distributor.errors");
			var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();

			var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");

			if (string.IsNullOrWhiteSpace(connectionString))
			{
				throw new Exception("Could not read the 'AzureServiceBus.ConnectionString' environment variable. Check the sample prerequisites.");
			}

			transport.ConnectionString(connectionString);
			transport.UseTopology<ForwardingTopology>();

			endpointConfiguration.UsePersistence<InMemoryPersistence>();
			endpointConfiguration.UseSerialization<JsonSerializer>();
			endpointConfiguration.EnableInstallers();

			ApplyConventions(endpointConfiguration);

			var sanitization = transport.Sanitization();
			sanitization.UseStrategy<Sha1Sanitization>();

			var endpointInstance = await Endpoint.Start(endpointConfiguration)
				.ConfigureAwait(false);

			await endpointInstance.Send(new SampleCommand()
			{
				Prop1 = "value1",
				Prop2 = 2
			});

			Console.WriteLine("Message Sent");

		}

		static void ApplyConventions(EndpointConfiguration endpointConfiguration)
		{
			var conventions = endpointConfiguration.Conventions();
			conventions.DefiningCommandsAs(IsCommand);
			conventions.DefiningEventsAs(IsEvent);
			conventions.DefiningMessagesAs(IsMessage);
			conventions.DefiningEncryptedPropertiesAs(IsEncrypted);
			conventions.DefiningExpressMessagesAs(IsExpressMessage);
			conventions.DefiningTimeToBeReceivedAs(GetTimeToBeReceived);
			conventions.DefiningDataBusPropertiesAs(IsDataBusProperty);
		}

		private static bool IsCommand(Type t)
		{
			return t.Namespace != null && t.IsSubclassOf(typeof(CommandBase));
		}

		private static bool IsEvent(Type t)
		{
			return t.Namespace != null && t.IsSubclassOf(typeof(EventBase));
		}

		private static bool IsMessage(Type t)
		{
			return t.Namespace != null && t.IsSubclassOf(typeof(MessageBase));
		}

		private static bool IsExpressMessage(Type t)
		{
			if (t.Name.EndsWith("Express"))
			{
				return true;
			}

			if (t.GetCustomAttributes(true).Any(att => att.GetType().Name == "ExpressAttribute"))
			{
				return true;
			}

			return false;
		}

		private static bool IsEncrypted(PropertyInfo pi)
		{
			if (pi.Name == "Encrypted" && pi.PropertyType == typeof(string))
			{
				return true;
			}

			if (pi.GetCustomAttributes(true).Any(att => att.GetType().Name == "EncryptAttribute"))
			{
				return true;
			}

			return false;
		}

		private static bool IsDataBusProperty(PropertyInfo pi)
		{
			if (pi.Name == "Data" && pi.PropertyType == typeof(byte[]))
			{
				return true;
			}

			if (pi.Name == "Data" && pi.PropertyType == typeof(byte[][]))
			{
				return true;
			}

			if (pi.Name == "Data" && pi.PropertyType == typeof(List<byte[]>))
			{
				return true;
			}

			if (pi.GetCustomAttributes(true).Any(att => att.GetType().Name == "DataBusAttribute"))
			{
				return true;
			}

			return false;
		}

		private static TimeSpan GetTimeToBeReceived(Type t)
		{
			var attribute = t.GetCustomAttributes(true).FirstOrDefault(att => att.GetType().Name == "TimeToBeReceivedAttribute");

			if (attribute != null)
			{
				var property = attribute.GetType().GetProperty("TimeToBeReceived");
				if (property != null && property.PropertyType == typeof(TimeSpan))
				{
					return (TimeSpan)property.GetValue(attribute, null);
				}
			}

			return TimeSpan.MaxValue;
		}
	}
}
