using Xunit.Abstractions;

namespace test2;

public class UnitTest1
{
    private readonly ITestOutputHelper output;

    public UnitTest1(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Test1()
    {
		output.WriteLine("OK!");
	}
}