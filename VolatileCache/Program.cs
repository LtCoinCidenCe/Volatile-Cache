using System.Collections.Concurrent;

namespace VolatileCache;

class Program
{
  static ConcurrentDictionary<ulong, Cache<Data>> thecache = new();
  static void Main(string[] args)
  {
    List<Task> expiringList = new();
    ulong AIID = 0;
    Console.WriteLine("按下任意键添加键值对，按Q退出，按R归零自增ID");
    string? readkey = Console.ReadLine();
    while (readkey is not null && !readkey.StartsWith("Q", true, null))
    {
      if (readkey.StartsWith("S", true, null))
      {
        goto doReadAgain;
      }
      if (readkey.StartsWith("R", true, null))
      {
        AIID = 0;
        Console.WriteLine("自增ID已归零");
        goto doReadAgain;
      }
      for (int i = 0; i < 500; i++)
      {
        if (thecache.Count > 2000)
        {
          break;
        }

        Cache<Data> result = thecache.AddOrUpdate(AIID, addValueFactory: addValueFactory, updateValueFactory: updateValueFactory);

        AIID++;
      }
    doReadAgain:
      Console.WriteLine("自增ID是{0}，缓存数量为{1}", AIID, thecache.Count);
      readkey = Console.ReadLine();
    }
  }

  public static Cache<Data> addValueFactory(ulong nonID)
  {
    var newData = new Data() { ID = nonID, description = $"object{nonID}" };
    var cancelSource = new CancellationTokenSource();
    var cancelTask = Task.Factory.StartNew(removeByID, nonID, cancelSource.Token);
    return new Cache<Data>() { data = newData, cancel = cancelSource, removeTask = cancelTask };
  }

  public static Cache<Data> updateValueFactory(ulong existingID, Cache<Data> cachedObject)
  {
    cachedObject.cancel.Cancel();
    cachedObject.cancel.Dispose();

    var newData = new Data() { ID = existingID, description = $"object{existingID}" };
    var cancelSource = new CancellationTokenSource();
    var cancelTask = Task.Factory.StartNew(removeByID, existingID, cancelSource.Token);
    return new Cache<Data>() { data = newData, cancel = cancelSource, removeTask = cancelTask };
  }

  async static Task removeByID(object? givenID)
  {
#pragma warning disable CS8605 // Unboxing a possibly null value.
    ulong idToBeDeleted = (ulong)givenID;
#pragma warning restore CS8605 // Unboxing a possibly null value.

    await Task.Delay(TimeSpan.FromMinutes(0.25));
    if (thecache.TryRemove(idToBeDeleted, out var value))
    {
      value.cancel.Dispose();
    }
  }
}
