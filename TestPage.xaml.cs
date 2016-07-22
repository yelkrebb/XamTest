using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Collections.ObjectModel;
using Motion.Mobile.Core.BLE;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Motion.Misc.Utilities;
using Motion.Core.WSHandler;
using Motion.Core.SyncHandler;
using Newtonsoft.Json;

using PCLStorage;
using System.IO;

namespace XamTest
{
	public partial class TestPage : ContentPage
	{


		string codeGenerated;

		IAdapter adapter;
		IDevice device;
		IService service;
		ICharacteristic ff07Char;
		ICharacteristic ff08Char;

		ObservableCollection<IDevice> devices;
		ObservableCollection<IService> services;
		ObservableCollection<ICharacteristic> characteristics;
		public TestPage (IAdapter adapter)
		{
			InitializeComponent ();
			this.adapter = adapter;
			this.devices = new ObservableCollection<IDevice> ();
			this.services = new ObservableCollection<IService> ();
			this.characteristics = new ObservableCollection<ICharacteristic> ();

			adapter.BluetoothStateUpdated += Adapter_BluetoothStateUpdated;
			adapter.ScanCompleted += Adapter_ScanCompleted;
			adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
			adapter.DeviceConnected += Adapter_DeviceConnected;
			adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
			adapter.DeviceFailedToConnect += Adapter_DeviceFailedToConnect;
			adapter.CommandResponse += Adapter_CommandResponse;
		
			btnValidate.Clicked += OnStartClicked;
			btnGenerateCode.Clicked += OnGenerateCodeClicked;
			btnDisconnect.Clicked += OnDisconnectDevice;

			btnGenSerial.Clicked += OnGenSerialClicked;
			btnValidateUser.Clicked += BtnValidateUser_Clicked;
			btnChangeUser.Clicked += BtnChangeUser_Clicked;

			//file.CreateFileAsync ("testFile.txt");
		}

		async void BtnChangeUser_Clicked(object sender, EventArgs e)
		{
			IFolder rootFolder = FileSystem.Current.LocalStorage;
			IFolder folder = await rootFolder.CreateFolderAsync("Cache",
				CreationCollisionOption.OpenIfExists);
			IFile file = await folder.CreateFileAsync("Test3" + ".txt",
			                                          CreationCollisionOption.OpenIfExists);
			
			Stream fileStream = await file.OpenAsync(FileAccess.ReadAndWrite);
			if (fileStream.CanSeek)
			{
				Debug.WriteLine("Can Seek");
				fileStream.Seek(0, SeekOrigin.End);
			}
			string str;
			using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
			{
				await writer.WriteLineAsync("Berkssss");


				writer.Flush();
			}

			fileStream = await file.OpenAsync(FileAccess.Read);
			using (StreamReader reader = new StreamReader(fileStream, Encoding.UTF8))
			{
				 str = reader.ReadToEnd();
			}
			Debug.WriteLine(str);
			IList<IFile> filenames = await folder.GetFilesAsync(System.Threading.CancellationToken.None);

			foreach(IFile f in filenames)
			{
				Debug.WriteLine(f.Name);	
			}
		}

		void BtnValidateUser_Clicked(object sender, EventArgs e)
		{
			//Post 
			WebService ws = new WebService();
			string credentials = txtEmail.Text + "|" + txtPassword.Text;
			Dictionary<string, object> data = new Dictionary<string, object> { };
			data.Add("ident", credentials);
			Task<Dictionary<string,object>> response = ws.PostData("https://test.savvysherpa.com/DeviceServices/api/Pedometer/ValidateUser", data);
			string dataStr = JsonConvert.SerializeObject(response);
			Debug.WriteLine(dataStr);
		}

		async void OnGenSerialClicked(object sender, EventArgs e)
		{
			WebService ws = new WebService();

			//Get
			Task<Dictionary<string,object>> task1 = ws.GetData("https://test.savvysherpa.com/DeviceServices/api/Production/GenerateSerial");

			Dictionary<string, object> response = await task1;

			string dataStr = JsonConvert.SerializeObject(response);
			Debug.WriteLine(dataStr);
		}

		void Adapter_BluetoothStateUpdated (object sender, BluetoothStateEventArgs e)
		{
			Debug.WriteLine ("BT state: " + e.BTState);
		}

		void Adapter_CommandResponse (object sender, CommandResponseEventArgs e)
		{
			Debug.WriteLine ("Response value " + System.Text.Encoding.UTF8.GetString(e.Data,0,e.Data.Length));
			Device.BeginInvokeOnMainThread(() => {
				this.lblCodeCaption.IsVisible = true;
				this.txtCode.IsVisible = true;
				this.btnValidate.IsVisible = true;
				btnValidate.IsEnabled = true;
				btnValidate.TextColor = Color.Blue;
			});
		}

