using System;
using System.Threading.Tasks;
using OpenBLive.Client.Data;

namespace ChatCore.Interfaces
{
	public interface IOpenBLiveProvider
	{
		void Start(bool reconnect = false);
		void Stop(bool force = false);

		public event Action onWssUpdate;

		IOpenBLiveProvider GetBLiveService();

		public string getWssLink();

		public string getAuthBody();
	}
}
