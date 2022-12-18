/*
namespace SyncServLib.Logic.Repliers.VideoForwarder.Utils;

public class SafeCancellationTokenSource : IDisposable
{
	private readonly CancellationTokenSource cancelSource;

	private bool isDisposed;

	public void Dispose()
	{
		if (isDisposed) return;
		isDisposed = true;
		cancelSource.Dispose();
	}

	public SafeCancellationTokenSource() => cancelSource = new CancellationTokenSource();
	public SafeCancellationTokenSource(TimeSpan timeout) => cancelSource = new CancellationTokenSource(timeout);

	public void Cancel()
	{
		if (isDisposed) return;
		cancelSource.Cancel();
	}

	public CancellationToken Token
	{
		get
		{
			if (isDisposed) throw new ObjectDisposedException(nameof(SafeCancellationTokenSource));
			return cancelSource.Token;
		}
	}
}
*/