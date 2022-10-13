using System.Collections.Concurrent;
using System.Drawing;

namespace RcContentManager.Tests;

public class RefCounterTests
{
    private readonly Color _blue = Color.CornflowerBlue;
    private readonly Color _green = Color.LawnGreen;
    private readonly Color _red = Color.Crimson;

    [Test]
    public void RetainValueTypesShouldWork()
    {
        var refCounter           = new RefCounter<Color>();
        var incrementedCounter   = new Dictionary<Color, int>();
        var firstRetainedCounter = new Dictionary<Color, int>();
        refCounter.Incremented += (_, color) =>
            incrementedCounter[color] = incrementedCounter.GetValueOrDefault(color) + 1;
        refCounter.FirstTimeRetained += (_, color) =>
            firstRetainedCounter[color] = firstRetainedCounter.GetValueOrDefault(color) + 1;

        refCounter.Retain(_red);
        refCounter.Retain(_blue);
        refCounter.Retain(_blue);

        Assert.Multiple(() => {
            Assert.That(refCounter.ItemCount, Is.EqualTo(2));

            Assert.That(refCounter.Count(_red), Is.EqualTo(1));
            Assert.That(refCounter.Count(_blue), Is.EqualTo(2));
            Assert.That(refCounter.Count(_green), Is.Zero);

            Assert.That(refCounter.Tracked(_red), Is.True);
            Assert.That(refCounter.Tracked(_blue), Is.True);
            Assert.That(refCounter.Tracked(_green), Is.False);

            Assert.That(incrementedCounter, Has.Count.EqualTo(2));
            Assert.That(firstRetainedCounter, Has.Count.EqualTo(2));
            Assert.That(incrementedCounter.GetValueOrDefault(_red), Is.EqualTo(1));
            Assert.That(incrementedCounter.GetValueOrDefault(_blue), Is.EqualTo(2));
            Assert.That(firstRetainedCounter.GetValueOrDefault(_red), Is.EqualTo(1));
            Assert.That(firstRetainedCounter.GetValueOrDefault(_blue), Is.EqualTo(1));
        });
    }

    [Test]
    public void ReleaseValueTypesShouldWork()
    {
        var refCounter         = new RefCounter<Color>();
        var decrementedCounter = new Dictionary<Color, int>();
        var releasedCounter    = new Dictionary<Color, int>();
        refCounter.Decremented += (_, color) =>
            decrementedCounter[color] = decrementedCounter.GetValueOrDefault(color) + 1;
        refCounter.Released += (_, color) =>
            releasedCounter[color] = releasedCounter.GetValueOrDefault(color) + 1;
        refCounter.Retain(_red);
        refCounter.Retain(_blue);
        refCounter.Retain(_blue);

        refCounter.Release(_red);
        refCounter.Release(_blue);
        refCounter.Release(_blue);
        refCounter.Release(_green);

        Assert.Multiple(() => {
            Assert.That(refCounter.ItemCount, Is.Zero);

            Assert.That(refCounter.Count(_red), Is.Zero);
            Assert.That(refCounter.Count(_blue), Is.Zero);
            Assert.That(refCounter.Count(_green), Is.Zero);

            Assert.That(refCounter.Tracked(_red), Is.False);
            Assert.That(refCounter.Tracked(_blue), Is.False);
            Assert.That(refCounter.Tracked(_green), Is.False);

            Assert.That(decrementedCounter, Has.Count.EqualTo(2));
            Assert.That(releasedCounter, Has.Count.EqualTo(2));
            Assert.That(decrementedCounter.GetValueOrDefault(_red), Is.EqualTo(1));
            Assert.That(decrementedCounter.GetValueOrDefault(_blue), Is.EqualTo(2));
            Assert.That(releasedCounter.GetValueOrDefault(_red), Is.EqualTo(1));
            Assert.That(releasedCounter.GetValueOrDefault(_blue), Is.EqualTo(1));
        });
    }

    [Test]
    public void ClearValueTypesShouldWork()
    {
        var refCounter = new RefCounter<Color>();
        refCounter.Retain(_red);
        refCounter.Retain(_blue);
        refCounter.Retain(_blue);
        var decrementedCounter = new Dictionary<Color, int>();
        var releasedCounter    = new Dictionary<Color, int>();
        refCounter.Decremented += (_, color) =>
            decrementedCounter[color] = decrementedCounter.GetValueOrDefault(color) + 1;
        refCounter.Released += (_, color) =>
            releasedCounter[color] = releasedCounter.GetValueOrDefault(color) + 1;

        refCounter.Clear();

        Assert.Multiple(() => {
            Assert.That(refCounter.ItemCount, Is.Zero);

            Assert.That(refCounter.Count(_red), Is.Zero);
            Assert.That(refCounter.Count(_blue), Is.Zero);
            Assert.That(refCounter.Count(_green), Is.Zero);

            Assert.That(refCounter.Tracked(_red), Is.False);
            Assert.That(refCounter.Tracked(_blue), Is.False);
            Assert.That(refCounter.Tracked(_green), Is.False);

            Assert.That(decrementedCounter, Has.Count.EqualTo(0));
            Assert.That(releasedCounter, Has.Count.EqualTo(2));
            Assert.That(releasedCounter.GetValueOrDefault(_red), Is.EqualTo(1));
            Assert.That(releasedCounter.GetValueOrDefault(_blue), Is.EqualTo(1));
        });
    }

