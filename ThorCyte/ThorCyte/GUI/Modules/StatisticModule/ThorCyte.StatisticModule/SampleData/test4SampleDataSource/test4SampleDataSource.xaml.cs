//      *********    请勿修改此文件     *********
//      此文件由设计工具再生成。更改
//      此文件可能会导致错误。
namespace Expression.Blend.SampleData.test4SampleDataSource
{
	using System; 
	using System.ComponentModel;

// 若要在生产应用程序中显著减小示例数据涉及面，则可以设置
// DISABLE_SAMPLE_DATA 条件编译常量并在运行时禁用示例数据。
#if DISABLE_SAMPLE_DATA
	internal class StatisticModel { }
#else

	public class StatisticModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public StatisticModel()
		{
			try
			{
				Uri resourceUri = new Uri("/ModuleA;component/SampleData/test4SampleDataSource/test4SampleDataSource.xaml", UriKind.RelativeOrAbsolute);
				System.Windows.Application.LoadComponent(this, resourceUri);
			}
			catch
			{
			}
		}

		private ComponentContainer _ComponentContainer = new ComponentContainer();

		public ComponentContainer ComponentContainer
		{
			get
			{
				return this._ComponentContainer;
			}

			set
			{
				if (this._ComponentContainer != value)
				{
					this._ComponentContainer = value;
					this.OnPropertyChanged("ComponentContainer");
				}
			}
		}
	}

	public class ComponentContainer : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private ComponentCollection _ComponentCollection = new ComponentCollection();

		public ComponentCollection ComponentCollection
		{
			get
			{
				return this._ComponentCollection;
			}
		}

		private ComponentRecordCollection _ComponentRecordCollection = new ComponentRecordCollection();

		public ComponentRecordCollection ComponentRecordCollection
		{
			get
			{
				return this._ComponentRecordCollection;
			}
		}
	}

	public class ComponentRecord : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}

		private StatisticRecordCollection _StatisticRecordContainer = new StatisticRecordCollection();

		public StatisticRecordCollection StatisticRecordContainer
		{
			get
			{
				return this._StatisticRecordContainer;
			}
		}
	}

	public class StatisticRecord : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}

		private ComponentContainer _ComponentContainer = new ComponentContainer();

		public ComponentContainer ComponentContainer
		{
			get
			{
				return this._ComponentContainer;
			}

			set
			{
				if (this._ComponentContainer != value)
				{
					this._ComponentContainer = value;
					this.OnPropertyChanged("ComponentContainer");
				}
			}
		}

		private StatisticMethodCollection _StatisticMethodContainer = new StatisticMethodCollection();

		public StatisticMethodCollection StatisticMethodContainer
		{
			get
			{
				return this._StatisticMethodContainer;
			}
		}

		private FeatureCollection _FeatureContainer = new FeatureCollection();

		public FeatureCollection FeatureContainer
		{
			get
			{
				return this._FeatureContainer;
			}
		}

		private ChannelCollection _ChannelContainer = new ChannelCollection();

		public ChannelCollection ChannelContainer
		{
			get
			{
				return this._ChannelContainer;
			}
		}

		private RegionCollection _RegionContainer = new RegionCollection();

		public RegionCollection RegionContainer
		{
			get
			{
				return this._RegionContainer;
			}
		}
	}

	public class Component : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}
	}

	public class StatisticMethod : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}
	}

	public class Feature : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}
	}

	public class Channel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}
	}

	public class Region : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private string _Name = string.Empty;

		public string Name
		{
			get
			{
				return this._Name;
			}

			set
			{
				if (this._Name != value)
				{
					this._Name = value;
					this.OnPropertyChanged("Name");
				}
			}
		}
	}

	public class ComponentCollection : System.Collections.ObjectModel.ObservableCollection<Component>
	{ 
	}

	public class ComponentRecordCollection : System.Collections.ObjectModel.ObservableCollection<ComponentRecord>
	{ 
	}

	public class StatisticRecordCollection : System.Collections.ObjectModel.ObservableCollection<StatisticRecord>
	{ 
	}

	public class StatisticMethodCollection : System.Collections.ObjectModel.ObservableCollection<StatisticMethod>
	{ 
	}

	public class FeatureCollection : System.Collections.ObjectModel.ObservableCollection<Feature>
	{ 
	}

	public class ChannelCollection : System.Collections.ObjectModel.ObservableCollection<Channel>
	{ 
	}

	public class RegionCollection : System.Collections.ObjectModel.ObservableCollection<Region>
	{ 
	}
#endif
}
