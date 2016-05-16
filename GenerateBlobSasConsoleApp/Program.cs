using System;

//Add WindowwsAzure.Storage Library from NuGet
//https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-shared-access-signature-part-1/
//https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-shared-access-signature-part-2/
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace GenerateBlobSasConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string uri = GenerateSasUri("somefilename.csv");
            Console.WriteLine(uri);
            Console.ReadLine();
        }

        private static string GenerateSasUri(string fileName)
        {
            //TODO Add this to the webjob app settings instead of hard coding this here.
            string storageAcctName = "";
            string storageAcctKey = ""; //storage account access key
            StorageCredentials storageAuth = new StorageCredentials(storageAcctName, storageAcctKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageAuth, true);

            //Create the blob client object.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container 
            CloudBlobContainer container = blobClient.GetContainerReference("attachments"); 

            //Get a reference to a blob within the container.
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

            //Set the expiry time and permissions for the blob.
            //In this case the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            //sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24); //Example - allow access for 24 hours
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddYears(100); //Example - allow access for 100 years
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read; //Example - allow read only access

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;

        }

    }
}
