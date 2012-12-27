using System;
using System.Diagnostics;
using System.Web;
using System.Threading;
using System.IO;

namespace AsyncHttpHandlerSample
{
	public class SyncHandler : IHttpHandler
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
			Debug.WriteLine("Begining sync process.");

			Thread.Sleep(10000);
			Debug.WriteLine("After sleep");

			context.Response.Write("Hello Sync World!");
		}

		#endregion
	}
}
