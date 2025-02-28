namespace Template.Domain.Test;

public class UnitTest1
{
    [
        Theory,
        //InlineData(61),
        //InlineData(67),
        //InlineData(71),
        //InlineData(73),
        //InlineData(79),
        //InlineData(83),
        //InlineData(89),
        //InlineData(91), // Failure
        //InlineData(97),
        //InlineData(103),
        //InlineData(107),
        //InlineData(109),
        //InlineData(113),
        //InlineData(117), // Failure
        InlineData(1029001476404750269),
    ]
    public static void Test1(decimal input)
    {
        var prime = IsPrime(input);
        Assert.True(prime);
    }
    private static bool IsPrime(decimal input)
    {
        if (input % 2 == 0)
            return false;

        decimal squrt = Sqrt(input);
        decimal root = Math.Floor(squrt);
        for (ulong i = 3; i <= root; i += 2)
        {
            if (input % i == 0)
                return false;
        }
        return true;
    }
    private static decimal Sqrt(decimal x, decimal epsilon = 0.0M)
    {
        if (x < 0) throw new OverflowException("Cannot calculate square root from a negative number");

        decimal current = (decimal)Math.Sqrt((double)x), previous;
        do
        {
            previous = current;
            if (previous == 0.0M) return 0;
            current = (previous + x / previous) / 2;
        }
        while (Math.Abs(previous - current) > epsilon);
        return current;
    }
}
