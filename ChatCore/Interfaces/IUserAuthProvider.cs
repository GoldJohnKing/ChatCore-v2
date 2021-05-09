using System;
using ChatCore.Models;

namespace ChatCore.Interfaces
{
	public interface IUserAuthProvider
	{
		event Action<LoginCredentials> OnCredentialsUpdated;
		LoginCredentials Credentials { get; }
		void Save(bool callback = true);
	}
}
