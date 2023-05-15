using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Icloud_Test
{
	internal class Icloud_Struct
	{
		public static class Icloud_Authentication
		{
			// Post 요청 관련 클래스
			public static Login_Post_Data Icloud_Login_Post_Data { get; set; }
			public static UserAgent_Post_Data Icloud_UserAgent_Post_Data { get; set; }
			public static Setup_Post_Data Icloud_Setup_Post_Data { get; set; }
			public static Drive_Auth_Post_Data Icloud_Drive_Auth_Post_Data { get; set; }
			public static Drive_Download_Token_Post_Data Icloud_Drive_Download_Token_Post_Data { get; set; }


			// 데이터 수집 관련 클래스
			public static Session_Data Icloud_Session_Data { get; set; }
			public static Personal_Information_Data Icloud_Personal_Information_Data { get; set; }
			public static Dictionary<string, string> Icloud_HTML_Data { get; set; }
			public static Dictionary<string, string> Icloud_Cookie_Data { get; set; }


			// Icloud_Drive 데이터 수집 관련 클래스 - 트리구조
			public static Icloud_Drive_Node Icloud_Drive_Tree { get; set; }


			// Post 요청 Json 관련
			public static string Icloud_Login_Post_Json { get; set; }
			public static string Icloud_UserAgent_Post_Json { get; set; }
			public static string Icloud_Second_Login_Post_Json { get; set; }
			public static string Icloud_Setup_Post_Json { get; set; }
			public static string Icloud_Drive_Auth_Post_Json { get; set; }
			public static string Icloud_Drive_Download_Token_Post_Json { get; set; }

			public static void init()
			{
				Icloud_Login_Post_Data = new Login_Post_Data();
				Icloud_Login_Post_Data.init();

				Icloud_UserAgent_Post_Data = new UserAgent_Post_Data();
				Icloud_UserAgent_Post_Data.init();

				Icloud_Session_Data = new Session_Data();
				Icloud_Session_Data.init();

				Icloud_Setup_Post_Data = new Setup_Post_Data();
				Icloud_Setup_Post_Data.init();

				Icloud_Personal_Information_Data = new Personal_Information_Data();
				Icloud_Personal_Information_Data.init();

				Icloud_Drive_Auth_Post_Data = new Drive_Auth_Post_Data();
				Icloud_Drive_Auth_Post_Data.init();

				Icloud_Drive_Download_Token_Post_Data = new Drive_Download_Token_Post_Data();
				Icloud_Drive_Download_Token_Post_Data.init();

				Icloud_HTML_Data = new Dictionary<string, string>();
				Icloud_Cookie_Data = new Dictionary<string, string>();

				// Icloud Drive 관련
				Icloud_Drive_Tree = new Icloud_Drive_Node();
				Icloud_Drive_Tree.init();
			}
		}

		// Icloud 로그인에서 수집 정보 관련
		public class Personal_Information_Data
		{
			public string dsid { get; set; }
			public string firstName { get; set; }
			public string lastName { get; set; }
			public string fullName { get; set; }
			public string appleId { get; set; }
			public string countryCode { get; set; }
			public string phoneNumber { get; set; }
			public string timeZone { get; set; }

			public void init()
			{
				dsid = "";
				firstName = "";
				lastName = "";
				fullName = "";
				appleId = "";
				countryCode = "";
				phoneNumber = "";
				timeZone = "";
			}
		}
		public class Session_Data
		{
			public string Session_ID { get; set; }
			public string Session_Token { get; set; }
			public string Scnt { get; set; }
			public string Attributes { get; set; }

			public void init()
			{
				Session_ID = "";
				Session_Token = "";
				Scnt = "";
				Attributes = "";
			}
		}



		// Icloud Drive에서 정보 관련
		public class Icloud_Drive_Node // 트리 각 층을 담당하는 구조
		{
			public Icloud_Drive_Node Parent { get; set; } // 부모 노드
			public List<Icloud_Drive_Node> Children { get; set; } // 하위 폴더
			public List<File_Meta_Data> File { get; set; } // 자기가 가지고 있는 파일
			public Folder_Meta_Data Folder { get; set; } // 내 정보

			public void init()
			{
				Parent = new Icloud_Drive_Node();
				Children = new List<Icloud_Drive_Node>();
				File = new List<File_Meta_Data>();
				Folder = new Folder_Meta_Data();
			}
		}

		


		public class File_Meta_Data // 파일
		{
			public string dateCreated { get; set; }
			public string drivewsid { get; set; }
			public string docwsid { get; set; }
			public string zone { get; set; }
			public string name { get; set; }
			public string extension { get; set; }
			public string parentId { get; set; }
			public string parentName { get; set; }
			public string isChainedToParent { get; set; }
			public string dateModified { get; set; }
			public string dateChanged { get; set; }
			public string size { get; set; }
			public string etag { get; set; }
			public string lastOpenTime { get; set; }
			public string type { get; set; }

			public void init()
			{
				dateCreated = "";
				drivewsid = "";
				docwsid = "";
				zone = "";
				name = "";
				extension = "";
				parentId = "";
				parentName = "";
				isChainedToParent = "";
				dateModified = "";
				dateChanged = "";
				size = "";
				etag = "";
				lastOpenTime = "";
				type = "";
			}
		}
		public class Folder_Meta_Data // 폴더
		{
			public string dateCreated { get; set; }
			public string drivewsid { get; set; }
			public string docwsid { get; set; }
			public string zone { get; set; }
			public string name { get; set; }
			public string parentId { get; set; }
			public string parentName { get; set; }
			public string hierarchy { get; set; } // 요청 보낼 때 받을 예정
			public string isChainedToParent { get; set; }
			public string etag { get; set; }
			public string type { get; set; }
			public string assetQuota { get; set; }
			public string fileCount { get; set; }
			public string shareCount { get; set; }
			public string shareAliasCount { get; set; }
			public string directChildrenCount { get; set; }
			public string numberOfItems { get; set; } // 루트 깊이에서의 현재 순수 파일들의 수 (요청 보낼 때 받을 예정)

			public void init()
			{
				dateCreated = "";
				drivewsid = "";
				docwsid = "";
				zone = "";
				name = "";
				parentId = "";
				parentName = "";
				hierarchy = "";
				isChainedToParent = "";
				etag = "";
				type = "";
				assetQuota = "";
				fileCount = "";
				shareCount = "";
				shareAliasCount = "";
				directChildrenCount = "";
			}
		}


		// 로그인에 필요한 데이터
		public class Login_Post_Data
		{
			public string accountName { get; set; }
			public string password { get; set; }
			public string rememberMe { get; set; }
			public string trustTokens { get; set; }
			public void init()
			{
				rememberMe = "False";
				trustTokens = null;
			}
		}
		public class UserAgent_Post_Data
		{
			public string U { get; set; }
			public string L { get; set; }
			public string Z { get; set; }
			public string V { get; set; }
			public string F { get; set; }

			public void init()
			{
				U = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";
				L = "ko_KR";
				Z = "GMT+09:00";
				V = "1.1";
				F = "";
			}
		}


		// 로그인한 사용자의 개인 정보 데이터를 요청하는데 필요한 데이터
		public class Setup_Post_Data
		{
			public string dsWebAuthToken { get; set; }
			public string accountCountryCode { get; set; }
			public string extended_login { get; set; }

			public void init()
			{
				dsWebAuthToken = "";
				extended_login = "true"; // ??
				accountCountryCode = "";
			}
		}


		// Icloud Drive 요청 관련 데이터
		public class Drive_Auth_Post_Data
		{
			public string drivewsid { get; set; }
			public string partialData { get; set; }
			public string includeHierarchy { get; set; }

			public void init()
			{
				drivewsid = "";
				partialData = "false"; // 요약 데이터만 뽑는 옵션
				includeHierarchy  = "true"; // 계층 구조를 알 수 있게 해주는 옵션
			}

		}

		// Icloud 다운로드 토큰 데이터
		public class Drive_Download_Token_Post_Data
		{
			public string document_id { get; set; }

			public void init()
			{
				document_id = "";
			}
		}




	}
}
