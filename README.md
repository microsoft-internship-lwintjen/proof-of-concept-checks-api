# Checks API Playground
This is a simple C# console application that is used as a proof of concept to use the github checks api.

# Why ?

The purpose of this repository is to simulate the workflow of the Github checks api via a C# console application. We're using the Octokit package to make some API calls from C# to Github. Based on some inputs given by the user, we look at a pull request opened by the authenticated user and we create some checks for that pull request. Whenever, there's a new commit to the PR, we rerun the checks.  

We want to build this proof of concept to resolve [this issue](https://github.com/dotnet/arcade/issues/3326) in Dotnet/Arcade repository

# What do you need to use it?

- Create an organization
- Create a repository in that organization
- Create a new GitHub application and change the credentials in the `Program.cs` file with your github app credentials
- Install your github application in your organization
- Create a PR in that repository

You've now everything to start testing the program!

[Here's a demo of the app] : https://www.loom.com/share/32421811dd7f412db4d0074756ab536e
