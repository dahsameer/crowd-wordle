namespace CrowdWordle.Test;

public class EncodingTest
{
    [Theory]
    [InlineData("hello", 15051911)]
    [InlineData("penis", 19149967)]
    public void PackStringToIntTest(string word, uint expectedNumber)
    {
        var packed = EncodingHelper.PackFromString(word);
        Assert.Equal(expectedNumber, packed);
    }

    [Theory]
    [InlineData(15051911, true)]
    [InlineData(92830593, false)]
    public void CheckPackedValidityTest(uint packed, bool isValid)
    {
        var result = EncodingHelper.IsValid(packed);
        Assert.Equal(isValid, result);
    }
}
