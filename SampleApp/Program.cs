using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Octokit;
namespace SampleApp
{
    class Program
    {
        // Method to find a repository in an array of repositories based on the name 
        static Repository findRepository(string repoName, IReadOnlyList<Repository> repositories)
        {
            foreach (var repo in repositories)
            {
                if (repo.Name == repoName)
                {
                    return repo;
                }
            }
            return null;
        }

        static async Task Main(string[] args)
        {
            var client = new GitHubClient(new ProductHeaderValue("SampleApp"));
            //github.Check.Suite.Create()
            string tokenPath = @"C:\Windows\System\.env";

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

                Console.Write("Please, enter the name of the organization you want to find the PR in : ");
                string orgName = Console.ReadLine();

                var org = await client.Organization.Get(orgName);
                var repoOrg = await client.Repository.GetAllForOrg(org.Login);


                // If there are 0 repo in the org, then there's no point running this program
                if (repoOrg.Count == 0) 
                {
                    Console.WriteLine("The given organization has no repository in it, please create a repo before trying to use this program.");
                    Environment.Exit(0);
                }


                Console.Write("Please, enter the name of the repository you want to find the PR in (the repo has to be in your organization) : ");
                string repoName = Console.ReadLine();

                Repository repo = findRepository(repoName, repoOrg);

                // Keep asking for a valid input if we don't find any matching repo in the given org
                while (repo == null)
                {
                    Console.WriteLine("We couldn't find any matching repository in " + orgName + " organization, please re-enter you repository name.");
                    Console.WriteLine("If you wan't to leave the program, please type in 'Exit'");
                    string rName = Console.ReadLine();
                    if (rName.CompareTo("Exit") == 0)
                    {
                        Environment.Exit(0);
                    }
                    repo = findRepository(rName, repoOrg);
                }


                // TODO: Now find the given PR then create a check for it
                Console.WriteLine("repo " + repo.FullName);
                //client.Check.Suite.Create(user.Login,)


            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                Environment.Exit(0);
            }

        }
    }
}
