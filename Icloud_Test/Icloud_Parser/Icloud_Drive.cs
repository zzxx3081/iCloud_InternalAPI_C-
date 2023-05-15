using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using static Icloud_Test.Icloud_Struct;
using System.Collections.Generic;

namespace Icloud_Test.Icloud_Parser
{

	internal class Icloud_Drive
	{
		// Icloud Drive Parser
		public static async Task Explorer()
		{
			// 메타데이터 수집
			await Get_Drive_Folder_Meta_Data(Icloud_Authentication.Icloud_Drive_Tree, null, "FOLDER::com.apple.CloudDocs::root");
		}


		// 해당 폴더 내의 파일과 폴더 메타데이터를 검색하는 Request를 보내는 함수
		public static async Task<JToken> Retrieve_Item_Details_In_Folders(string drivewsid)
		{
			string Url = "https://p125-drivews.icloud.com/retrieveItemDetailsInFolders?appIdentifier=iclouddrive";
			Uri uri = new Uri(Url);
			HttpClientHandler Handler = new HttpClientHandler();

			Handler.CookieContainer = new CookieContainer();
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-TOKEN", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-TOKEN"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-PCS-Sharing", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-PCS-Sharing"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-PCS-News", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-PCS-News"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-PCS-Notes", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-PCS-Notes"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-PCS-Documents", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-PCS-Documents"]));
			Handler.CookieContainer.Add(uri, new System.Net.Cookie("X-APPLE-WEBAUTH-USER", Icloud_Authentication.Icloud_Cookie_Data["X-APPLE-WEBAUTH-USER"]));
			
			HttpClient client = new HttpClient(Handler);

			// Headers
			client.DefaultRequestHeaders.Add("Accept", "*/*");
			client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
			client.DefaultRequestHeaders.Add("Referer", "https://www.icloud.com/");
			client.DefaultRequestHeaders.Add("Origin", "https://www.icloud.com");

			// Body
			Icloud_Authentication.Icloud_Drive_Auth_Post_Data.drivewsid = drivewsid;
			Icloud_Authentication.Icloud_Drive_Auth_Post_Json = "[" + JsonConvert.SerializeObject(Icloud_Authentication.Icloud_Drive_Auth_Post_Data) + "]";
			HttpContent data = new StringContent(Icloud_Authentication.Icloud_Drive_Auth_Post_Json, Encoding.UTF8, "text/plain");

			var Response = client.PostAsync(uri, data).Result;
			string Temp = "{Response:" + Response.Content.ReadAsStringAsync().Result + "}";
			JObject Result = JObject.Parse(Temp);

			return Result["Response"][0];
		}


		// 재귀적으로 폴더 내부의 모든 데이터를 파싱하는 함수
		public static async Task Get_Drive_Folder_Meta_Data(Icloud_Drive_Node Node, Icloud_Drive_Node Parent, string drivewsid)
		{
			// drivewsid 관련 Json 파일 데이터
			JToken Meta_Data_Set_In_Folder = await Retrieve_Item_Details_In_Folders(drivewsid);
			

			// 현재 폴더 정보
			Node.Folder.dateCreated = Time_Convert(Meta_Data_Set_In_Folder["dateCreated"]);
			Node.Folder.drivewsid = Meta_Data_Set_In_Folder["drivewsid"].ToString();
			Node.Folder.docwsid = Meta_Data_Set_In_Folder["docwsid"].ToString();
			Node.Folder.zone = Meta_Data_Set_In_Folder["zone"].ToString();
			Node.Folder.name = Meta_Data_Set_In_Folder["name"].ToString();
			Node.Folder.hierarchy = Meta_Data_Set_In_Folder["hierarchy"].ToString();
			Node.Folder.etag = Meta_Data_Set_In_Folder["etag"].ToString(); // HTTP 관련 태그
			Node.Folder.type = Meta_Data_Set_In_Folder["type"].ToString();
			Node.Folder.assetQuota = Meta_Data_Set_In_Folder["assetQuota"].ToString(); // 폴더 내 파일의 모든 크기 합산 바이트
			Node.Folder.fileCount = Meta_Data_Set_In_Folder["fileCount"].ToString(); // 하위 폴더까지 포함한 모든 파일 수
			Node.Folder.shareCount = Meta_Data_Set_In_Folder["shareCount"].ToString(); // 공유 파일 수
			Node.Folder.shareAliasCount = Meta_Data_Set_In_Folder["shareAliasCount"].ToString(); // 공유 별명? 수
			Node.Folder.directChildrenCount = Meta_Data_Set_In_Folder["directChildrenCount"].ToString(); // 폴더 아래 하위 노드들의 수
			Node.Folder.numberOfItems = Meta_Data_Set_In_Folder["numberOfItems"].ToString(); // 폴더 안에 파일 + 폴더 수

			Node.Parent = Parent; // 연결만

			if (Node.Parent == null)
			{
				Node.Folder.parentId = "없음";
				Node.Folder.parentName = "없음";
				Node.Folder.isChainedToParent = "false";
				Node.Folder.name = "root";
			}
			else Node.Folder.parentName = Node.Parent.Folder.name;


			foreach (var Unit in Meta_Data_Set_In_Folder["items"])
			{
				if (Unit["type"].ToString() == "FOLDER")
				{
					Icloud_Drive_Node SubFolder = new Icloud_Drive_Node();
					SubFolder.init();

					SubFolder.Folder.drivewsid = Unit["drivewsid"].ToString();
					SubFolder.Folder.parentId = Unit["parentId"].ToString();
					SubFolder.Folder.isChainedToParent = Unit["isChainedToParent"].ToString(); // 위에 부모노드가 존재하는지 여부

					// 하위 폴더 데이터를 보관할 클래스 생성
					await Get_Drive_Folder_Meta_Data(SubFolder, Node, SubFolder.Folder.drivewsid);
					Node.Children.Add(SubFolder);
				}
				else
				{
					// 파일 데이터를 보관할 클래스 생성
					File_Meta_Data File = new File_Meta_Data();
					File.init();

					File.dateCreated = Time_Convert(Unit["dateCreated"]);
					File.drivewsid = Unit["drivewsid"].ToString();
					File.docwsid = Unit["docwsid"].ToString();
					File.zone = Unit["zone"].ToString();
					File.name = Unit["name"].ToString();
					File.extension = Unit["extension"].ToString();
					File.parentId = Unit["parentId"].ToString();
					File.parentName = Node.Folder.parentName; 
					File.isChainedToParent = Unit["isChainedToParent"].ToString();
					File.dateModified = Time_Convert(Unit["dateModified"]);
					File.dateChanged = Time_Convert(Unit["dateChanged"]);
					File.size = Unit["size"].ToString(); // 바이트
					File.etag = Unit["etag"].ToString();
					File.lastOpenTime = Time_Convert(Unit["lastOpenTime"]);
					File.type = Unit["type"].ToString();

					Node.File.Add(File);
				}
			}
		}


		public static string Time_Convert(JToken Time)
		{
			TimeZoneInfo Korea = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
			DateTime Input = Convert.ToDateTime(Time);
			string Result = TimeZoneInfo.ConvertTime(Input, Korea).ToString("yyyy-MM-dd HH:mm:ss");
			return Result;
		}

	}
}