    [Test]
    public void ExcessiveReleaseValueTypesShouldWork()
    {
        var refCounter         = new RefCounter<Color>();
        var decrementedCounter = new Dictionary<Color, int>();
        var releasedCounter    = new Dictionary<Color, int>();
        refCounter.Decremented += (_, color) =>
            decrementedCounter[color] = decrementedCounter.GetValueOrDefault(color) + 1;
        refCounter.Released += (_, color) =>
            releasedCounter[color] = releasedCounter.GetValueOrDefault(color) + 1;
        int retained = 10;
        int released = 20;
        for (int i = 0; i < retained; i++)
            refCounter.Retain(_red);

        for (int i = 0; i < released; i++)
            refCounter.Release(_red);

        Assert.Multiple(() => {
            Assert.That(refCounter.ItemCount, Is.Zero);

            Assert.That(refCounter.Count(_red), Is.Zero);
            Assert.That(refCounter.Tracked(_red), Is.False);

            Assert.That(decrementedCounter.GetValueOrDefault(_red), Is.EqualTo(retained));
            Assert.That(releasedCounter.GetValueOrDefault(_red), Is.EqualTo(1));
        });
    }

    [Test]
    public void ExcessiveReleaseValueTypesShouldWorkWithMultithreading()
    {
        var refCounter         = new RefCounter<Color>();
        var decrementedCounter = new ConcurrentDictionary<Color, int>();
        var releasedCounter    = new ConcurrentDictionary<Color, int>();
        refCounter.Decremented += (_, color) =>
            decrementedCounter.AddOrUpdate(color, _ => 1, (_, curr) => curr + 1);
        refCounter.Released += (_, color) =>
            releasedCounter.AddOrUpdate(color, _ => 1, (_, curr) => curr + 1);
        int retained = 10;
        int released = 100;
        for (int i = 0; i < retained; i++)
            refCounter.Retain(_red);

        using (var countdownEvent = new CountdownEvent(released)) {
            for (int i = 0; i < released; i++)
                ThreadPool.QueueUserWorkItem(_ => {
                    refCounter.Release(_red);
                    countdownEvent.Signal();
                });

            countdownEvent.Wait();
        }

        Assert.Multiple(() => {
            Assert.That(refCounter.ItemCount, Is.Zero);

            Assert.That(refCounter.Count(_red), Is.Zero);
            Assert.That(refCounter.Tracked(_red), Is.False);

            Assert.That(decrementedCounter.GetValueOrDefault(_red), Is.EqualTo(retained));
            Assert.That(releasedCounter.GetValueOrDefault(_red), Is.EqualTo(1));
        });
    }

    [Test]
    public void RetainValueTypesShouldWorkWithMultithreading()
    {
        var refCounter               = new RefCounter<Color>();
        var firstTimeRetainedCounter = new ConcurrentDictionary<Color, int>();
        var incrementedCounter       = new ConcurrentDictionary<Color, int>();
        refCounter.Incremented += (_, color) =>
            incrementedCounter.AddOrUpdate(color, _ => 1, (_, curr) => curr + 1);
        refCounter.FirstTimeRetained += (_, color) =>
            firstTimeRetainedCounter.AddOrUpdate(color, _ => 1, (_, curr) => curr + 1);
        int retained = 100;

        using (var countdownEvent = new CountdownEvent(retained)) {
            for (int i = 0; i < retained; i++)
                ThreadPool.QueueUserWorkItem(_ => {
                    refCounter.Retain(_red);
                    countdownEvent.Signal();
                });

            countdownEvent.Wait();
        }

        Assert.Multiple(() => {
            Assert.That(refCounter.ItemCount, Is.EqualTo(1));

            Assert.That(refCounter.Count(_red), Is.EqualTo(retained));
            Assert.That(refCounter.Tracked(_red), Is.True);

            Assert.That(incrementedCounter.GetValueOrDefault(_red), Is.EqualTo(retained));
            Assert.That(firstTimeRetainedCounter.GetValueOrDefault(_red), Is.EqualTo(1));
        });
    }
}