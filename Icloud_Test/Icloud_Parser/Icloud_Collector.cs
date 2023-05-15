using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using static Icloud_Test.Icloud_Struct;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Playwright;
using System.IO;

namespace Icloud_Test.Icloud_Parser
{
	internal class Icloud_Collector
	{
		// 메뉴 설명서
		public static async Task Main_Menu()
		{
			Console.WriteLine("\n");
			Console.WriteLine("------------------------------------------------------------------------------------");
			Console.WriteLine("★ M.E.N.U");
			Console.WriteLine("0. 종료");
			Console.WriteLine("1. Icloud 유저 정보 출력");
			Console.WriteLine("2. Icloud 메타데이터 출력");
			Console.WriteLine("3. Icloud 검색");
			Console.WriteLine("4. Icloud 다운로드");
			Console.WriteLine("5. 메뉴 다시보기");
			Console.WriteLine();
		}

	
		// 진입점
		public static async Task Menu()
		{
			Main_Menu();

			while (true)
			{
				Console.Write("Select Menu : ");
				string cmd_number = Console.ReadLine();

				if (cmd_number == "0") break;

				// 유저 데이터 출력
				else if (cmd_number == "1") { await Show_User_Meta_Data(); Main_Menu(); }

				// 메타데이터 출력
				else if (cmd_number == "2") { Show_Drive_Meta_Data(); Main_Menu(); }

				// 검색
				else if (cmd_number == "3") { /*Search();*/ Main_Menu(); }

				// 다운로드
				else if (cmd_number == "4")
				{
					while (true)
					{
						Console.WriteLine();
						Console.WriteLine("------------------------------------------------------------------------------------");
						Console.WriteLine("★ 수집 방법을 선택하세요. ");
						Console.WriteLine("0. 메뉴로 돌아가기");
						Console.WriteLine("1. 이름 또는 아이디로 파일 수집");
						Console.WriteLine("2. 전체 파일 수집\n");

						Console.Write("Select Download_menu : ");
						string select = Console.ReadLine();

						if (select == "0") { Main_Menu(); break; }

						else if (select == "1") 
						{
							Console.Write("수집할 파일의 이름 또는 아이디를 입력(상위 메뉴로 돌아가기 -1): ");
							string Answer = Console.ReadLine();

							if (Answer == "-1") continue;
							else if (!await File_Export_With_Name(Icloud_Authentication.Icloud_Drive_Tree, Answer))
								Console.WriteLine("입력한 파일의 이름 또는 아이디는 클라우드 내에 존재하지 않습니다.");
						}

						else if (select == "2") await File_Export_All(Icloud_Authentication.Icloud_Drive_Tree);

						else Console.WriteLine("잘못누르셨습니다. 다시 입력하세요. \n");
					}
				}
				else { Console.WriteLine("잘못누르셨습니다. 다시 입력하세요. \n"); Main_Menu(); }
			}
		}


		// 유저 정보 출력 관련
		public static async Task Show_User_Meta_Data()
		{
			Console.Write("★ 성: ");
			Console.WriteLine(Icloud_Authentication.Icloud_Personal_Information_Data.lastName.ToString());

			Console.Write("★ 이름: ");
			Console.WriteLine(Icloud_Authentication.Icloud_Personal_Information_Data.firstName.ToString());

			Console.Write("★ 전체 이름: ");
			Console.WriteLine(Icloud_Authentication.Icloud_Personal_Information_Data.fullName.ToString());

			Console.Write("★ 아이디: ");
			Console.WriteLine(Icloud_Authentication.Icloud_Personal_Information_Data.appleId.ToString());

			Console.Write("★ 계정 식별자: ");
			Console.WriteLine(Icloud_Authentication.Icloud_Personal_Information_Data.dsid.ToString());

			Console.Write("★ 국적: ");
			Console.WriteLine(Icloud_Authentication.Icloud_Personal_Information_Data.countryCode.ToString());

			Console.Write("★ 타임존: ");
			Console.WriteLine(Icloud_Authentication.Icloud_Personal_Information_Data.timeZone.ToString());


			/*Console.Write("★ 전화번호: ");
			Console.WriteLine(Icloud_Authentication.Icloud_Personal_Information_Data.timeZone.ToString());*/
		}

