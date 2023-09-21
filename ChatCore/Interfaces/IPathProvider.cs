namespace ChatCore.Interfaces
{
	public interface IPathProvider
	{
		string GetDataPath();
		string GetImagePath();
		string GetAvatarImagePath();
		string GetBadgesImagePath();
		string GetResourcePath();
	}
}
