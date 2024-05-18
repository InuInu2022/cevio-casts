using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GithubReleaseDownloader;
using GithubReleaseDownloader.Entities;
using Mayerch1.GithubUpdateCheck;

namespace CevioCasts.UpdateChecker;

/// <summary>
/// Github Releaseのcast-dataのバージョンをチェック、ダウンロードなどを行うクラス。
/// </summary>
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

	/// <summary>
	/// Github releaseからのチェックを行うクラスのFactoryメソッド
	/// </summary>
	/// <param name="ceviocastsJsonPath">チェック対象のローカルに保存したcastデータjsonへのパス</param>
	/// <param name="username">別リポジトリ対応</param>
	/// <param name="repository">別リポジトリ対応</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
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

	/// <summary>
	/// ローカルに保存されたcast-dataのバージョン情報を取得します。
	/// </summary>
	/// <remarks>
	/// 既にバージョン情報が設定されている場合は、その情報を返します。
	/// バージョン情報が設定されていない場合は、定義ファイルからバージョン情報を読み込み、
	/// 新規のバージョン情報として設定します。バージョン情報の読み込みに失敗した場合は、
	/// デフォルトのバージョン（0.0.0）を使用します。
	/// </remarks>
	/// <returns>ローカルに保存されたバージョン情報を表すVersionオブジェクト。</returns>
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

	/// <summary>
	/// リポジトリから最新のcast-dataのバージョン情報を非同期で取得します。
	/// </summary>
	/// <remarks>
	/// このメソッドは、キャッシュが無効な場合、リポジトリから新しいバージョン情報を取得し、それをローカルに保存します。
	/// 取得したバージョン情報は、次回以降の呼び出し時にキャッシュされます。
	/// </remarks>
	/// <param name="useCache">有効にすると、既に取得済みのバージョン情報がある場合は、それを返します。</param>
	/// <returns>リポジトリの最新バージョン情報を表すVersionオブジェクト。</returns>
	public async ValueTask<Version>
	GetRepositoryVersionAsync(
		bool useCache = false
	)
	{
		if(useCache && repoVersion is not null)
		{
			return repoVersion;
		}

		var v = await update.VersionAsync()
			.ConfigureAwait(false);
		repoVersion = new(v);
		return repoVersion;
	}

	/// <summary>
	/// ローカルバージョンがリポジトリの最新バージョンと比較して
	/// アップデートが利用可能かどうかを非同期で確認します。
	/// </summary>
	/// <returns>アップデートが利用可能の場合はtrue、利用不可の場合はfalse。</returns>
	public Task<bool>
	IsAvailableAsync() =>
		update.IsUpdateAvailableAsync(
			GetLocalVersion().ToString()
		);

	/// <summary>
	/// リポジトリからcast-dataファイルをダウンロードします。
	/// </summary>
	/// <param name="destPath">ファイルを保存するディレクトリのパス。</param>
	/// <param name="fileName">ダウンロードするファイル名。デフォルトは "data.json"。</param>
	/// <param name="isSkipCheck">チェックをスキップするかどうか。デフォルトは <see langword="false"/>。</param>
	/// <param name="percent">ダウンロード進捗を報告するための <see cref="IProgress{T}"/> インターフェイス。</param>
	/// <param name="cancellationToken">この操作をキャンセルするための <see cref="CancellationToken"/>。</param>
	/// <returns>非同期タスク。</returns>
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
