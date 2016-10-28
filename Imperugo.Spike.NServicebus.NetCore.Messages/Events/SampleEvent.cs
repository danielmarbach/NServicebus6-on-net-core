namespace Imperugo.Spike.NServicebus.NetCore.Messages.Events
{
	public class SampleEvent : EventBase
	{
		public string Prop1 { get; set; }
		public int Prop2 { get; set; }
	}
}