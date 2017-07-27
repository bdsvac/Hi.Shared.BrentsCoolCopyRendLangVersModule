using Sitecore;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.SecurityModel;

namespace Hi.Shared.BrentsCoolCopyRendLangVersModule
{
    public static class ItemExtensions
    {
        public static bool IsNotNull(this Item i)
        {
            return i != null && i.Versions.Count > 0;
        }

        public static LayoutField GetLayoutField(this Item i)
        {
            if (i == null) return null;
            Field lf = i.Fields[FieldIDs.FinalLayoutField];
            return lf != null ? new LayoutField(lf) : null;
        }

        public static LayoutDefinition GetLayoutDefinition(this LayoutField f)
        {
            if (f != null)
            {
                return LayoutDefinition.Parse(f.Value);
            }
            return null;
        }

        public static DeviceDefinition GetDeviceDefinition(this LayoutDefinition d, string deviceId)
        {
            if (d != null)
            {
                return d.GetDevice(deviceId);
            }
            return null;
        }

        public static RenderingDefinition GetRenderingDefinitionByUniqueId(this Item i, string uniqueId, string deviceId)
        {
            DeviceDefinition dd = i.GetLayoutField().GetLayoutDefinition().GetDeviceDefinition(deviceId);
            if (dd != null)
            {
                return dd.GetRenderingByUniqueId(uniqueId);
            }
            return null;
        }

        public static void CopyRenderingReference(this Item i, RenderingDefinition rd, string deviceId)
        {
            var lf = i.GetLayoutField();
            var ld = lf.GetLayoutDefinition();
            var dd = ld.GetDeviceDefinition(deviceId);
            if (dd != null)
            {
                rd.UniqueId = string.Empty;
                dd.AddRendering(rd);
                using (new SecurityDisabler())
                {
                    i.Editing.BeginEdit();
                    lf.Value = ld.ToXml();
                    i.Editing.EndEdit();
                }
            }
        }
    }
}
