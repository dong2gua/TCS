
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace ThorCyte.HeaderModule.ViewModels
{
    public class AnalysisViewModel : BindableBase
    {
        private ObservableCollection<string> _analysisList = new ObservableCollection<string>();
        private string _folerName;
        private string _experimentPath;
        private bool _isSaveWindow;

        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; } 

        public string SaveAnalysisPath { get; set; }
        public string WindowTile { get; set; }
        public string FolderName
        {
            set { SetProperty(ref _folerName, value); }
            get { return _folerName; }
        }

        public bool IsSaveWindow
        {
            set { SetProperty(ref _isSaveWindow, value); }
            get { return _isSaveWindow; }
        }

        public ObservableCollection<string> AnalysisList
        {get { return _analysisList; }}
 

        public AnalysisViewModel(string experimentPath, bool isSaveWindow)
        {
            IsSaveWindow = isSaveWindow;
            _experimentPath = experimentPath;
            _folerName = "";
            string path = experimentPath + "\\Analysis\\";
            if (Directory.Exists(path))
            {
                foreach (string subdirectory in Directory.GetDirectories(path))
                {
                    AnalysisList.Add(subdirectory.Remove(0, path.Length));
                }
            }
            
            OkCommand = new DelegateCommand<Window>(OnOk);
            CancelCommand = new DelegateCommand<Window>(OnCancel);

            if (IsSaveWindow)
                WindowTile = "Save Experiment Analysis Result";
            else
                WindowTile = "Load Experiment Analysis Result";
        }

        private void OnCancel(Window obj)
        {
            obj.Close();
        }

        private void OnOk(Window obj)
        {
            if (_isSaveWindow)
            {
                if (FolderName != "")
                {
                    SaveAnalysisPath = _experimentPath + "\\Analysis\\" + FolderName;
                    var di = new DirectoryInfo(SaveAnalysisPath);
                    if (!di.Exists)
                    {
                        di.Create();
                        obj.DialogResult = true;
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show(Application.Current.MainWindow, "Are you sure replace analysis result?", "Save analysis result", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        if (result == MessageBoxResult.Yes)
                        {
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (DirectoryInfo dir in di.GetDirectories())
                            {
                                dir.Delete(true);
                            }
                            obj.DialogResult = true;
                        }
                        
                    }
                    
                    obj.Close();
                }
                else
                {
                    MessageBox.Show(Application.Current.MainWindow,"Please input analysis folder", "Message", MessageBoxButton.OK, null);
                }

                //if (!Regex.Match(FolerName, @"^[0-9]+\s+([a-zA-Z]+|[a-zA-Z]+\s[a-zA-Z]+)$").Success)
                //{
                //    MessageBox.Show("Invalid address", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                //} 
            }
            else
            {
                if (FolderName != "")
                {
                    SaveAnalysisPath = _experimentPath + "\\Analysis\\" + FolderName;
                    obj.DialogResult = true;
                    obj.Close();
                }
                else 
                {
                    if (AnalysisList.Count > 0)
                    {
                        MessageBox.Show(Application.Current.MainWindow, "Please selected analysis folder", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        obj.Close();
                    }
                }
            }
            
        }

    }
}