		void Device_ServicesDiscovered (object sender, EventArgs e)
		{
			Debug.WriteLine("device.ServicesDiscovered");
			//this.services = (List<IService>)device.Services;

			if (this.services.Count == 0) 
			{
				Debug.WriteLine ("Service Count: " + device.Services.Count);
				foreach (var service in device.Services) 
				{
					service.CharacteristicsDiscovered += (object s, EventArgs ce) => {

						Debug.WriteLine("service.CharacteristicsDiscovered");
						if (this.characteristics.Count == 0)
						{
							Debug.WriteLine("Characteristics count: "+service.Characteristics.Count);
							foreach (var characteristic in service.Characteristics) 
							{
								if(characteristic !=null)
								{
									if(characteristic.Uuid.ToUpper().Contains("2A24") || 
										characteristic.Uuid.ToUpper().Contains("2A25") ||
										characteristic.Uuid.ToUpper().Contains("2A26") ||
										characteristic.Uuid.ToUpper().Contains("2A19") )
									{
										if(characteristic.CanRead)
										{
											characteristic.ValueUpdated += (cs, chre) => {
												Debug.WriteLine("characteristic.ValueUpdated");
											};
											characteristic.ReadAsync();
										}
									}
									else if(characteristic.Uuid.ToUpper().Contains("FF07"))
									{
										Debug.WriteLine("FF07");
										if(characteristic.CanUpdate)
										{
											this.ff07Char = characteristic;

											characteristic.StartUpdates();
										}
									}
									else if(characteristic.Uuid.ToUpper().Contains("FF08"))
									{
										Debug.WriteLine("FF08");
										if(characteristic.CanUpdate)
										{
											this.ff08Char = characteristic;

											characteristic.StartUpdates();
										}
									}
								}
							}
						}
					};

					service.DiscoverCharacteristics ();
				}
			}
		}


		void Adapter_DeviceFailedToConnect (object sender, DeviceConnectionEventArgs e)
		{
			StartScanning ();
		}

		void Adapter_DeviceDisconnected (object sender, DeviceConnectionEventArgs e)
		{
			Device.BeginInvokeOnMainThread(() => {
				this.lblConnectedDevice.IsVisible = false;
				this.lblCodeCaption.IsVisible = false;
				this.txtCode.IsVisible = false;
				this.btnValidate.IsVisible = false;
				this.btnGenerateCode.IsVisible = false;
				this.btnValidate.IsEnabled = false;
				this.btnDisconnect.IsVisible = false;
				this.lblConnectedDevice.Text = "";
				this.txtCode.Text = "";
			});

			//this.StartScanning();
			this.StartScanning (0x180F.UuidFromPartial ());
		}

		void Adapter_DeviceConnected (object sender, DeviceConnectionEventArgs e)
		{
			Debug.WriteLine("Device connected");
			//adapter.StopScanningForDevices();
			StopScanning();
			Device.BeginInvokeOnMainThread(() => {
				this.lblConnectedDevice.IsVisible = true;
				this.btnGenerateCode.IsVisible = true;
				this.btnDisconnect.IsVisible = true;
				this.lblConnectedDevice.Text = "Device Connected: " + e.Device.Name;
			});

			Debug.WriteLine("Connected Devices Count: "+ adapter.ConnectedDevices.Count);

			device = e.Device;
			device.ServicesDiscovered += Device_ServicesDiscovered;

			device.DiscoverServices ();
		}

		void Adapter_DeviceDiscovered (object sender, DeviceDiscoveredEventArgs e)
		{
			Debug.WriteLine("Device discovered");
			Device.BeginInvokeOnMainThread(() => {
				//devices.Add (e.Device);
				//Debug.WriteLine ("Found device: "+e.Device.Name+")");
				int numLines = this.lblDevice.Text.Split('\n').Length;
				if(numLines > 8)
					this.lblDevice.Text = "";
				String c = this.lblDevice.Text;

				c += "\nName: "+ e.Device.Name + "   RSSI:" + e.RSSI;
				this.lblDevice.Text = c;
			});
			//this.ConnectDevice(e.Device,e.RSSI);
		}

