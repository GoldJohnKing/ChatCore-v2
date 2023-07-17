namespace ChatCore.Interfaces
{
	public interface IWebLoginProvider
	{
		void Start(bool bilibiliOnly = false);
		void Stop();
	}
}
