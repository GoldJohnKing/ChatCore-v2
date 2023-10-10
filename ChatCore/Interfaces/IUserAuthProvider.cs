using System;
using ChatCore.Models;

namespace ChatCore.Interfaces
{
	public interface IUserAuthProvider
	{
		event Action<LoginCredentials> OnTwitchCredentialsUpdated, OnBilibiliCredentialsUpdated;
		LoginCredentials Credentials { get; }
		void SaveTwitch(bool callback = true);
		void SaveBilibili(bool callback = true);
		void Reload();
	}
}
