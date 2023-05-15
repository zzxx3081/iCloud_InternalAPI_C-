using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Playwright;
using System.Linq;
using static Icloud_Test.Icloud_Struct;
using System.Threading;

namespace Icloud_Test.Icloud_Parser
{
	internal class Icloud_Login
	{
		public static async Task Authentication()
		{
			// 초기화 및 아이디, 비밀번호 입력하기
			await Icloud_Init();

			// 로그인 인증 (이름, 국적, dsid, timestamp 등을 추출)
			await Icloud_Auth();
		}

		// 초기화 및 아이디, 비밀번호 입력하기
		public static async Task Icloud_Init()
		{
			Icloud_Authentication.init();
			Icloud_Authentication.Icloud_Login_Post_Data.accountName = "zzxx3081@korea.ac.kr";
			Icloud_Authentication.Icloud_Login_Post_Data.password = "Dfrc4738";
			Icloud_Authentication.Icloud_Login_Post_Json = JsonConvert.SerializeObject(Icloud_Authentication.Icloud_Login_Post_Data); // 아이디, 패스워드 관련 Json
			Icloud_Authentication.Icloud_UserAgent_Post_Json = JsonConvert.SerializeObject(Icloud_Authentication.Icloud_UserAgent_Post_Data); // 크롬 브라우저 및 유저 에이전트 관련 Json
		}

		public static async Task Icloud_Auth()
		{
			var playwright = await Playwright.CreateAsync();
			var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
			var context = await browser.NewContextAsync();
			var page = await context.NewPageAsync();

			// 홈페이지에 들어간다.
			await page.GotoAsync(url: "https://www.icloud.com");

			// 로그인 버튼을 누른다.
			await page.ClickAsync("body > div:nth-child(5) > ui-main-pane > div > div.root-component > div > div > main > div > div.landing-page-content > ui-button");
			Thread.Sleep(3000);

			// HTML에서 client_id 수집
			Icloud_Authentication.Icloud_HTML_Data.Add("client_id", page.ContentAsync().Result.Split(new string[] { "client_id=" }, StringSplitOptions.None)[1].Substring(0, 64));

			// 로그인한 사용자의 정보를 추출하는 함수
			await User_Info(Icloud_Authentication.Icloud_Login_Post_Json, Icloud_Authentication.Icloud_UserAgent_Post_Json);

			// 1차 로그인
			await page.Keyboard.PressAsync("Tab");
			await page.Keyboard.PressAsync("Tab");
			await page.Keyboard.PressAsync("Tab");
			await page.Keyboard.TypeAsync(Icloud_Authentication.Icloud_Login_Post_Data.accountName);
			Thread.Sleep(1000);

			await page.Keyboard.PressAsync("Tab");
			await page.Keyboard.PressAsync("Tab");
			await page.Keyboard.PressAsync("Enter");
			Thread.Sleep(1000);

			await page.Keyboard.TypeAsync(Icloud_Authentication.Icloud_Login_Post_Data.password);
			await page.Keyboard.PressAsync("Enter");
			Thread.Sleep(3000);

			// 2차 로그인
			Console.Write("2차 인증 코드(6자리): ");
			string code = Console.ReadLine();
			await page.Keyboard.TypeAsync(code);
			Thread.Sleep(1000);

			// 브라우저 신뢰창
			await page.Keyboard.PressAsync("Tab");
			await page.Keyboard.PressAsync("Enter");
			Thread.Sleep(10000);

			var Icloud_Cookie = page.Context.CookiesAsync();

			// 웹 인증 토큰 수집
			foreach (var Cookie in Icloud_Cookie.Result)
				Icloud_Authentication.Icloud_Cookie_Data.Add(Cookie.Name, Cookie.Value);

			await page.CloseAsync();
			await browser.CloseAsync();
		}


