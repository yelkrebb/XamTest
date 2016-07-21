using System;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Motion.Mobile.Core.BLE;

//[assembly: XamlCompilationAttribute(XamlCompilationOptions.Compile)]
using System.Collections.ObjectModel;
using System.Diagnostics;
using Motion.Misc.Utilities;
using Motion.Core.WSHandler;
using Motion.Core.SyncHandler;

using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Acrotech.PortableLogAdapter;
using Acrotech.PortableLogAdapter.Managers;

namespace XamTest
{
	public class App : Application
	{
		static IAdapter Adapter;
		TestPage testPage;

		public static readonly ILogManager LogManager = new DelegateLogManager((logger, message) => System.Diagnostics.Debug.WriteLine("[{0}]{1}", logger.Name, message), LogLevel.Info);

		//File file;
		public App ()
		{
			TestObject test = new TestObject();
			string testStr = JsonConvert.SerializeObject(test);
			Debug.WriteLine(testStr);

			ILogger logger = LogManager.GetLogger("TestLogger");

			logger.Info("This is a test log for multi-platform");

			testPage = new TestPage(Adapter);
			MainPage = new NavigationPage(testPage) {
				BarBackgroundColor = Color.Yellow,
				BarTextColor = Color.Blue
			};
		}

		public static void SetAdapter (IAdapter adapter) {
			Adapter = adapter;
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
			this.testPage.StartScanning(0x180F.UuidFromPartial());

		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

