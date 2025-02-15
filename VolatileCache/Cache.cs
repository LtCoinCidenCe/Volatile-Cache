namespace VolatileCache;

public class Cache<T>
{
  public required T data;
  public required CancellationTokenSource cancel;
  public required Task removeTask;
}