		// 메타데이터 출력 관련
		public static async Task Show_Drive_Meta_Data()
		{
			while (true)
			{
				Console.WriteLine();
				Console.WriteLine("------------------------------------------------------------------------------------");
				Console.WriteLine("★ 메타데이터 출력 방법을 선택하세요. ");
				Console.WriteLine("0. 메뉴로 돌아가기");
				Console.WriteLine("1. 트리 구조도 출력");
				Console.WriteLine("2. 특정 폴더의 메타데이터 출력");
				Console.WriteLine("3. 특정 파일의 메타데이터 출력");
				Console.WriteLine("4. 전체 메타데이터 출력\n");

				Console.Write("메타데이터 출력 메뉴 선택: ");
				string select = Console.ReadLine();

				if (select == "0") { break; }

				else if (select == "1") await Show_Drive_Meta_Data_Structure(Icloud_Authentication.Icloud_Drive_Tree);

				else if (select == "2")
				{
					Console.Write("메타데이터를 출력할 폴더의 이름 또는 아이디를 입력(상위 메뉴로 돌아가기 -1): ");
					string Answer = Console.ReadLine();

					if (Answer == "-1") continue;
					else if (!await Show_Drive_Meta_Data_Folder(Icloud_Authentication.Icloud_Drive_Tree, Answer))
						Console.WriteLine("입력한 폴더의 이름 또는 아이디는 클라우드 내에 존재하지 않습니다.");
				}

				else if (select == "3")
				{
					Console.Write("메타데이터를 출력할 파일의 이름 또는 아이디를 입력(상위 메뉴로 돌아가기 -1): ");
					string Answer = Console.ReadLine();

					if (Answer == "-1") continue;
					else if (!await Show_Drive_Meta_Data_File(Icloud_Authentication.Icloud_Drive_Tree, Answer))
						Console.WriteLine("입력한 파일의 이름 또는 아이디는 클라우드 내에 존재하지 않습니다.");
				}

				else if (select == "4") await Show_Drive_Meta_Data_All(Icloud_Authentication.Icloud_Drive_Tree);
				
				else Console.WriteLine("잘못누르셨습니다. 다시 입력하세요. \n");
			}
		}

		// 트리 구조도 함수
		public static async Task Show_Drive_Meta_Data_Structure(Icloud_Drive_Node Node, int count = 0)
		{
			string Interval = "    ";
			int new_count = count + 1;

			// 폴더 자신 정보
			Console.WriteLine(string.Concat(Enumerable.Repeat(Interval, count)) + "▶ " + Node.Folder.name + "    " + Node.Folder.docwsid);

			// 하위 폴더로
			foreach (var SubFolder in Node.Children)
			{
				await Show_Drive_Meta_Data_Structure(SubFolder, new_count);
			}

			// 같이 있는 파일 정보
			foreach (var File in Node.File)
			{
				Console.WriteLine(string.Concat(Enumerable.Repeat(Interval, new_count)) + "◆ " + File.name + "    " + File.docwsid);
			}

			Console.WriteLine();
		}

		// 특정 폴더의 메타데이터를 출력하는 함수
		public static async Task<bool> Show_Drive_Meta_Data_Folder(Icloud_Drive_Node Node, string Input)
		{
			bool flag = false;

			if (Node.Folder.name == Input || Node.Folder.docwsid == Input)
			{
				await Show_Folder_Meta_Data(Node.Folder);
				return true;
			}
			else
			{
				foreach (var SubFolder in Node.Children)
				{
					flag = await Show_Drive_Meta_Data_Folder(SubFolder, Input);
					if (flag) return flag; // 재귀 탈출용 플래그
				}
			}

			return flag;
		}

		// 특정 파일의 메타데이터를 출력하는 함수
		public static async Task<bool> Show_Drive_Meta_Data_File(Icloud_Drive_Node Node, string Input)
		{
			bool flag = false;

			foreach (var File in Node.File)
			{
				if (File.name == Input || File.docwsid == Input)
				{
					Console.WriteLine("★ 파일 메타데이터");
					Console.WriteLine("------------------------------------------------------------------------------------");
					
					await Show_File_Meta_Data(File);
					return true;
				}
			}

			foreach (var SubFolder in Node.Children)
			{
				flag = await Show_Drive_Meta_Data_File(SubFolder, Input);
				if (flag) return flag; // 재귀 탈출용 플래그
			}

			return flag;
		}

		// 모든 메타데이터를 출력하는 함수
		public static async Task Show_Drive_Meta_Data_All(Icloud_Drive_Node Node)
		{
			if (Node.Parent == null) Console.WriteLine("★ 상위 폴더: 없음");
			else Console.WriteLine("★ 상위 폴더: " + Node.Parent.Folder.name);

			Show_Folder_Meta_Data(Node.Folder);
			Show_FileList_Meta_Data(Node.File);

			foreach (var SubFolder in Node.Children)
			{
				Show_Drive_Meta_Data_All(SubFolder);
				Console.WriteLine("------------------------------------------------------------------------------------");
			}
		}
		
