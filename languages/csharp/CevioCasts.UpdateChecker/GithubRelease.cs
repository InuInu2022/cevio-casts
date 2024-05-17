using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GithubReleaseDownloader;
using GithubReleaseDownloader.Entities;
using Mayerch1.GithubUpdateCheck;

namespace CevioCasts.UpdateChecker;

public sealed class GithubRelease
{
	private string Json { get; init; }
	public string UserName { get; }
	public string Repository { get; }
	private Definitions Definitions { get; init; }

	private readonly GithubUpdateCheck update;
	private Version? localVersion;
	private Version? repoVersion;
	private static string? jsonPath;

	private GithubRelease(
		string json,
		string username,
		string repository
	) {
		update = new GithubUpdateCheck(username, repository);
		Definitions = Definitions.FromJson(json);
		Json = json;
		this.UserName = username;
		this.Repository = repository;
	}

	public static async ValueTask<GithubRelease> BuildAsync(
		string ceviocastsJsonPath,
		string username = "InuInu2022",
		string repository = "cevio-casts",
		CancellationToken cancellationToken = default
	)
	{
		jsonPath = ceviocastsJsonPath;
		var path = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			ceviocastsJsonPath);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
		var json = await File.ReadAllTextAsync(path, cancellationToken)
			.ConfigureAwait(false);

#else
		var json = await Task.Run(
			() => File.ReadAllText(path)
		).ConfigureAwait(false);
#endif
		return new GithubRelease(json, username, repository);
	}

	public Version GetLocalVersion()
	{
		if(localVersion is not null)
		{
			return localVersion;
		}

		localVersion = (Version
			.TryParse(Definitions?.Version, out var v)
				? v : new(0, 0, 0));
		return localVersion;
	}

	public async ValueTask<Version>
	GetRepositoryVersionAsync()
	{
		if(repoVersion is not null)
		{
			return repoVersion;
		}

		var v = await update.VersionAsync()
			.ConfigureAwait(false);
		repoVersion = new(v);
		return repoVersion;
	}

	public Task<bool>
	IsAvailableAsync() =>
		update.IsUpdateAvailableAsync(
			GetLocalVersion().ToString()
		);

	[SuppressMessage("","CA1822")]
	public async ValueTask DownloadAsync(
		string? destPath = null,
		string fileName = "data.json",
		bool isSkipCheck = false,
		IProgress<double>? percent = default,
		CancellationToken cancellationToken = default
	)
	{
		if(!isSkipCheck)
		{
			var result = await IsAvailableAsync()
				.ConfigureAwait(false);
			if(!result)
			{
				throw new OperationCanceledException("Download canceled: no new version data exists.",cancellationToken);
				//return;
			}
		}

		var release = await ReleaseManager.Instance
			.GetLatestAsync(UserName, Repository)
			.ConfigureAwait(false);
		cancellationToken.ThrowIfCancellationRequested();

		if (release is null) { return; }

		var dest = Path.GetDirectoryName(Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			destPath ?? jsonPath ?? $"data/{fileName}"
		));
		Debug.WriteLine($"dest: {dest}");

		try
		{
			await AssetDownloader.Instance
				.DownloadAssetAsync(
					release.Assets.First(a => string.Equals(a.Name, fileName, StringComparison.Ordinal)),
					dest!,
					fileName,
					info => ProgressChanged(info, percent,cancellationToken)
				)
				.ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (FileNotFoundException e)
		{
			Debug.WriteLine(e.Message);
			throw;
		}
	}

	private static void ProgressChanged(
		DownloadInfo info,
		IProgress<double>? percent,
		CancellationToken ctx = default)
	{
		ctx.ThrowIfCancellationRequested();
		percent?.Report(info.DownloadPercent);
	}
}
