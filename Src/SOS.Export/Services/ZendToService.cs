using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SOS.Export.Services.Interfaces;

namespace SOS.Export.Services
{
    public class ZendToService: IZendToService
    {
        /// <inheritdoc />
        public async Task<bool> SendFile(string filePath)
        {
            using var form = new MultipartFormDataContent();
            form.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            
            /*form.Add(new StringContent("Action"), "dropoff");
            form.Add(new StringContent("uname"), "jnka0004");
            form.Add(new StringContent("password"), "artdatabanken2020");
            form.Add(new StringContent("senderName"), "Artdatabanken");
            form.Add(new StringContent("senderEmail"), "noreply@artdatabanken.se");
            form.Add(new StringContent("senderOrganization"), "Artdatabanken");
            form.Add(new StringContent("subject"), "Fil från ADB");
            form.Add(new StringContent("note"), "Här kommer filen du beställde...");
            form.Add(new StringContent("confirmDelivery"), "0");
            form.Add(new StringContent("informRecipients"), "1");
            form.Add(new StringContent("informPasscode"), "0");
            form.Add(new StringContent("checksumFiles"), "0");
            //form.Add(new StringContent("encryptFiles"), "0");
            //form.Add(new StringContent("encryptPassword"), "0");
            form.Add(new StringContent("recipient_1"), "1");
            form.Add(new StringContent("recipName_1"), "Johan Karlsson");
            form.Add(new StringContent("recipEmail_1"), "johan.b.karlsson@slu.se");
            form.Add(new StringContent("desc_1"), "en beskrivning");
            */
              var dataContent = new FormUrlEncodedContent(new []
              {
                  new KeyValuePair<string, string>("Action", "dropoff"),
                  new KeyValuePair<string, string>("uname", "jnka0004"),
                  new KeyValuePair<string, string>("password", "artdatabanken2020"),
                  new KeyValuePair<string, string>("senderName", "Artdatabanken"),
                  new KeyValuePair<string, string>("senderEmail", "noreply@artdatabanken.se"),
                  new KeyValuePair<string, string>("senderOrganization", "Artdatabanken"),
                  new KeyValuePair<string, string>("subject", "Fil från ADB"),
                  new KeyValuePair<string, string>("note", "Här kommer filen du beställde..."),
                  new KeyValuePair<string, string>("confirmDelivery", "0"),
                  new KeyValuePair<string, string>("informRecipients", "1"),
                  new KeyValuePair<string, string>("informPasscode", "0"),
                  new KeyValuePair<string, string>("checksumFiles", "0"),
                  new KeyValuePair<string, string>("encryptFiles", "0"),
                  new KeyValuePair<string, string>("encryptPassword", null),
                  new KeyValuePair<string, string>("recipient_1", "1"),
                  new KeyValuePair<string, string>("recipName_1", "Kalle"),
                  new KeyValuePair<string, string>("recipEmail_1", "mats.lindgren@slu.se"),
                  new KeyValuePair<string, string>("desc_1", "en beskrivning")
              });

              dataContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

              form.Add(dataContent);

            /*using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "file_1", Path.GetFileName(filePath));
             */

            await using var fileStream = File.OpenRead(filePath);
              using var fileStreamContent = new StreamContent(fileStream);
              fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
              fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
              {
                  Name = "file_1",
                  FileName = Path.GetFileName(filePath)
              };

              form.Add(fileStreamContent);
             
            using var client = new HttpClient();
            using var response = await client.PostAsync("https://zendto.artdata.slu.se/dropoff.php", form);

            if (response.IsSuccessStatusCode)
            {
                var g = await response.Content.ReadAsStringAsync();
                return true;
            }

            return false;
        }
    }
}
