using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

public partial class _Default : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		MyLabel.Text = "Hello World from Azure, test version one";
	}

	protected void GoButton_Click(object sender, EventArgs e)
	{
		if (MyFileUpload.HasFile)
		{
			var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageAccountConnectionString"].ConnectionString);
			var blobClient = storageAccount.CreateCloudBlobClient();
			var containerReference = blobClient.GetContainerReference("firstcontainer"); // name of your container

			var blobReference = containerReference.GetBlockBlobReference(MyFileUpload.FileName);
			//Az tohle fakt pristupuje ke cloudu
			blobReference.UploadFromStream(MyFileUpload.FileContent);
		}
	}
}