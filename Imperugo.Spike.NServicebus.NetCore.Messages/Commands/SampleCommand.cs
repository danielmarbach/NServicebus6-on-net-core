namespace Imperugo.Spike.NServicebus.NetCore.Messages.Commands
{
	public class SampleCommand : CommandBase
	{
		public string Prop1 { get; set; }
		public int Prop2 { get; set; }
	}
}