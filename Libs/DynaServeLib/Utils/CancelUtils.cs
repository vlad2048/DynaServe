using System.Reactive.Disposables;
using PowRxVar;

namespace DynaServeLib.Utils;

static class CancelUtils
{
	public static CancellationToken MakeFromDisp(IRoDispBase d)
	{
		var cancelSource = new CancellationTokenSource();
		var cancelToken = cancelSource.Token;
		Disposable.Create(() =>
		{
			cancelSource.Cancel();
			cancelSource.Dispose();
		}).D(d);
		return cancelToken;
	}
}