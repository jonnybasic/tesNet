using DaggerfallWorkshop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace wpfDagger
{
    public class RecordModelValueConverter : IValueConverter
    {
        static System.Windows.DependencyObject designerCheck = new System.Windows.DependencyObject();

        public DaggerfallUnity DaggerfallUnity
        { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(designerCheck))
            {
                App.SetupUnityEngine();
            }

            // check incoming value
            if (value is RecordModel record
                && DaggerfallUnity != null)
            {
                // check target type
                if (targetType == typeof(Model3D))
                {
                    //try
                    {
                        // load this model
                        if (record.ModelCache is null
                            && DaggerfallUnity.MeshReader.GetModelData(record.Id, out record._modelData))
                        {
                            record.CameraUpDirection = new Vector3D(0, 1, 0);
                            // load model preview;
                            record._mesh = DaggerfallUnity.MeshReader.GetMesh(
                                DaggerfallUnity,
                                record.Id,
                                out record._cachedMaterials,
                                out record._textureKeys,
                                out record._hasAnimations);
                            // get albedo textures
                            record.Textures = new ObservableCollection<ImageSource>(
                                record._cachedMaterials.Select<CachedMaterial, ImageSource>((cm) => cm.albedoMap));
                            // get emissive textures
                            foreach (ImageSource t in record._cachedMaterials.Where((cm) => cm.emissionMap != null).Select((cm) => cm.emissionMap))
                            {
                                record.Textures.Add(t);
                            }
                            // update model
                            record.SetModelCache(
                                record._mesh.GetMeshProxy(
                                    record._cachedMaterials.Select((cm) => cm.material).ToArray()));
                        }
                        // move the camera
                        record.CameraPosition = new Point3D(
                            -record._modelData.DFMesh.Radius,
                            record._modelData.DFMesh.Radius * 0.25f,
                            0.0f);
                        record.CameraLookDirection = new Vector3D(
                            -record.CameraPosition.X,
                            -record.CameraPosition.Y,
                            0.0f);
                        return record.ModelCache;
                    }
                    //catch (Exception e)
                    //{
                    //    // HACK: Designer Mode is Throwing tring to load the INI parser library
                    //    //       Is there a way the designer mode can load assemblies
                    //    System.Diagnostics.Debug.Print(e.Message);
                    //    throw e;
                    //    return null;
                    //}
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