		// 로그인한 사용자의 정보를 추출하는 함수
		public static async Task User_Info(string Icloud_Login_Post_Json, string Icloud_UserAgent_Post_Json)
		{
			string Url = "https://idmsa.apple.com/appleauth/auth/signin?isRememberMeEnabled=true";
			Uri uri = new Uri(Url);
			HttpClientHandler Handler = new HttpClientHandler();
			HttpClient client = new HttpClient(Handler);

			// Client
			client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
			client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
			client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR, ko; q=0.9, en-US; q=0.8, en; q=0.7");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
			client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

			// Headers
			client.DefaultRequestHeaders.Add("Referer", "https://idmsa.apple.com/appleauth/auth/signin");
			client.DefaultRequestHeaders.Add("X-Apple-I-FD-Client-Info", Icloud_UserAgent_Post_Json);
			client.DefaultRequestHeaders.Add("X-Apple-OAuth-Client-Id", Icloud_Authentication.Icloud_HTML_Data["client_id"]);
			client.DefaultRequestHeaders.Add("X-Apple-OAuth-Client-Type", "firstPartyAuth");
			client.DefaultRequestHeaders.Add("X-Apple-OAuth-Redirect-URI", "https://www.icloud.com");
			client.DefaultRequestHeaders.Add("X-Apple-Widget-Key", Icloud_Authentication.Icloud_HTML_Data["client_id"]);
			client.DefaultRequestHeaders.Add("Origin", "https://idmsa.apple.com");

			HttpContent data = new StringContent(Icloud_Login_Post_Json, Encoding.UTF8, "application/json");
			var Response = client.PostAsync(uri, data).Result;
			
			Icloud_Authentication.Icloud_Session_Data.Scnt = Response.Headers.GetValues("scnt").First();
			Icloud_Authentication.Icloud_Session_Data.Attributes = Response.Headers.GetValues("X-Apple-Auth-Attributes").First();
			Icloud_Authentication.Icloud_Session_Data.Session_ID = Response.Headers.GetValues("X-Apple-ID-Session-Id").First();
			Icloud_Authentication.Icloud_Session_Data.Session_Token = Response.Headers.GetValues("X-Apple-Session-Token").First();
			Icloud_Authentication.Icloud_Setup_Post_Data.dsWebAuthToken = Icloud_Authentication.Icloud_Session_Data.Session_Token;
			Icloud_Authentication.Icloud_Setup_Post_Json = JsonConvert.SerializeObject(Icloud_Authentication.Icloud_Setup_Post_Data);


			// 로그인한 사용자의 신상정보를 요청하는 Url
			Url = "https://setup.icloud.com/setup/ws/1/accountLogin";
			uri = new Uri(Url);
			Handler = new HttpClientHandler();
			client = new HttpClient(Handler);

			// Client
			client.DefaultRequestHeaders.Add("Accept", "*/*");
			client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");

			// Headers
			client.DefaultRequestHeaders.Add("Referer", "https://www.icloud.com/");
			client.DefaultRequestHeaders.Add("Origin", "https://www.icloud.com");
			client.DefaultRequestHeaders.Add("Connection", "keep-alive");

			data = new StringContent(Icloud_Authentication.Icloud_Setup_Post_Json, Encoding.UTF8, "text/plain");
			Response = client.PostAsync(uri, data).Result;

			JObject Personal_Information_Json = JObject.Parse(Response.Content.ReadAsStringAsync().Result);

			// 신상정보 수집 (아이디, 성, 이름, 전체 이름, 나라코드, dsid)
			Icloud_Authentication.Icloud_Personal_Information_Data.appleId = Personal_Information_Json["dsInfo"]["appleId"].ToString();
			Icloud_Authentication.Icloud_Personal_Information_Data.firstName = Personal_Information_Json["dsInfo"]["firstName"].ToString();
			Icloud_Authentication.Icloud_Personal_Information_Data.lastName = Personal_Information_Json["dsInfo"]["lastName"].ToString();
			Icloud_Authentication.Icloud_Personal_Information_Data.fullName = Personal_Information_Json["dsInfo"]["fullName"].ToString();
			Icloud_Authentication.Icloud_Personal_Information_Data.dsid = Personal_Information_Json["dsInfo"]["dsid"].ToString();
			Icloud_Authentication.Icloud_Personal_Information_Data.countryCode = Personal_Information_Json["dsInfo"]["countryCode"].ToString();
			Icloud_Authentication.Icloud_Personal_Information_Data.timeZone = Personal_Information_Json["requestInfo"]["timeZone"].ToString();
		}
	}
}



