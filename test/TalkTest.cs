using Xunit.Abstractions;

namespace test;

public class TalkTest
{
	private readonly ITestOutputHelper output;

	public TalkTest(ITestOutputHelper output)
	{
		this.output = output;
	}

	[Fact]
	public void Simple()
	{
		output.WriteLine("test ok!");
	}
}