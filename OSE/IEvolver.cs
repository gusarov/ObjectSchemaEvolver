using System;

namespace ObjectSchemaEvolver
{
	public interface IEvolver<TRoot> : IEvolver
	{
	}

	public interface IEvolver
	{
		byte[] UpgradeDatabase(byte[] database, Func<byte[], dynamic> dynamer/*, Func<dynamic, byte[]> store*/, Action<byte[]> intermediateDump = null);
		decimal LatestVersion { get; }
	}
}