		public static async Task Show_FileList_Meta_Data(List<File_Meta_Data> Files)
		{
			Console.WriteLine("★ 파일 리스트");
			Console.WriteLine("------------------------------------------------------------------------------------");

			foreach (var file in Files)
			{
				await Show_File_Meta_Data(file);
			}
		}

		public static async Task Show_Folder_Meta_Data(Folder_Meta_Data folder)
		{
			Console.WriteLine("★ 폴더: " + folder.name);
			Console.WriteLine("------------------------------------------------------------------------------------");

			Console.Write(" dateCreated ▶ ");
			Console.WriteLine(folder.dateCreated);

			Console.Write(" drivewsid ▶ ");
			Console.WriteLine(folder.drivewsid);

			Console.Write(" docwsid ▶ ");
			Console.WriteLine(folder.docwsid);

			Console.Write(" zone ▶ ");
			Console.WriteLine(folder.zone);

			Console.Write(" name ▶ ");
			Console.WriteLine(folder.name);

			Console.Write(" parentId ▶ ");
			Console.WriteLine(folder.parentId);

			Console.Write(" parentName ▶ ");
			Console.WriteLine(folder.parentName);

			Console.Write(" hierarchy ▶ ");
			Console.WriteLine(folder.hierarchy);

			Console.Write(" isChainedToParent ▶ ");
			Console.WriteLine(folder.isChainedToParent);

			Console.Write(" etag ▶ ");
			Console.WriteLine(folder.etag);

			Console.Write(" type ▶ ");
			Console.WriteLine(folder.type);

			Console.Write(" assetQuota ▶ ");
			Console.WriteLine(folder.assetQuota);

			Console.Write(" fileCount ▶ ");
			Console.WriteLine(folder.fileCount);

			Console.Write(" shareCount ▶ ");
			Console.WriteLine(folder.shareCount);

			Console.Write(" shareAliasCount ▶ ");
			Console.WriteLine(folder.shareAliasCount);

			Console.Write(" directChildrenCount ▶ ");
			Console.WriteLine(folder.directChildrenCount);

			Console.Write(" numberOfItems ▶ ");
			Console.WriteLine(folder.numberOfItems);

			Console.WriteLine("------------------------------------------------------------------------------------");
		}

		public static async Task Show_File_Meta_Data(File_Meta_Data file)
		{
			Console.Write(" dateCreated ▶ ");
			Console.WriteLine(file.dateCreated);

			Console.Write(" drivewsid ▶ ");
			Console.WriteLine(file.drivewsid);

			Console.Write(" docwsid ▶ ");
			Console.WriteLine(file.docwsid);

			Console.Write(" zone ▶ ");
			Console.WriteLine(file.zone);

			Console.Write(" name ▶ ");
			Console.WriteLine(file.name);

			Console.Write(" extension ▶ ");
			Console.WriteLine(file.extension);

			Console.Write(" parentId ▶ ");
			Console.WriteLine(file.parentId);

			Console.Write(" parentName ▶ ");
			Console.WriteLine(file.parentName);

			Console.Write(" isChainedToParent ▶ ");
			Console.WriteLine(file.isChainedToParent);

			Console.Write(" dateModified ▶ ");
			Console.WriteLine(file.dateModified);

			Console.Write(" dateChanged ▶ ");
			Console.WriteLine(file.dateChanged);

			Console.Write(" size ▶ ");
			Console.WriteLine(file.size);

			Console.Write(" etag ▶ ");
			Console.WriteLine(file.etag);

			Console.Write(" lastOpenTime ▶ ");
			Console.WriteLine(file.lastOpenTime);

			Console.Write(" type ▶ ");
			Console.WriteLine(file.type);

			Console.WriteLine("------------------------------------------------------------------------------------");
		}




		// 다운로드 관련

		// 토큰을 가져오는 함수

