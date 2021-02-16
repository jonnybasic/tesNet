using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshopWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpfDagger.Properties;

namespace wpfDagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainContext Context
        {
            get { return DataContext as MainContext; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OptixCore.Library.NvRunTime.GetVersion(out int cudaMajor, out int cudaMinor);
            Context.StatusMessage = String.Format("CUDA {0}.{1}", cudaMajor, cudaMinor);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid caller = sender as Grid;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is MeshReader reader)
            {
                Context.Progress = 0.05f;
                Context.StatusMessage = "loading archive";
                if (reader.IsReady)
                {
                    Context.Progress = 0.10f;
                    Context.StatusMessage = String.Format("archive loaded {0} records", reader.Arch3d.Count);
                    List<RecordModel> tempList = new List<RecordModel>(reader.Arch3d.Count);
                    for (int i = 0; i < reader.Arch3d.Count; ++i)
                    {
                        uint recordId = reader.Arch3d.GetRecordId(i);
                        tempList.Add(new RecordModel
                        {
                            Id = recordId,
                            Index = i,
                            Name = recordId.ToString(),
                            SizeInBytes = 0
                        });
                    }
                    Context.Progress = 0.25f;
                    Context.Records = new ObservableCollection<RecordModel>(tempList);
                    Context.NotifyPropertyChanged("Records");
                    Context.Progress = 1.0f;
                }
                else
                {
                    Context.StatusMessage = String.Format("failed to load archive");
                    Context.Progress = 0.0f;
                }
            }
        }

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (e.Parameter is MeshReader reader);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 1)
            {
                // unload this model
                RecordModel record = e.RemovedItems[0] as RecordModel;
            }
            // selection mode is single
            foreach (RecordModel record in e.AddedItems)
            {
                // check if model is ready
                if (record.Model == null)
                {
                    // load this model
                    MeshReader reader = FindResource("DaggerMeshReader") as MeshReader;
                    if (reader.GetModelData(record.Id, out ModelData model))
                    {
                        // move the camera
                        record.CameraPosition = new Point3D(
                            -model.DFMesh.Radius,
                            model.DFMesh.Radius * 0.25f,
                            0.0f);
                        record.CameraLookDirection = new Vector3D(
                            -record.CameraPosition.X,
                            -record.CameraPosition.Y,
                            0.0f);
                        record.CameraUpDirection = new Vector3D(0, 1, 0);
                        // load model preview;
                        record.Model = reader.GetMesh(
                            record.Id,
                            out record._cachedMaterials,
                            out record._textureKeys,
                            out record._hasAnimations);
                        // expose the active texture list
                        record.Textures = new ObservableCollection<ImageSource>(
                            from src in record._cachedMaterials
                            select src.albedoMap);
                    }
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        { }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //TextBlock t;
            // destroy optix resources
            Context.Dispose();
        }
    }

    public abstract class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class RecordModel : ModelBase
    {
        private Model3D _model;
        private Vector3D _cameraUpDirection;
        private Vector3D _cameraLookDirection;
        private Point3D _cameraPosition;
        private ObservableCollection<ImageSource> _textures;

        internal CachedMaterial[] _cachedMaterials;
        internal int[] _textureKeys;
        internal bool _hasAnimations;

        public uint Id
        { get; set; }

        public int Index
        { get; set; }

        public string Name
        { get; set; }

        public UInt32 SizeInBytes
        { get; set; }

        public ObservableCollection<ImageSource> Textures
        {
            get => _textures;
            set
            {
                _textures = value;
                NotifyPropertyChanged("Textures");
            }
        }

        public Point3D CameraPosition
        {
            get => _cameraPosition;
            set
            {
                if (value != _cameraPosition)
                {
                    _cameraPosition = value;
                    NotifyPropertyChanged("CameraPosition");
                }
            }
        }

        public Vector3D CameraLookDirection
        {
            get => _cameraLookDirection;
            set
            {
                if (value != _cameraLookDirection)
                {
                    _cameraLookDirection = value;
                    NotifyPropertyChanged("CameraLookDirection");
                }
            }
        }

        public Vector3D CameraUpDirection
        {
            get => _cameraUpDirection;
            set
            {
                if (value != _cameraUpDirection)
                {
                    _cameraUpDirection = value;
                    NotifyPropertyChanged("CameraUpDirection");
                }
            }
        }

        public Model3D Model
        {
            get => _model;
            set
            {
                if (value != _model)
                {
                    _model = value;
                    NotifyPropertyChanged("Model");
                }
            }
        }

        public RecordModel()
        {
            Textures = new ObservableCollection<ImageSource>();
        }
    }

    public class RecordModelCollection : ObservableCollection<RecordModel>
    { }

    public class MainContext : ModelBase, IDisposable
    {
        //private OptixCore.Library.Context optixContext;
        //private OptixCore.Library.OptixBuffer outputBuffer;
        //private OptixCore.Library.OptixProgram rayGenProgram;
        //private WriteableBitmap outputImage;
        //private System.Numerics.Vector3 drawColor;
        private bool disposed;
        private bool showBackFace;
        private bool windingCCW;
        private float progress;
        private string statusMessage;

        public ObservableCollection<RecordModel> Records
        { get; set; }

        public bool ShowBackFace
        {
            get => showBackFace;
            set
            {
                if (value != showBackFace)
                {
                    showBackFace = value;
                    NotifyPropertyChanged("ShowBackFace");
                }
            }
        }

        public bool WindingCCW
        {
            get => windingCCW;
            set
            {
                if (value != windingCCW)
                {
                    windingCCW = value;
                    NotifyPropertyChanged("WindingCCW");
                }
            }
        }

        //public ImageSource ImageBuffer
        //{
        //    get { return outputImage; }
        //}

        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                if (value != statusMessage)
                {
                    statusMessage = value;
                    NotifyPropertyChanged("StatusMessage");
                }
            }
        }

        public float Progress
        {
            get => progress;
            set
            {
                if (value != progress)
                {
                    progress = value;
                    NotifyPropertyChanged("Progress");
                }
            }
        }

        public MainContext()
        {
            disposed = false;
            Records = new ObservableCollection<RecordModel>();
            //outputImage = new WriteableBitmap(512, 512, 96, 96, PixelFormats.Bgra32, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
            }
            disposed = true;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }

}
