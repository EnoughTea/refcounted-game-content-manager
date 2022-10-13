using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace RcContentManager.Tests;

public class ContentManagerTests
{
    [Test]
    public void MultipleLoadsShouldOnlyLoadOnce()
    {
        var cm    = new RefCountedContentManager(new ServiceProvider());
        var asset = Substitute.For<IAsset>();
        asset.Name.Returns("NoOpAsset");

        cm.Load(asset);
        cm.Load(asset);
        cm.Load(asset);

        asset.Received(Quantity.Exactly(1)).Load(Arg.Any<IServiceProvider>());
    }

    [Test]
    public void MultipleLoadsShouldOnlyLoadOnceMultiThreaded()
    {
        var cm    = new RefCountedContentManager(new ServiceProvider());
        var asset = Substitute.For<IAsset>();
        asset.Name.Returns("NoOpAsset");

        int loaded = 100;
        using (var countdownEvent = new CountdownEvent(loaded)) {
            for (int i = 0; i < loaded; i++)
                ThreadPool.QueueUserWorkItem(_ => {
                    cm.Load(asset);
                    countdownEvent.Signal();
                });

            countdownEvent.Wait();
        }

        asset.Received(Quantity.Exactly(1)).Load(Arg.Any<IServiceProvider>());
    }

    [Test]
    public void MultipleUnloadsShouldOnlyUnloadOnce()
    {
        var cm    = new RefCountedContentManager(new ServiceProvider());
        var asset = Substitute.For<IAsset>();
        asset.Name.Returns("NoOpAsset");
        cm.Load(asset);

        cm.Unload(asset);
        cm.Unload(asset);
        cm.Unload(asset);

        asset.Received(Quantity.Exactly(1)).Unload(Arg.Any<IServiceProvider>());
    }

    [Test]
    public void MultipleUnloadsShouldOnlyUnloadOnceMultiThreaded()
    {
        var cm    = new RefCountedContentManager(new ServiceProvider());
        var asset = Substitute.For<IAsset>();
        asset.Name.Returns("NoOpAsset");
        cm.Load(asset);

        int unloaded = 100;
        using (var countdownEvent = new CountdownEvent(unloaded)) {
            for (int i = 0; i < unloaded; i++)
                ThreadPool.QueueUserWorkItem(_ => {
                    cm.Unload(asset);
                    countdownEvent.Signal();
                });

            countdownEvent.Wait();
        }

        asset.Received(Quantity.Exactly(1)).Unload(Arg.Any<IServiceProvider>());
    }

    [Test]
    public void ExceptionInLoadShouldNotCorruptState()
    {
        var cm    = new RefCountedContentManager(new ServiceProvider());
        var asset = Substitute.For<IAsset>();
        asset.Name.Returns("NoOpAsset");
        asset.When(_ => _.Load(Arg.Any<IServiceProvider>())).Do(_ => throw new NotSupportedException());

        try {
            cm.Load(asset);
        }
        catch (NotSupportedException) {
        }

        cm.Unload(asset);

        asset.Received(Quantity.None()).Unload(Arg.Any<IServiceProvider>());
        Assert.That(cm.Find<IAsset>(asset.Name), Is.Null);
    }

    [Test]
    public void ExceptionInUnloadShouldNotCorruptState()
    {
        var cm    = new RefCountedContentManager(new ServiceProvider());
        var asset = Substitute.For<IAsset>();
        asset.Name.Returns("NoOpAsset");
        asset.When(_ => _.Unload(Arg.Any<IServiceProvider>())).Do(_ => throw new NotSupportedException());
        cm.Load(asset);

        try {
            cm.Unload(asset);
        }
        catch (NotSupportedException) {
        }

        Assert.That(cm.Find<IAsset>(asset.Name), Is.Not.Null);
    }
}