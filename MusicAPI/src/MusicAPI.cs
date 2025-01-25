using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Net;

namespace MusicAPI;

public enum Platform
{
	Netease,
	Tencent
}
public static class MusicAPI
{
	public static IMusicAPI GetAPI(Platform platform)
	{
		return platform switch
		{
			Platform.Netease => new NeteaseAPI(),
			// Platform.Tencent => new TencentAPI(),
			_ => throw new ArgumentException("Invalid platform")
		};
	}
}
