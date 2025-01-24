using Newtonsoft.Json;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace TenCent {
	public class TenCentAPI {
		private HttpManager.HttpRequest http;

		public TenCentAPI() {
			this.http = new HttpManager.HttpRequest("TenCent");
		}


	}
}