		void Adapter_ScanCompleted(object sender, EventArgs e)
		{
			//StopScanning();

			//this.StartScanning (0x180F.UuidFromPartial ());

			Device.BeginInvokeOnMainThread ( () => {
				//DisplayAlert("Timeout", "Bluetooth scan timeout elapsed", "OK");
			});

			bool foundValidDevice = false;
			foreach (var device in adapter.DiscoveredDevices) {
				if (device.Name.Contains ("961") ||
				   device.Name.Contains ("932") ||
				   device.Name.Contains ("936") ||
				   device.Name.Contains ("939")) {
					this.adapter.ConnectToDevice (device);
					foundValidDevice = true;
					break;
				}
			}

			if (!foundValidDevice)
				StartScanning (0x180F.UuidFromPartial ());
		}

		void OnDisconnectDevice(object sender, EventArgs e)
		{
			adapter.DisconnectDevice(this.device);
		}

		void OnGenerateCodeClicked(object sender, EventArgs e)
		{
			Debug.WriteLine ("Entering getCodeCommand method");

//			byte[] command = this.getCodeCommand ();
//			ff08Char.Write(command);

			adapter.SendCommand (ff08Char, this.getCodeCommand ());
		}

		void OnStartClicked(object sender, EventArgs e)
		{
			//this.ff07Char.Write (this.getCodeCommand());
			if (this.codeGenerated.Equals (txtCode.Text)) {
				DisplayAlert ("Alert", "That's correct!", "OK");
			} else {
				DisplayAlert ("Alert", "Invalid code", "OK");
			}
		}

		void ConnectDevice(IDevice device, int RSSI)
		{

			if (device.Name == null) {
				Debug.WriteLine ("Device Name is NULL");

				return;
			}

			if ( device.Name!=null && (device.Name.Contains ("961") || device.Name.Contains ("932") ||
			   device.Name.Contains ("939") || device.Name.Contains ("936") ||
				device.Name.Contains ("900")) && RSSI >= -75 ) 
			{
				this.adapter.ConnectToDevice (device);
			}
		}

		async void StartScanning () {
			await Task.Delay (500);
			StartScanning (Guid.Empty);
		}

		public async void StartScanning (Guid forService) {
			await Task.Delay (500);
			if (adapter.IsScanning) {
				//adapter.StopScanningForDevices();
				StopScanning();
				Debug.WriteLine ("adapter.StopScanningForDevices()");
			} else {
				devices.Clear();
				List<Guid> serviceUuids = new List<Guid>();
				serviceUuids.Add(forService);
				adapter.StartScanningForDevices(serviceUuids);
				Debug.WriteLine ("adapter.StartScanningForDevices("+forService+")");
			}
		}

		void StopScanning () {
			// stop scanning
			new Task( () => {
				if(adapter.IsScanning) {
					Debug.WriteLine ("Still scanning, stopping the scan");
					adapter.StopScanningForDevices();
				}
			}).Start();
		}


		int CalculateChecksum(byte[] dataToCalculate)
		{
			int checksum = 0;
			foreach (byte chData in dataToCalculate)
			{
				checksum += chData;
			}
			checksum &= 0xffff;
			return checksum;
		}

		byte[] getCodeCommand()
		{
			
			this.codeGenerated = this.generate4DigitRandomNumberString ();
			string codeHexStr = this.convertToHexString (this.codeGenerated);
			string messageCodeStr = String.Format ("072d000000"+codeHexStr+"FF");
			int messageSize = messageCodeStr.Length / 2;
			int checkSum = this.CalculateChecksum (this.StringToByteArray(messageCodeStr));
			string commandStr = "1B540000" + this.convertToHexString(messageSize.ToString()) + this.convertToHexString(checkSum.ToString()) +  messageCodeStr;

			byte[] command = this.StringToByteArray(commandStr);
			Debug.WriteLine ("Get code command str: " +commandStr);
			return  command;
		}

		string generate4DigitRandomNumberString()
		{
			string random = String.Empty;
			Random rnd = new Random();
			for (int i = 0; i < 4; i++) 
			{
				int rand = rnd.Next (0, 9);

				random += rand.ToString();
			}
			return random;
		}

		string convertToHexString(string param)
		{
			char[] charValues = param.ToCharArray();
			string hexOutput="";
			foreach (char _eachChar in charValues )
			{
				// Get the integral value of the character.
				int value = Convert.ToInt32(_eachChar);
				// Convert the decimal value to a hexadecimal value in string form.
				hexOutput += String.Format("{0:X}", value);
				// to make output as your eg 
				//  hexOutput +=" "+ String.Format("{0:X}", value);
			}
			return hexOutput;
		}

		byte[] StringToByteArray(string hex) {
			return Enumerable.Range(0, hex.Length)
				.Where(x => x % 2 == 0)
				.Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
				.ToArray();
		}
	}
}

