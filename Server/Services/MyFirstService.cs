using MagicOnion;
using MagicOnion.Server;
using Shared.Interfaces.Services;

public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    public async UnaryResult<int> SumAsync(int x, int y)
    {
        Console.WriteLine("Received(SumAsync):" + x + "," + y);
        return x + y;
    }

    public async UnaryResult<int> SubAsync(int x, int y)
    {
        Console.WriteLine("Received(SubAsync):" + x + "," + y);
        return x - y;
    }

    public async UnaryResult<int> SumAllAsync(int[] numList)
    {
        int result = 0;
        foreach (var num in numList)
        {
            result += num;
        }
        Console.WriteLine("Received(SumAllAsync):" + result);
        return result;
    }

    public async UnaryResult<int[]> CalcForOperationAsync(int x, int y)
    {
        int[] result = new int[4] {
            x + y,
            x - y,
            x * y,
            x / y,
        };
        Console.WriteLine("Received(CalcForOperationAsync):" + result.ToString());
        return result;
    }

    public async UnaryResult<float> SumAllNumberAsync(IMyFirstService.Number number)
    {
        Console.WriteLine("Received(SumAsync):" + number.x + "," + number.y);
        return number.x + number.y;
    }
}
