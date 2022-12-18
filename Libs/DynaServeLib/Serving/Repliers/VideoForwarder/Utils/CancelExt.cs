using PowRxVar;

namespace DynaServeLib.Serving.Repliers.VideoForwarder.Utils;

static class CancelExt
{
	public static (CancellationToken, IDisposable) WithTimeout(this CancellationToken token, TimeSpan timeout)
	{
		var d = new Disp();
		var timeoutSource = new CancellationTokenSource(timeout).D(d);
		var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, token).D(d);
		return (linkedSource.Token, d);
	}
}