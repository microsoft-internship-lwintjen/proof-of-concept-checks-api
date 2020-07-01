using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
namespace SampleApp
{
    class Program
    {


        static async Task Main(string[] args)
        {
            var client = new GitHubClient(new ProductHeaderValue("SampleApp"));
            //github.Check.Suite.Create()
            string tokenPath = @"C:\Windows\System\.env";

            string login = "lwintjen";

            DotNetEnv.Env.Load(tokenPath);
            try
            {
                string token = DotNetEnv.Env.GetString("API_GH_PERSONAL_TOKEN", "Variable not found");
                if (token.CompareTo("Variable not found") == 0)
                {
                    throw new FileNotFoundException("The github token was not found in your .env file, please make sure that the key 'API_GH_PERSONAL_TOKEN' is set in your .env file (C:\\Windows\\System\\.env)");
                }

                Console.WriteLine("Token found, setting up the token...");

                var tokenAuth = new Credentials(token); 
                client.Credentials = tokenAuth;

                Console.WriteLine("Finding the current authenticated user...");

                var user = await client.User.Current();

                var repoList = await client.Organization.GetAllForUser(login);

                for (int i = 0; i < repoList.Count; i++)
                {
                    Console.WriteLine(repoList.ElementAt(i));
                }
                Console.WriteLine("user : " + repoList);


            } catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                Environment.Exit(0);
            }

        }
    }
}
