using CevioCasts;
using CevioCasts.UpdateChecker;
using FluentAssertions;
using GithubReleaseDownloader;
using Mayerch1.GithubUpdateCheck;
using Xunit.Abstractions;

namespace UpdateCheck;

public class UpdateCheck
{
	private readonly ITestOutputHelper output;
	private readonly string jsonString;

	public UpdateCheck(ITestOutputHelper output)
	{
		this.output = output;
		var path = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"../../../file/data.1.0.0.json"
		);
		jsonString = File.ReadAllText(path);
	}

	[Fact]
	public async void CheckAvailable()
	{
		var def = Definitions.FromJson(jsonString);
		var currentVer = new Version(def.Version);

		var update = new GithubUpdateCheck("InuInu2022", "cevio-casts");
		var result = await update.IsUpdateAvailableAsync(def.Version);
		result.Should().BeTrue();
		output.WriteLine($"github release version: {await update.VersionAsync()}");
	}

	[Fact]
	public async void CheckAvailableLib()
	{
		var gr = await GithubRelease
			.BuildAsync("../../../file/data.1.0.0.json");
		var result = await gr.IsAvailableAsync();
		result.Should().BeTrue();

		var local = gr.GetLocalVersion();

		local.Should()
			.Be(new Version(1,0,0));
		var repo = await gr.GetRepositoryVersionAsync();

		output.WriteLine($"local:{local}, latest:{repo}");
	}

	[Fact]
	public async void CheckAndDownloadLib()
	{
		var gr = await GithubRelease
			.BuildAsync("../../../file/data.1.0.0.json");
		await gr.DownloadAsync(
			destPath: "../../../dest/"
		);
	}

	[Fact]
	public async void CheckAndDownload()
	{
		//check
		var def = Definitions.FromJson(jsonString);
		var currentVer = new Version(def.Version);

		var update = new GithubUpdateCheck("InuInu2022", "cevio-casts");
		var result = await update.IsUpdateAvailableAsync(def.Version);
		result.Should().BeTrue();
		if(!result)return;

		//download
		var release = await ReleaseManager.Instance
			.GetLatestAsync("InuInu2022", "cevio-casts");
		release.Should().NotBeNull();
		if(release is null)return;
		await AssetDownloader.Instance
			.DownloadAssetAsync(
				release.Assets.First(a => a.Name == "data.json"),
				Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory,
					"../../../dest/"
				),
				"data.json",
				(info) =>
				{
					output.WriteLine($"{info.Name} DL:[ {info.DownloadPercent:P} ]");
					if(info.DownloadPercent == 1.0)
					{
						output.WriteLine($"Finish: {info.Name}");
					}
				}
			);
	}
}