		// 다운로드 url을 가져오는 함수
		public static async Task<JToken> Get_Drive_Download_Token(string docwsid)
		{
			string Url = "https://p125-docws.icloud.com/ws/com.apple.CloudDocs/download/batch";
			Uri uri = new Uri(Url);
			HttpClientHandler Handler = new HttpClientHandler();

			Handler.CookieContainer = new CookieContainer();
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-TOKEN", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-TOKEN"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-VALIDATE", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-VALIDATE"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-PCS-Sharing", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-PCS-Sharing"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-PCS-News", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-PCS-News"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-PCS-Notes", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-PCS-Notes"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-PCS-Documents", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-PCS-Documents"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-USER", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-USER"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-UNIQUE-CLIENT-ID", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-UNIQUE-CLIENT-ID"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X_APPLE_WEB_KB-K3VMN7P_W6TG870JF8C-69LIJRE", Icloud_Authentication.Icloud_Cookie_Data["X_APPLE_WEB_KB-K3VMN7P_W6TG870JF8C-69LIJRE"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEB-ID", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEB-ID"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-DS-WEB-SESSION-TOKEN", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-DS-WEB-SESSION-TOKEN"]));

			HttpClient client = new HttpClient(Handler);

			// Headers
			client.DefaultRequestHeaders.Add("Accept", "*/*");
			client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
			client.DefaultRequestHeaders.Add("Referer", "https://www.icloud.com/");
			client.DefaultRequestHeaders.Add("Origin", "https://www.icloud.com");

			// Body
			Icloud_Authentication.Icloud_Drive_Download_Token_Post_Data.document_id = docwsid;
			Icloud_Authentication.Icloud_Drive_Download_Token_Post_Json = JsonConvert.SerializeObject(Icloud_Authentication.Icloud_Drive_Download_Token_Post_Data);
			HttpContent data = new StringContent(Icloud_Authentication.Icloud_Drive_Download_Token_Post_Json, Encoding.UTF8, "text/plain");

			var Response = client.PostAsync(uri, data).Result;
			string Temp = "{Response:" + Response.Content.ReadAsStringAsync().Result + "}";
			JObject Result = JObject.Parse(Temp);

			return Result["Response"][0];
		}

		// 이름 or ID 기반 파일 다운로드
		public static async Task<bool> File_Export_With_Name(Icloud_Drive_Node Node, string Input)
		{
			bool flag = false;

			File_Meta_Data Target = Node.File.Find((x => x.name == Input || x.docwsid == Input));

			if (Target != null)
			{
				JToken Download_Json = await Get_Drive_Download_Token(Target.docwsid);
				string Download_Url = Download_Json["data_token"]["url"].ToString();

				// 다운로드 로직
				Uri uri = new Uri(Download_Url);
				HttpClientHandler Handler = new HttpClientHandler();
				HttpClient client = new HttpClient(Handler);

				// Headers
				client.DefaultRequestHeaders.Add("Accept", "*/*");
				client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
				var Response = client.GetAsync(uri).Result;

				byte[] ByteArray_Response = Response.Content.ReadAsByteArrayAsync().Result;

				Stream stream = new FileStream(".downloads/" + Target.name + "." + Target.extension, FileMode.Create);
				using (BinaryWriter wr = new BinaryWriter(stream))
				{
					for (int i = 0; i < ByteArray_Response.Length; i++)
						wr.Write(ByteArray_Response[i]);
				}

				Console.WriteLine();
				Console.WriteLine("★ 다운로드한 파일");
				Console.WriteLine("★ 이름: " + Target.name);
				Console.WriteLine("★ 아이디: " + Target.docwsid);
				Console.WriteLine("★ 생성시간: " + Target.dateCreated);
				// 위치까지 만들면 good!

				return true;
			}


			foreach (var SubFolder in Node.Children)
			{
				flag = await File_Export_With_Name(SubFolder, Input);
				if (flag) return flag;
			}

			return flag;
		}

		// 모든 파일 다운로드
		public static async Task File_Export_All(Icloud_Drive_Node Node)
		{
			foreach (var File in Node.File)
			{
				JToken Download_Json = await Get_Drive_Download_Token(File.docwsid);
				string Download_Url = Download_Json["data_token"]["url"].ToString();

				// 다운로드 로직
				Uri uri = new Uri(Download_Url);
				HttpClientHandler Handler = new HttpClientHandler();
				HttpClient client = new HttpClient(Handler);

				// Headers
				client.DefaultRequestHeaders.Add("Accept", "*/*");
				client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
				var Response = client.GetAsync(uri).Result;

				byte[] ByteArray_Response = Response.Content.ReadAsByteArrayAsync().Result;

				Stream stream = new FileStream(".downloads/" + File.name + "." + File.extension, FileMode.Create);
				using (BinaryWriter wr = new BinaryWriter(stream))
				{
					for (int i = 0; i < ByteArray_Response.Length; i++)
						wr.Write(ByteArray_Response[i]);
				}
				
				Console.WriteLine();
				Console.WriteLine("★ 다운로드한 파일");
				Console.WriteLine("★ 이름: " + File.name);
				Console.WriteLine("★ 아이디: " + File.docwsid);
				Console.WriteLine("★ 생성시간: " + File.dateCreated);

			}

			foreach (var SubFolder in Node.Children)
			{
				await File_Export_All(SubFolder);
			}
		}
	}
}
