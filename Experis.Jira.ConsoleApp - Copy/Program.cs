using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Experis.Jira.ConsoleApp
{
    class Program
    {
        static void Main1(string[] args)
        {
            //Console.WriteLine("JIRA TO EXCELL");
            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine("Enter the URL of JIRA Server");
            //string jiraURL = Console.ReadLine();
            //Console.WriteLine("Enter the Project  name");
            //string projectName = Console.ReadLine();
            //Console.WriteLine("Enter the Login/password info");
            //string loginInfo = Console.ReadLine();
            //Console.WriteLine("Enter the Date range ");
            //string dateRange = Console.ReadLine();
            //Console.WriteLine("Enter the CSV Save location ");
            //string csvlocation = Console.ReadLine();
            GetAllIssuesInfoFromJIRAByRest("", "");

        }

        public static async Task GetAllIssuesInfoFromJIRAByRest(string JiraURL, string ProjectName)
        {
            using (var client = new HttpClient())
            {
                // New code:
                client.BaseAddress = new Uri("https://jira.atlassian.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                try
                {
                    HttpResponseMessage response = await client.GetAsync("rest/api/latest/search?jql=project=ATLMVN");
                    if (response.IsSuccessStatusCode)
                    {
                        //Product product = await response.Content.ReadAsAsync > Product > ();
                        //Console.WriteLine("{0}\t${1}\t{2}", product.Name, product.Price, product.Category);
                    }

                }
                catch (Exception)
                {

                    throw;
                }            }
        }


    }


    
}
