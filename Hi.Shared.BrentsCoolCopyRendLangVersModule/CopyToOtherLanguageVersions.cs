using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi.Shared.BrentsCoolCopyRendLangVersModule
{
    [Serializable]
    public class CopyToOtherLanguageVersions : WebEditCommand
    {
        public CopyToOtherLanguageVersions()
        {
        }

        protected virtual string ConvertToXml(string layout)
        {
            Assert.ArgumentNotNull(layout, "layout");
            return WebEditUtil.ConvertJSONLayoutToXML(layout);
        }

        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            ItemUri itemUri = ItemUri.ParseQueryString();
            if (itemUri != null)
            {
                Item item = Database.GetItem(itemUri);
                if (item != null && !WebEditUtil.CanDesignItem(item))
                {
                    SheerResponse.Alert("The action cannot be executed because of security restrictions.", new string[0]);
                    return;
                }
            }

            string formValue = WebUtil.GetFormValue("scLayout");
            Assert.IsNotNullOrEmpty(formValue, "Layout Definition");
            string str = ShortID.Decode(WebUtil.GetFormValue("scDeviceID"));
            Assert.IsNotNullOrEmpty(str, "device ID");
            string str1 = ShortID.Decode(context.Parameters["uniqueId"]);
            Assert.IsNotNullOrEmpty(str1, "Unique ID");
            string xml = this.ConvertToXml(formValue);
            Assert.IsNotNullOrEmpty(xml, "convertedLayoutDefition");

            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection["deviceId"] = str;
            nameValueCollection["uniqueId"] = str1;
            nameValueCollection["contextItemUri"] = itemUri != null ? itemUri.ToString() : string.Empty;
            
            Context.ClientPage.Start(this, "Run", nameValueCollection);
        }

        [UsedImplicitly]
        protected void Run(ClientPipelineArgs args)
        {
            string deviceId;
            string uniqueId;
            string contextItemUri;

            Assert.ArgumentNotNull(args, "args");
            try
            {
                deviceId = args.Parameters["deviceId"];
                Assert.IsNotNull(deviceId, "deviceId");
                uniqueId = args.Parameters["uniqueId"];
                Assert.IsNotNull(uniqueId, "uniqueId");
                contextItemUri = args.Parameters["contextItemUri"];
                Assert.IsNotNull(contextItemUri, "contextItemUri");
            }
            catch
            {
                throw;
            }

            if (args.IsPostBack)
            {
                if (args.Result == "yes")
                {
                    TryCopy(deviceId, uniqueId, contextItemUri);
                }
            }
            else
            {
                try
                {
                    SheerResponse.Confirm("Are you sure you want to copy renderings to the most recent language versions?");
                    args.WaitForPostBack();
                }
                catch
                {
                    throw;
                }
            }
        }
        
        public void TryCopy(string deviceId, string uniqueId, string contextItemUri)
        {
            var i = Database.GetItem(new ItemUri(contextItemUri));
            var rd = i.GetRenderingDefinitionByUniqueId(uniqueId, deviceId);
            if (i != null && rd != null)
            {
                var list = i.Languages.Where(e => !e.CultureInfo.Name.Equals(i.Language.CultureInfo.Name)).ToList();
                foreach (var l in list)
                {
                    var li = i.Database.GetItem(i.ID, l);
                    if (li.IsNotNull())
                    {
                        li.CopyRenderingReference(rd, deviceId);
                    }
                }
            }
        }
    }
}
