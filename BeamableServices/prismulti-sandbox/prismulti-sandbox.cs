using Beamable.Server;

namespace Beamable.prismulti_sandbox
{
	[Microservice("prismulti_sandbox")]
	public partial class prismulti_sandbox : Microservice
	{
		[ClientCallable]
		public int Add(int a, int b)
		{
			return a + b;
		}
	}
}
