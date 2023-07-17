using System.Threading.Tasks;

namespace ChatCore.Interfaces
{
	public interface IOpenBLiveProvider
	{
		void Start(bool reconnect = false);
		void Stop(bool force = false);

		IOpenBLiveProvider GetBLiveService();
	}
}
