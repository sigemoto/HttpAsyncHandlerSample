using System;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Threading.Tasks;

namespace AsyncHttpHandlerSample
{
	public class AsyncHandler : IHttpAsyncHandler
	{
		/// <summary>
		/// Web を使用するには、Web の web.config ファイルでこの
		/// ハンドラーを設定し、IIS に登録する必要があります。詳細については、 
		/// 次のリンクを参照してください: http://go.microsoft.com/?linkid=8101007
		/// </summary>
		#region IHttpHandler Members

		public bool IsReusable
		{
			// Managed Handler が別の要求に再利用できない場合は、false を返します。
			// 要求ごとに保存された状態情報がある場合、通常、これは false になります。
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			throw new NotImplementedException("AsyncHandler.ProcessRequest is not implemented.");
		}

		#endregion

		// BeginInvoke を使うために delegate を定義。
		private delegate void TestDelegate();
		private static readonly TestDelegate TestMethod = () => Thread.Sleep(1000);

		// BeginProcessRequest メソッドは、デリゲートにおける BeginInvoke と同じルールによってシグニチャが
		// 決定している。つまり、最後の二つ意外のパラメタは元の（同期型の）メソッドと同じ。この場合は HttpContext 
		// となっている。最後の二つは非同期呼び出しが完了したら呼び出されるべきコールバックと、非同期呼び出しで情報
		// をやり取りする object 型のデータ。

		// AsyncCallback は、BeginProcessRequest 内部で呼び出すであろう外部の非同期呼び出し
		// （この例では delegate の BeginInvoke）が完了した時点で呼び出されるべきコールバック
		public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
		{
			Debug.WriteLine("Begining async process.");

			// 非同期処理（ここでは Thread.sleep）が完了したら cb が呼び出されなければならないので、
			// Awaken 内で呼び出すことにする。そのためには extraData で渡すしか方法がない。
			IAsyncResult ret =
				TestMethod.BeginInvoke(new AsyncCallback(Awaken), new object[] {context, cb});

			Debug.WriteLine("After calling task.");

			return ret;
		}

		public void EndProcessRequest(IAsyncResult result)
		{
			Debug.WriteLine("EndProcessRequest is called.");

			if (!(result.AsyncState is object[]))
			{
				throw new Exception(string.Format(
					"IAsyncResult.AsyncState is not expected type. Type: ", result.AsyncState.GetType()));
			}

			var extraData = (object[])result.AsyncState;
			if (extraData.Length != 2)
			{
				throw new Exception(string.Format(
					"extraData length is not 2. Length: {0}", extraData.Length));
			}

			if(!(extraData[0] is HttpContext))
			{
				throw new Exception(string.Format(
					"extraData[0] is not expected type. Type: {0}", extraData[0]));
			}

			HttpContext context = (HttpContext)extraData[0];
			context.Response.Write("Hello Async World!");
		}

		// TestMethod（Thread.sleep の処理）が完了したときに呼び出されるコールバックメソッド
		private static void Awaken(IAsyncResult result)
		{
			Debug.WriteLine("Async callback Awaken is called.");

			if (!(result.AsyncState is object[]))
			{
				throw new Exception(string.Format(
					"IAsyncResult.AsyncState is not expected type. Type: ", result.AsyncState.GetType()));
			}

			var extraData = (object[]) result.AsyncState;
			if (extraData.Length != 2)
			{
				throw new Exception(string.Format(
					"extraData length is not 2. Length: {0}", extraData.Length));
			}

			if (!(extraData[1] is AsyncCallback))
			{
				throw new Exception(string.Format(
					"extraData[1] is not expected type. Type: {0}", extraData[1]));
			}

			var callback = (AsyncCallback) extraData[1];
			callback(result);
		}
	}
}
