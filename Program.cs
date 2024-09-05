using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;

class Program
{
     static string connectionString = "YOUR_CONNECTION_STRING";
     static string containerName = "CONTAINER-NAME";
     static CloudBlobContainer? container;

     static async Task Main(string[] args)
     {
          await InitializeStorageAsync();

          while (true)
          {
               DisplayMenu();
               string? choice = Console.ReadLine();

               switch (choice)
               {
                    case "1":
                         await UploadFileAsync();
                         break;
                    case "2":
                         await DownloadFileAsync();
                         break;
                    case "3":
                         await ListBlobsAsync();
                         break;
                    case "4":
                         await DeleteBlobAsync();
                         break;
                    case "5":
                         return;
                    default:
                         Console.WriteLine("Invalid choice. Please try again.");
                         break;
               }
          }
     }

     static void DisplayMenu()
     {
          Console.WriteLine("\nAzure Blob Storage Operations:");
          Console.WriteLine("1. Upload a file to Blob Storage");
          Console.WriteLine("2. Download a file from Blob Storage");
          Console.WriteLine("3. List all blobs in the container");
          Console.WriteLine("4. Delete a blob");
          Console.WriteLine("5. Exit");
          Console.Write("Enter your choice: ");
     }

     static async Task InitializeStorageAsync()
     {
          try
          {
               CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
               CloudBlobClient blobClient = account.CreateCloudBlobClient();
               container = blobClient.GetContainerReference(containerName);
               await container.CreateIfNotExistsAsync();
          }
          catch (Exception ex)
          {
               Console.WriteLine($"Error initializing storage: {ex.Message}");
               Environment.Exit(1);
          }
     }

     static async Task UploadFileAsync()
     {
          Console.Write("Enter the local file path to upload: ");
          string? localFilePath = Console.ReadLine();

          if (string.IsNullOrEmpty(localFilePath) || !File.Exists(localFilePath))
          {
               Console.WriteLine("File not found. Please check the path and try again.");
               return;
          }

          string blobName = Path.GetFileName(localFilePath);
          CloudBlockBlob blob = container!.GetBlockBlobReference(blobName);

          try
          {
               Console.WriteLine("Uploading file...");
               await TransferManager.UploadAsync(localFilePath, blob);
               Console.WriteLine("Upload complete!");
          }
          catch (Exception ex)
          {
               Console.WriteLine($"An error occurred during upload: {ex.Message}");
          }
     }

     static async Task DownloadFileAsync()
     {
          Console.Write("Enter the blob name to download: ");
          string? blobName = Console.ReadLine();

          if (string.IsNullOrEmpty(blobName))
          {
               Console.WriteLine("Invalid blob name. Please try again.");
               return;
          }

          CloudBlockBlob blob = container!.GetBlockBlobReference(blobName);

          if (!await blob.ExistsAsync())
          {
               Console.WriteLine("Blob not found. Please check the name and try again.");
               return;
          }

          string localFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), blobName);

          try
          {
               Console.WriteLine("Downloading file...");
               await TransferManager.DownloadAsync(blob, localFilePath);
               Console.WriteLine($"Download complete! File saved to: {localFilePath}");
          }
          catch (Exception ex)
          {
               Console.WriteLine($"An error occurred during download: {ex.Message}");
          }
     }

     static async Task ListBlobsAsync()
     {
          Console.WriteLine("Listing blobs in the container:");
          BlobContinuationToken? continuationToken = null;
          do
          {
               var results = await container!.ListBlobsSegmentedAsync(null, true, BlobListingDetails.None, 100, continuationToken, null, null);
               continuationToken = results.ContinuationToken;
               foreach (var item in results.Results)
               {
                    if (item is CloudBlockBlob blob)
                    {
                         Console.WriteLine(blob.Name);
                    }
               }
          } while (continuationToken != null);
     }

     static async Task DeleteBlobAsync()
     {
          Console.Write("Enter the blob name to delete: ");
          string? blobName = Console.ReadLine();

          if (string.IsNullOrEmpty(blobName))
          {
               Console.WriteLine("Invalid blob name. Please try again.");
               return;
          }

          CloudBlockBlob blob = container!.GetBlockBlobReference(blobName);

          if (!await blob.ExistsAsync())
          {
               Console.WriteLine("Blob not found. Please check the name and try again.");
               return;
          }

          try
          {
               Console.WriteLine("Deleting blob...");
               await blob.DeleteAsync();
               Console.WriteLine("Blob deleted successfully!");
          }
          catch (Exception ex)
          {
               Console.WriteLine($"An error occurred during deletion: {ex.Message}");
          }
     }
}
