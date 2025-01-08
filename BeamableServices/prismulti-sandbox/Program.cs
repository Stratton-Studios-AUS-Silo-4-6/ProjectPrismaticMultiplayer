using Beamable.Server;
using System.Threading.Tasks;

namespace Beamable.prismulti_sandbox
{
	public class Program
	{
		/// <summary>
		/// The entry point for the <see cref="prismulti_sandbox"/> service.
		/// </summary>
		public static async Task Main()
		{
			// inject data from the CLI.
			await MicroserviceBootstrapper.Prepare<prismulti_sandbox>();
			
			// run the Microservice code
			await MicroserviceBootstrapper.Start<prismulti_sandbox>();
		}
	}
}
