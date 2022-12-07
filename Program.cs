using System.Diagnostics;

namespace AsyncExample2
{
    internal class Program
    {
        static readonly List<string> playList = new()
        {
            "Fox", "Sugar", "Squirrel", "Elephant", "Rabbit"
        };

        static readonly Dictionary<string, string> lyrics = new()
        {
            { "Fox", "The quick brown fox" },
            { "Sugar", "The sweetest thing is sugar" },
            { "Squirrel", "Squirrels are brown or red" },
            { "Elephant", "Elephants are grey and have tusks" },
            { "Rabbit", "Rabbits are small with floppy ears" }
        };

        static async Task<string> TryFindLyrics(string songName)
        {
            return await Task.Run(() => {
                Console.WriteLine($"Retrieving lyrics for {songName}");
                Thread.Sleep(3000);
                return Program.lyrics[songName];
            });
        }

        static async Task<string?> TryFindSongName(string matchString)
        {
            foreach (string songName in playList)
            {
                var lyrics = await TryFindLyrics(songName);
                if (lyrics.Contains(matchString))
                {
                    Console.WriteLine($"Found song {songName} containing {matchString}");
                    return songName;
                }
            }

            Console.WriteLine($"No song contains {matchString}");
            return null;
        }

        static void FindSong(string matchString)
        {
            Console.WriteLine($"Creating task to search for song containing {matchString} ... ");
            var findTask = Program.TryFindSongName(matchString);
            Console.WriteLine($"About to await find task ... ");
            var songName = findTask.GetAwaiter().GetResult();
            Console.WriteLine($"Find task has returned a result!");

            if (songName == null)
            {
                Console.WriteLine($"No song found containing {matchString}!");
            }
            else
            {
                Console.WriteLine($"Found song name {songName}");
            }
        }

        static async Task MyWaitTask(string taskName, int milliseconds)
        {
            Console.WriteLine($"Task: {taskName} delaying for {milliseconds} milliseconds at {sw.ElapsedMilliseconds}");
            Thread.Sleep(1000);
            await Task.Delay(milliseconds).ContinueWith(
                t =>
                {
                    Console.WriteLine($"Task: {taskName} finished {milliseconds} millisecond delay at {sw.ElapsedMilliseconds}");
                });
        }

        static Task CreateWaitTask(string taskName, int milliseconds)
        {
            Console.WriteLine($"StartWaitTask: creating {taskName} to sleep for {milliseconds} milliseconds at {sw.ElapsedMilliseconds}");
            return Task.Run(() => MyWaitTask(taskName, milliseconds));
        }

        static async Task MySegmentedWaitTask()
        {
            const int delay = 10000;
            List<Task> someTasks = new();

            Console.WriteLine($"MySegmentedWaitTask starting first wait at {sw.ElapsedMilliseconds}");
            someTasks.Add(Task.Delay(delay));

            Console.WriteLine($"MySegmentedWaitTask starting second wait at {sw.ElapsedMilliseconds}");
            someTasks.Add(Task.Delay(delay));

            Console.WriteLine($"MySegmentedWaitTask starting third wait at {sw.ElapsedMilliseconds}");
            someTasks.Add(Task.Delay(delay));

            Console.WriteLine($"MySegmentedWaitTask starting fourth wait at {sw.ElapsedMilliseconds}");
            someTasks.Add(Task.Delay(delay));

            Console.WriteLine($"MySegmentedWaitTask waiting for tasks to complete at {sw.ElapsedMilliseconds}");
            await Task.WhenAll(someTasks.ToArray());

            Console.WriteLine($"MySegmentedWaitTask all tasks complete at {sw.ElapsedMilliseconds}");
        }

        static async Task MyNestedWaitTask2()
        {
            Console.WriteLine($"MyNestedWaitTask2 starting nested task at {sw.ElapsedMilliseconds}");

#if false
            for (int i = 0; i < int.MaxnValue; ++i)
            {
                Console.WriteLine($"MyNestedWaitTask2 looping at {sw.ElapsedMilliseconds}");
                Thread.Sleep(1000);
            }
#endif

            await Task.Delay(int.MaxValue);
        }

        static async Task MyNestedWaitTask1()
        {
            Console.WriteLine($"MyNestedWaitTask1 starting nested task at {sw.ElapsedMilliseconds}");
            await MyNestedWaitTask2();
        }

        static async Task MyNestedWaitTask()
        {
            Console.WriteLine($"MyNestedWaitTask starting nested task at {sw.ElapsedMilliseconds}");
            await MyNestedWaitTask1();
        }

        static readonly Stopwatch sw = Stopwatch.StartNew();

        static void SecondFunction()
        {
            Thread.Sleep(int.MaxValue);
        }

        static void FirstFunction()
        {
            SecondFunction();
        }
        static void TopLevelThreadFunction()
        {
            int myThreadId = Thread.CurrentThread.ManagedThreadId;
            int myProcessorId = Thread.GetCurrentProcessorId();
            Thread.CurrentThread.Name = "TopLevelThreadFunction " + myThreadId.ToString();
            FirstFunction();
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine($"main: Invoking Task.Delay at {sw.ElapsedMilliseconds}");
            Task.Delay(1000).GetAwaiter().GetResult();
            Console.WriteLine($"main: one second Task.Delay completed at {sw.ElapsedMilliseconds}");

            switch (args[0])
            {
                case "a":
                    FindSong("tusks");
                    break;

                case "b":
                    List<Task> myTasks = new();
                    myTasks.Add(CreateWaitTask("A", 3000));
                    myTasks.Add(CreateWaitTask("B", 4000));
                    myTasks.Add(CreateWaitTask("C", 5000));
                    Thread.Sleep(1000);
                    Task.WhenAll(myTasks).GetAwaiter().GetResult();
                    Console.WriteLine($"All tasks complete at {sw.ElapsedMilliseconds}");
                    break;

                case "c":
                    List<Thread> myThreads = new();
                    for (int i = 0; i < 10; ++i)
                    {
                        myThreads.Add(new Thread(TopLevelThreadFunction));
                        myThreads[myThreads.Count - 1].Start();
                    }

                    myThreads[0].Join();

                    break;

                case "d":
                    Console.WriteLine($"main: starting MySegmentedWaitTask at {sw.ElapsedMilliseconds}");
                    var mytask = MySegmentedWaitTask();
                    Console.WriteLine($"main: MySegmentedWaitTask has been started at {sw.ElapsedMilliseconds}");
                    await mytask;
                    Console.WriteLine($"main: MySegmentedWaitTask completed at {sw.ElapsedMilliseconds}");
                    break;

                case "e":
                    Console.WriteLine($"main: starting MyNestedWaitTask at {sw.ElapsedMilliseconds}");
                    await MyNestedWaitTask();
                    break;
            }
        }
    }
}